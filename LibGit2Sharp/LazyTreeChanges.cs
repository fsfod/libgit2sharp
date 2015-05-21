using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp
{
    /// <summary>
    /// Holds the result of a diff between two trees.
    /// <para>To obtain the actual patch of the diff, use the <see cref="Patch"/> class when calling Compare.</para>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LazyTreeChanges : IEnumerable<LazyTreeEntryChanges>
    {
        List<LazyTreeEntryChanges> changes = new List<LazyTreeEntryChanges>();

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected LazyTreeChanges()
        { }

        internal LazyTreeChanges(DiffSafeHandle diff)
        {
            Proxy.git_diff_foreach2(diff, FileCallback);
        }

        private unsafe int FileCallback(IntPtr deltaPtr, float progress, IntPtr payload)
        {
            var delta = (GitDiffDeltaUnsafe*)deltaPtr;

            if (delta->Status == ChangeKind.Unmodified && delta->OldFile.Path == delta->NewFile.Path)
            {
                return 0;
            }

            var treeEntryChanges = new LazyTreeEntryChanges(delta);

            changes.Add(treeEntryChanges);
            return 0;
        }


        #region IEnumerable<TreeEntryChanges> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.</returns>
        public virtual IEnumerator<LazyTreeEntryChanges> GetEnumerator()
        {
            return changes.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "+{0} ~{1} -{2} \u00B1{3} R{4} C{5}",
                    changes.Where(c => c.Status == ChangeKind.Added).Count(),
                    changes.Where(c => c.Status == ChangeKind.Modified).Count(),
                    changes.Where(c => c.Status == ChangeKind.Deleted).Count(),
                    changes.Where(c => c.Status == ChangeKind.TypeChanged).Count(),
                    changes.Where(c => c.Status == ChangeKind.Renamed).Count(),
                    changes.Where(c => c.Status == ChangeKind.Copied).Count());
            }
        }
    }
}
