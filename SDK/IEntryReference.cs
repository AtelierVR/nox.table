using System;
using Nox.CCK.Utils;

namespace Nox.Tables {
	/// <summary>
	/// Represents a reference to an entry in the table.
	/// </summary>
	public interface IEntryReference {
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
		/// Gets the hash of the value.
		/// </summary>
		public byte[] Hash { get; }

		/// <summary>
		/// Datetime of the last update of the entry.
		/// </summary>
		public DateTime UpdatedAt { get; }

		/// <summary>
		/// Datetime of the creation of the entry.
		/// </summary>
		public DateTime CreatedAt { get; }
	}
}