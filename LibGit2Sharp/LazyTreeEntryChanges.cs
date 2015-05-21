using System.Diagnostics;
using System.Globalization;
using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    /// <summary>
    /// Holds the changes between two versions of a tree entry.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LazyTreeEntryChanges
    {
       // private GitDiffDelta delta;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected LazyTreeEntryChanges()
        { }

        internal unsafe LazyTreeEntryChanges(GitDiffDeltaUnsafe* delta)
        {
            Path = LaxUtf8Marshaler.FromNative(delta->NewFile.Path);

            Mode = (Mode)delta->NewFile.Mode;
            OldMode = (Mode)delta->OldFile.Mode;

            //Check if we can skip 
            if (delta->OldFile.Path != delta->NewFile.Path)
            {
                OldPath = LaxUtf8Marshaler.FromNative(delta->OldFile.Path);
            }
            else
            {
                OldPath = Path;
            }

            if (delta->Status == ChangeKind.Untracked || delta->Status == ChangeKind.Ignored)
            {
                Status = ChangeKind.Added;
            }
            else
            {
                Status = delta->Status;
            }
        }

        /// <summary>
        /// The new path.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// The new <see cref="Mode"/>.
        /// </summary>
        public Mode Mode { get; }

        /// <summary>
        /// The new content hash.
        /// </summary>
        //public ObjectId Oid { get; }

        /// <summary>
        /// The kind of change that has been done (added, deleted, modified ...).
        /// </summary>
        public ChangeKind Status { get; }

        /// <summary>
        /// The old path.
        /// </summary>
        public string OldPath { get; }

        /// <summary>
        /// The old <see cref="Mode"/>.
        /// </summary>
        public Mode OldMode { get; }

        /// <summary>
        /// The old content hash.
        /// </summary>
        //public ObjectId OldOid => delta.OldFile.Id;

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "Path = {0}, File {1}",
                    !string.IsNullOrEmpty(Path) ? Path : OldPath, Status);
            }
        }
    }
}
