using System.Text;
using Nox.CCK.Utils;
using Nox.Tables;

namespace Nox.Table.Runtime {
	public class Entry : IEntry {
		public Entry(string key, byte[] value, string mime, Identifier user) {
			Key     = key;
			AsBytes = value;
			Mime    = mime;
			User    = user;
		}

		public Identifier User { get; }
		public string Key { get; }
		public string Mime { get; }
		public byte[] AsBytes { get; }

		public string AsString
			=> Encoding.UTF8.GetString(AsBytes);
	}
}