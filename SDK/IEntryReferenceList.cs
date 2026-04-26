using System;
using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;

namespace Nox.Tables
{
    /// <summary>
    /// Represents a paginated response containing references to entries in the table.
    /// </summary>
    public interface IEntryReferenceList
    {
        /// <summary>
        /// Gets the total number of entries matching the query.
        /// </summary>
        public uint Total { get; }

        /// <summary>
        /// Gets the offset of the current page of results.
        /// </summary>
        public uint Offset { get; }

        /// <summary>
        /// Gets the limit of the number of entries per page.
        /// </summary>
        public uint Limit { get; }

        /// <summary>
        /// Gets the entries matching the query.
        /// </summary>
        public IEntryReference[] Items { get; }

        /// <summary>
        /// Determines whether there is a next page of results.
        /// </summary>
        public bool HasNext();

        /// <summary>
        /// Determines whether there is a previous page of results.
        /// </summary>
        public bool HasPrevious();

        /// <summary>
        /// Gets the next page of results.
        /// </summary>
        public UniTask<IEntryReferenceList> Next();

        /// <summary> 
        /// Gets the previous page of results.
        /// </summary>
        public UniTask<IEntryReferenceList> Previous();
    }
}