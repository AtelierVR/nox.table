using Cysharp.Threading.Tasks;

namespace Nox.Tables {
	public interface ITableAPI {
		/// <summary>
		/// Gets the entry from the table by key.
		/// If the entry does not exist, it will return null.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="local">When <c>true</c>, read from the local filesystem backend.</param>
		/// <returns></returns>
		public UniTask<IEntry> Get(string key, bool local = false);

		/// <summary>
		/// Sets the entry in the table with the specified key and value.
		/// If the entry already exists, it will be overwritten.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="mime"></param>
		/// <param name="local">When <c>true</c>, write to the local filesystem backend.</param>
		/// <returns></returns>
		public UniTask<IEntry> Set(string key, byte[] value, string mime = "application/octet-stream", bool local = false);

		/// <summary>
		/// Sets the entry in the table with the specified key and value.
		/// If the entry already exists, it will be overwritten.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="mime"></param>
		/// <param name="local">When <c>true</c>, write to the local filesystem backend.</param>
		/// <returns></returns>
		public UniTask<IEntry> Set(string key, string value, string mime = "text/plain", bool local = false);

		/// <summary>
		/// Deletes the entry from the table by key.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="local">When <c>true</c>, delete from the local filesystem backend.</param>
		/// <returns></returns>
		public UniTask<bool> Delete(string key, bool local = false);

		/// <summary>
		/// Lists of Tables entries for the current user.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="limit"></param>
		/// <param name="local">When <c>true</c>, list from the local filesystem backend.</param>
		/// <returns></returns>
		public UniTask<IEntryReferenceList> List(uint offset = 0, uint limit = 50, bool local = false);
	}
}