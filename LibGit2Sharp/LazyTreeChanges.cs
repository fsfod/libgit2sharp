using System;
using System.Diagnostics;
using System.Globalization;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Handles;
using System.Collections.Generic;

namespace LibGit2Sharp
{
    /// <summary>
    /// Holds the result of a diff between two trees.
    /// <para>To obtain the actual patch of the diff, use the <see cref="Patch"/> class when calling Compare.</para>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LazyTreeChanges : IDisposable   
    {
        private DiffSafeHandle diff;

        internal LazyTreeChanges(DiffSafeHandle diff)
        {
            this.diff = diff;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAction">Function to invoke on each change</param>
        /// <param name="data"></param>
        public unsafe void ProcessChanges<T>(Action<LazyTreeEntryChanges, T> userAction, T data)
        {
            int count = Count;

            for (int i = 0; i < count; i++)
            {
                userAction(new LazyTreeEntryChanges((GitDiffDeltaUnsafe*)Proxy.git_diff_get_delta(diff, i)), data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="userAction"></param>
        /// <returns></returns>
        public unsafe T[] TransformChanges<T>(Func<LazyTreeEntryChanges, T> userAction)
        {
            int count = Count;

            var resultCount = count - UnmodifiedCount;
            
            //Don't waste time walking the delta list if theres no unmodified deltas
            if (resultCount == 0)
            {
                return Array.Empty<T>();
            }

            var result = new T[resultCount];

            for (int i = 0, j = 0; i < count ; i++)
            {
                var delta = (GitDiffDeltaUnsafe*)Proxy.git_diff_get_delta(diff, i);

                if (delta->Status == ChangeKind.Unmodified)
                {
                    continue;
                }

                result[j++] = userAction(new LazyTreeEntryChanges(delta));

                // we can exit early if we've got all the non unmodified deltas
                if (j == resultCount)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// The number of changed files in this diff
        /// </summary>
        public int Count => Proxy.git_diff_num_deltas(diff);
        
        /// <summary>
        /// Unmodified change entry count
        /// </summary>
        public int UnmodifiedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Unmodified);

        /// <summary>
        /// Added change entry count
        /// </summary>
        public int AddedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Added);

        /// <summary>
        /// Deleted change entry count
        /// </summary>
        public int DeletedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Deleted);

        /// <summary>
        /// Modified change entry count
        /// </summary>
        public int ModifiedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Modified);

        /// <summary>
        /// Modified change entry count
        /// </summary>
        public int RenamedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Renamed);

        /// <summary>
        /// Modified change entry count
        /// </summary>
        public int CopiedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.Copied);

        /// <summary>
        /// Modified change entry count
        /// </summary>
        public int TypeChangedCount => Proxy.git_diff_num_deltas_of_type(diff, ChangeKind.TypeChanged);

        private string DebuggerDisplay
        {
            get
            {
                var change = new ChangeCount(diff);

                return string.Format(CultureInfo.InvariantCulture,
                    "+{0} ~{1} -{2} \u00B1{3} R{4} C{5}",
                    AddedCount,
                    ModifiedCount,
                    DeletedCount,
                    TypeChangedCount,
                    RenamedCount,
                    CopiedCount);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

                diff.Close();

                disposedValue = true;
            }
        }


        ~LazyTreeChanges()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    class ChangeCount
    {
        internal int Added, Modified, Deleted, TypeChanged, Renamed, Copied;

        public ChangeCount(DiffSafeHandle diff)
        {
            Proxy.git_diff_foreach2(diff, FileCallback);
        }

        private unsafe int FileCallback(IntPtr deltaPtr, float progress, IntPtr payload)
        {
            var delta = (GitDiffDeltaUnsafe*)deltaPtr;

            switch (delta->Status)
            {
                case ChangeKind.Added:
                    Added++;
                    break;
                case ChangeKind.Deleted:
                    Deleted++;
                    break;
                case ChangeKind.Modified:
                    Modified++;
                    break;
                case ChangeKind.Renamed:
                    Renamed++;
                    break;
                case ChangeKind.Copied:
                    Copied++;
                    break;
                case ChangeKind.TypeChanged:
                    TypeChanged++;
                    break;
                default:
                    break;
            }

            return 0;
        }
    }
}
