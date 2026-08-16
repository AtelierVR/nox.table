using Cysharp.Threading.Tasks;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
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
		internal Local   Local;

		#endregion

		#region ModInitializer

		public void OnInitialize(IModCoreAPI api) {
			CoreAPI  = api;
			Instance = this;
			Network  = new Network();
			Local    = new Local(CoreAPI.ConfigAPI.GetFolder());

			TablesModule.TableAPI = this;
			var scripting = api.ModAPI.GetMod("scripting")?.GetInstance<IScriptingAPI>();
			scripting?.RegisterModule(TablesModule.Module);
		}

		public void OnDispose() {
			var scripting = CoreAPI?.ModAPI.GetMod("scripting")?.GetInstance<IScriptingAPI>();
			scripting?.UnregisterModule(TablesModule.Module);

			TablesModule.TableAPI = null;
			Network  = null;
			Local    = null;
			CoreAPI  = null;
			Instance = null;
		}

		#endregion

		public async UniTask<IEntry> Get(string key, bool local = false)
			=> local 
			? await Local.Get(key) 
			: await Network.Get(key);

		public async UniTask<IEntry> Set(string key, byte[] value, string mime = "application/octet-stream", bool local = false)
			=> local 
			? await Local.Set(key, value, mime) 
			: await Network.Set(key, value, mime);
		
		public async UniTask<IEntry> Set(string key, string value, string mime = "text/plain", bool local = false)
			=> local 
			? await Local.Set(key, value, mime) 
			: await Network.Set(key, value, mime);

		public async UniTask<bool> Delete(string key, bool local = false)
			=> local 
			? await Local.Delete(key) 
			: await Network.Delete(key);
		
		public async UniTask<EntryReferenceList> List(uint offset = 0, uint limit = 50, bool local = false)
			=> local 
			? await Local.List(offset, limit) 
			: await Network.List(offset, limit);

		async UniTask<IEntryReferenceList> ITableAPI.List(uint offset = 0, uint limit = 50, bool local = false)
			=> await List(offset, limit, local);
	}
}