using Cysharp.Threading.Tasks;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.CCK.Scripting;
using Nox.Network;
using Nox.Scripting;
using Nox.Table.Runtime.Modules;
using Nox.Tables;
using Nox.Users;
using IEntry = Nox.Tables.IEntry;

namespace Nox.Table.Runtime {
	public class Main : IMainModInitializer, ITableAPI {
		#region Variables

		static internal Main       Instance;
		internal        IModCoreAPI CoreAPI;

		static internal INetworkAPI NetworkAPI
			=> Instance.CoreAPI.ModAPI
				.GetMod("network")
				?.GetInstance<INetworkAPI>();

		static internal IUserAPI UserAPI
			=> Instance.CoreAPI.ModAPI
				.GetMod("users")
				?.GetInstance<IUserAPI>();

		internal Network Network;

		#endregion

		#region ModInitializer

		public void OnInitialize(IModCoreAPI api) {
			CoreAPI  = api;
			Instance = this;
			Network  = new Network();

			TablesModule.TableAPI = this;
			var scripting = api.ModAPI.GetMod("scripting")?.GetInstance<IScriptingAPI>();
			scripting?.RegisterModule(TablesModule.Module);
		}

		public void OnDispose() {
			var scripting = CoreAPI?.ModAPI.GetMod("scripting")?.GetInstance<IScriptingAPI>();
			scripting?.UnregisterModule(new NameResolver("tables"));

			TablesModule.TableAPI = null;
			CoreAPI  = null;
			Instance = null;
		}

		#endregion

		public async UniTask<IEntry> Get(string key)
			=> await Network.Get(key);

		public async UniTask<IEntry> Set(string key, byte[] value, string mime = "application/octet-stream")
			=> await Network.Set(key, value);
		
		public async UniTask<IEntry> Set(string key, string value, string mime = "text/plain")
			=> await Network.Set(key, value);

		public async UniTask<bool> Delete(string key)
			=> await Network.Delete(key);
		
		public async UniTask<IEntryReferenceList> List(uint offset = 0, uint limit = 50)
			=> await Network.List(offset, limit);
	}
}