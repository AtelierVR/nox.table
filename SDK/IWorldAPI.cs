using Cysharp.Threading.Tasks;

namespace Nox.Tables {
	public interface ITableAPI {
		/// <summary>
		/// Gets the entry from the table by key.
		/// If the entry does not exist, it will return null.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public UniTask<IEntry> Get(string key);

		/// <summary>
		/// Sets the entry in the table with the specified key and value.
		/// If the entry already exists, it will be overwritten.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="mime"></param>
		/// <returns></returns>
		public UniTask<IEntry> Set(string key, byte[] value, string mime = "application/octet-stream");

		/// <summary>
		/// Sets the entry in the table with the specified key and value.
		/// If the entry already exists, it will be overwritten.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="mime"></param>
		/// <returns></returns>
		public UniTask<IEntry> Set(string key, string value, string mime = "text/plain");

		/// <summary>
		/// Deletes the entry from the table by key.
		/// On you current account.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public UniTask<bool> Delete(string key);
	}
}