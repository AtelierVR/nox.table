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
		public string Key { get; internal set; }

        [JsonProperty("mime")]
		public string Mime { get; internal set; }

        [JsonProperty("hash", ItemConverterType = typeof(HexaToBytes))]
		public byte[] Hash { get; internal set; }

        [JsonProperty("updated_at", ItemConverterType = typeof(UnixTimestampToDateTime))]
        public DateTime UpdatedAt { get; internal set; }

        [JsonProperty("created_at", ItemConverterType = typeof(UnixTimestampToDateTime))]
        public DateTime CreatedAt { get; internal set; }

        [JsonIgnore]
        public bool Local { get; internal set; }
    }
}