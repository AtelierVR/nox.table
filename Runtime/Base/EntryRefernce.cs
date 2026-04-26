using System;
using Newtonsoft.Json;
using Nox.CCK.Convertors;
using Nox.CCK.Utils;
using Nox.Tables;

namespace Nox.Table.Runtime {
    [Serializable]
	public class EntryReference : IEntryReference {
		public Identifier User { get; internal set; }

        [JsonProperty("key")]
		public string Key { get; private set; }

        [JsonProperty("mime")]
		public string Mime { get; private set; }

        [JsonProperty("hash", ItemConverterType = typeof(HexaToBytes))]
		public byte[] Hash { get; private set; }

        [JsonProperty("updated_at", ItemConverterType = typeof(UnixTimestampToDateTime))]
        public DateTime UpdatedAt { get; private set; }

        [JsonProperty("created_at", ItemConverterType = typeof(UnixTimestampToDateTime))]
        public DateTime CreatedAt { get; private set; }
    }
}