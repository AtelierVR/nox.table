using System;
using System.Linq;
using System.Threading.Tasks;
using Nox.CCK.Scripting;
using Nox.Scripting;
using Nox.Sessions;
using Nox.Tables;

namespace Nox.Table.Runtime.Modules {
	/// <summary>
	/// Scripting module <c>"tables"</c> — session-scoped persistent storage.
	/// <code>
	/// import { getPrivate, setPrivate, getPublic, setPublic } from 'tables';
	/// const data = await getPrivate();
	/// </code>
	/// </summary>
	public static class TablesModule {
		internal static ITableAPI TableAPI { get; set; }

		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("tables")
				.WithTags("session")
				.AddAsyncMethod("getPublic",  async (ctx, _) => await GetTable(ctx.Session, EntryType.Public))
				.AddAsyncMethod("getPrivate", async (ctx, _) => await GetTable(ctx.Session, EntryType.Private))
				.AddAsyncMethod("getLocal",  async (ctx, _) => await GetTable(ctx.Session, EntryType.Local))
				.AddAsyncMethod("setPublic",  async (ctx, args) => await SetTable(ctx.Session, EntryType.Public, args))
				.AddAsyncMethod("setPrivate", async (ctx, args) => await SetTable(ctx.Session, EntryType.Private, args))
				.AddAsyncMethod("setLocal",  async (ctx, args) => await SetTable(ctx.Session, EntryType.Local, args))
				.AddAsyncMethod("delPrivate", async (ctx, _) => await DeleteTable(ctx.Session, EntryType.Private))
				.AddAsyncMethod("delPublic",  async (ctx, _) => await DeleteTable(ctx.Session, EntryType.Public))
				.AddAsyncMethod("delLocal",  async (ctx, _) => await DeleteTable(ctx.Session, EntryType.Local))
				.Build();

		private static bool TryTableKey(ISession session, EntryType entryType, out string key) {
			if (session?.Dimensions == null) { key = null; return false; }
			var id = session.Dimensions.Identifier;
			if (!id.IsValid()) { key = null; return false; }
			var userServer = Main.UserAPI?.Current?.Identifier.Server;
			var sameServer = id.IsLocal() || (userServer != null && id.Server == userServer);
			key = $"{(entryType == EntryType.Public ? "public." : "")}worlds.{id.ToShortString(withServer: !sameServer)}";
			return true;
		}

		private static async Task<byte[]> GetTable(ISession session, EntryType entryType) {
			if (!TryTableKey(session, entryType, out var key)) 
				return null;
			var entry = await TableAPI.Get(
				key,
				local: entryType == EntryType.Local
			);
			return entry?.AsBytes;
		}

		private static async Task<byte[]> SetTable(ISession session, EntryType entryType, object[] args) {
			if (args.Length == 0 || !TryTableKey(session, entryType, out var key)) return null;
			byte[] data;
			if (args[0] is byte[] b)
				data = b;
			else if (args[0] is object[] arr)
				data = arr.Select(Convert.ToByte).ToArray();
			else
				return null;
			var entry = await TableAPI.Set(
				key, 
				data, 
				"application/octet-stream+world", 
				local: entryType == EntryType.Local
			);
			return entry?.AsBytes;
		}

		private static async Task<bool> DeleteTable(ISession session, EntryType entryType) {
			if (!TryTableKey(session, entryType, out var key)) 
				return false;
			return await TableAPI.Delete(
				key, 
				local: entryType == EntryType.Local
			);
		}
	}
}
