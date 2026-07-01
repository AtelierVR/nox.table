using System;
using System.Linq;
using Nox.CCK.Scripting;
using Nox.Scripting;
using Nox.Sessions;
using Nox.Tables;
using Nox.Users;

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
				.AddAsyncMethod("getPrivate", async (ctx, _) => (object)await GetTable(ctx.Session, false))
				.AddAsyncMethod("getPublic",  async (ctx, _) => (object)await GetTable(ctx.Session, true))
				.AddAsyncMethod("setPrivate", async (ctx, args) => (object)await SetTable(ctx.Session, false, args))
				.AddAsyncMethod("setPublic",  async (ctx, args) => (object)await SetTable(ctx.Session, true, args))
				.AddAsyncMethod("delPrivate", async (ctx, _) => (object)await DeleteTable(ctx.Session, false))
				.AddAsyncMethod("delPublic",  async (ctx, _) => (object)await DeleteTable(ctx.Session, true))
				.Build();

		private static bool TryTableKey(ISession session, bool isPublic, out string key) {
			if (session?.Dimensions == null) { key = null; return false; }
			var id = session.Dimensions.Identifier;
			if (!id.IsValid()) { key = null; return false; }
			var userServer = Main.UserAPI?.Current?.Identifier.Server;
			var sameServer = id.IsLocal() || (userServer != null && id.Server == userServer);
			key = $"{(isPublic ? "public." : "")}worlds.{id.ToShortString(withServer: !sameServer)}";
			return true;
		}

		private static async System.Threading.Tasks.Task<byte[]> GetTable(ISession session, bool isPublic) {
			if (!TryTableKey(session, isPublic, out var key)) return null;
			var entry = await TableAPI.Get(key);
			return entry?.AsBytes;
		}

		private static async System.Threading.Tasks.Task<byte[]> SetTable(ISession session, bool isPublic, object[] args) {
			if (args.Length == 0 || !TryTableKey(session, isPublic, out var key)) return null;
			byte[] data;
			if (args[0] is byte[] b)
				data = b;
			else if (args[0] is object[] arr)
				data = arr.Select(Convert.ToByte).ToArray();
			else
				return null;
			var entry = await TableAPI.Set(key, data, "application/octet-stream+world");
			return entry?.AsBytes;
		}

		private static async System.Threading.Tasks.Task<bool> DeleteTable(ISession session, bool isPublic) {
			if (!TryTableKey(session, isPublic, out var key)) return false;
			return await TableAPI.Delete(key);
		}
	}
}
