using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Nox.Tables;

namespace Nox.Table.Runtime
{
    [Serializable]
    public class EntryReferenceList : IEntryReferenceList
    {
        [JsonProperty("total")]
        public uint Total { get; internal set; }

        [JsonProperty("offset")]
        public uint Offset { get; internal set; }

        [JsonProperty("limit")]
        public uint Limit { get; internal set; }

        [JsonProperty("items")]
        public EntryReference[] Items { get; internal set; }

        [JsonIgnore]
        public bool Local { get; internal set; }

        IEntryReference[] IEntryReferenceList.Items
            => Items.ToArray<IEntryReference>();

        public bool HasNext()
            => Offset + Limit < Total;
        
        public bool HasPrevious()
            => Offset > Limit;

        public UniTask<EntryReferenceList> Next()
            => HasNext()
                ? Main.Instance.List(Offset + Limit, Limit, Local)
                : default;

        async UniTask<IEntryReferenceList> IEntryReferenceList.Next()
            => await Next();

        public UniTask<EntryReferenceList> Previous()
            => HasPrevious()
                ? Main.Instance.List(Offset - Limit, Limit, Local)
                : default;

        async UniTask<IEntryReferenceList> IEntryReferenceList.Previous()
            => await Previous();
    }
}