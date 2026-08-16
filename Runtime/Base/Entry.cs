using System;
using System.Text;
using Nox.CCK.Utils;
using Nox.Tables;

namespace Nox.Table.Runtime {
	public class Entry : IEntry {
		public Entry(string key, byte[] value, string mime, Identifier user, DateTime updatedAt, DateTime createdAt, bool local) {
			Key       = key;
			AsBytes   = value;
			Mime      = mime;
			User      = user;
			UpdatedAt = updatedAt;
			CreatedAt = createdAt;
			Local     = local;
		}

		public Identifier User { get; }
		public string Key { get; }
		public string Mime { get; }
		public bool Local { get; }
		public DateTime UpdatedAt { get; }
		public DateTime CreatedAt { get; }
		public byte[] AsBytes { get; }

		public string AsString
			=> Encoding.UTF8.GetString(AsBytes);
	}
}