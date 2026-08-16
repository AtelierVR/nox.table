using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.CCK.Utils;
using UnityEngine.Events;

namespace Nox.Table.Runtime {
	/// <summary>
	/// Local (filesystem) storage backend for tables.
	/// <para>
	/// Stores each entry under <c>&lt;ConfigAPI.GetFolder()&gt;/tables/</c> as a single
	/// file named <c>&lt;id_hash&gt;</c> (SHA-256 of the key, no extension), whose content is
	/// <c>&lt;header JSON&gt;\n&lt;content bytes&gt;</c>. The header holds the original key,
	/// mime, content hash and timestamps. The first <c>\n</c> byte separates header from content.
	/// </para>
	/// <para>
	/// Mirrors the <see cref="Network"/> backend API so it can be swapped in
	/// transparently when tables are stored locally.
	/// </para>
	/// </summary>
	public class Local {
		private readonly string _root;
		private readonly UnityEvent<Entry> _getEvent = new();
		private readonly UnityEvent<Entry> _setEvent = new();
		private readonly UnityEvent<string, Identifier> _deleteEvent = new();

		public Local(string root) {
			_root = Path.Combine(root, "tables");
			Directory.CreateDirectory(_root);
		}

		private void InvokeGet(Entry entry) {
			if (entry == null)
				return;
			_getEvent.Invoke(entry);
			Main.Instance?.CoreAPI?.EventAPI.Emit("table_get", entry);
		}

		private void InvokeSet(Entry entry) {
			if (entry == null)
				return;
			_setEvent.Invoke(entry);
			Main.Instance?.CoreAPI?.EventAPI.Emit("table_set", entry);
			InvokeGet(entry);
		}

		private void InvokeDelete(string key, Identifier user) {
			_deleteEvent.Invoke(key, user);
			Main.Instance?.CoreAPI?.EventAPI.Emit("table_delete", key, user);
		}

		#region Path helpers

		private static string IdHash(string key)
			=> Hashing.Hash(key);

		private string FilePath(string key)
			=> Path.Combine(_root, IdHash(key));

		private const string Sep = "\n";

		#endregion

		#region Metadata

		private sealed class Meta {
			[JsonProperty("key")]        public string Key       { get; set; }
			[JsonProperty("mime")]       public string Mime      { get; set; }
			[JsonProperty("hash")]       public string Hash      { get; set; }
			[JsonProperty("updated_at")] public long   UpdatedAt { get; set; }
			[JsonProperty("created_at")] public long   CreatedAt { get; set; }
		}

		private static long ToUnixMs(DateTime dt)
			=> new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeMilliseconds();

		private static DateTime FromUnixMs(long ms)
			=> DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime;

		#endregion

		private Identifier CurrentUser
			=> Main.UserAPI?.Current?.Identifier ?? Identifier.Invalid;

		public UniTask<Entry> Get(string key) {
			try {
				var path = FilePath(key);
				if (!File.Exists(path))
					return UniTask.FromResult<Entry>(null);

				var raw = File.ReadAllBytes(path);
				var split = Array.IndexOf(raw, Sep);
				if (split < 0)
					return UniTask.FromResult<Entry>(null);

				var meta = JsonConvert.DeserializeObject<Meta>(Encoding.UTF8.GetString(raw, 0, split));

				var content = new byte[raw.Length - split - 1];
				Array.Copy(raw, split + 1, content, 0, content.Length);

				var entry = new Entry(
					meta.Key ?? key,
					content,
					meta.Mime ?? "application/octet-stream",
					CurrentUser,
					FromUnixMs(meta.UpdatedAt),
					FromUnixMs(meta.CreatedAt),
					local: true
				);

				InvokeGet(entry);
				return UniTask.FromResult(entry);
			} catch (Exception e) {
				Logger.LogError($"Failed to read local table {key}: {e.Message}");
				return UniTask.FromResult<Entry>(null);
			}
		}

		public UniTask<Entry> Set(string key, string value, string mime = "text/plain")
			=> Set(key, Encoding.UTF8.GetBytes(value), mime);

		public UniTask<Entry> Set(string key, byte[] value, string mime = "application/octet-stream") {
			try {
				var now = DateTime.UtcNow;
				value ??= Array.Empty<byte>();

				var path = FilePath(key);
				long createdAt;
				if (File.Exists(path)) {
					var existing = ReadMeta(path);
					createdAt = existing?.CreatedAt ?? ToUnixMs(now);
				} else {
					createdAt = ToUnixMs(now);
				}

				var meta = new Meta {
					Key       = key,
					Mime      = mime,
					Hash      = Hashing.HashBytes(value),
					UpdatedAt = ToUnixMs(now),
					CreatedAt = createdAt,

				};

				var header = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(meta));
				using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
					fs.Write(header, 0, header.Length);
					fs.Write(Encoding.UTF8.GetBytes(Sep), 0, Sep.Length);
					fs.Write(value, 0, value.Length);
				}

				var entry = new Entry(
					key,
					value,
					mime,
					CurrentUser,
					now,
					FromUnixMs(createdAt),
					local: true
				);

				InvokeSet(entry);
				return UniTask.FromResult(entry);
			} catch (Exception e) {
				Logger.LogError($"Failed to write local table {key}: {e.Message}");
				return UniTask.FromResult<Entry>(null);
			}
		}

		public UniTask<bool> Delete(string key) {
			try {
				var path = FilePath(key);
				if (!File.Exists(path))
					return UniTask.FromResult(false);

				File.Delete(path);

				InvokeDelete(key, CurrentUser);
				return UniTask.FromResult(true);
			} catch (Exception e) {
				Logger.LogError($"Failed to delete local table {key}: {e.Message}");
				return UniTask.FromResult(false);
			}
		}

		public UniTask<EntryReferenceList> List(uint offset = 0, uint limit = 50) {
			try {
				var entries = Directory.GetFiles(_root)
					.Select(ReadMeta)
					.Where(x => x != null)
					.Select(x => new EntryReference {
						User      = CurrentUser,
						Key       = x.Key,
						Mime      = x.Mime,
						Hash      = HexToBytes(x.Hash),
						UpdatedAt = FromUnixMs(x.UpdatedAt),
						CreatedAt = FromUnixMs(x.CreatedAt),
						Local     = true
					})
					.OrderBy(r => r.Key, StringComparer.OrdinalIgnoreCase)
					.ToArray();

				uint total = (uint)entries.Length;
				var page = entries.Skip((int)offset).Take((int)limit).ToArray();

				var list = new EntryReferenceList {
					Total  = total,
					Offset = offset,
					Limit  = limit,
					Items  = page,
					Local  = true,
				};
				return UniTask.FromResult(list);
			} catch (Exception e) {
				Logger.LogError($"Failed to list local tables: {e.Message}");
				return UniTask.FromResult<EntryReferenceList>(null);
			}
		}

		#region File read helpers

		private static Meta ReadMeta(string path) {
			try {
				var raw = File.ReadAllBytes(path);
				var split = Array.IndexOf(raw, Encoding.UTF8.GetBytes(Sep));
				if (split < 0)
					return null;

				return JsonConvert.DeserializeObject<Meta>(Encoding.UTF8.GetString(raw, 0, split));
			} catch {
				return null;
			}
		}

		private static byte[] HexToBytes(string hex) {
			if (string.IsNullOrEmpty(hex))
				return Array.Empty<byte>();
			var bytes = new byte[hex.Length / 2];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
			return bytes;
		}

		#endregion
	}
}
