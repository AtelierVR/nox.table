using Nox.CCK.Utils;

namespace Nox.Tables {
	/// <summary>
	/// Represents an entry in the table.
	/// </summary>
	public interface IEntry {
		/// <summary>
		/// Gets the user who owns the entry.
		/// </summary>
		public Identifier User { get; }

		/// <summary>
		/// Gets the key.
		/// </summary>
		/// <returns></returns>
		public string Key { get; }

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <returns></returns>
		public byte[] AsBytes { get; }

		/// <summary>
		/// Gets the value as a string.
		/// </summary>
		public string AsString { get; }
	}
}