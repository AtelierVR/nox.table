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
        public uint Total { get; private set; }

        [JsonProperty("offset")]
        public uint Offset { get; private set; }

        [JsonProperty("limit")]
        public uint Limit { get; private set; }

        [JsonProperty("items")]
        public EntryReference[] Items { get; private set; }

        IEntryReference[] IEntryReferenceList.Items
            => Items.ToArray<IEntryReference>();

        public bool HasNext()
            => Offset + Limit < Total;
        
        public bool HasPrevious()
            => Offset > Limit;

        public UniTask<EntryReferenceList> Next()
            => HasNext()
                ? Main.Instance.Network.List(Offset + Limit, Limit)
                : default;

        async UniTask<IEntryReferenceList> IEntryReferenceList.Next()
            => await Next();

        public UniTask<EntryReferenceList> Previous()
            => HasPrevious()
                ? Main.Instance.Network.List(Offset - Limit, Limit)
                : default;

        async UniTask<IEntryReferenceList> IEntryReferenceList.Previous()
            => await Previous();
    }
}