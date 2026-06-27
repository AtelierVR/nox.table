using System;
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
		/// Gets the MIME type of the value.
		/// </summary>
		public string Mime { get; }

		/// <summary>
		/// Gets the last modification time (UTC).
		/// </summary>
		public DateTime UpdatedAt { get; }

		/// <summary>
		/// Gets the creation time (UTC).
		/// </summary>
		public DateTime CreatedAt { get; }

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