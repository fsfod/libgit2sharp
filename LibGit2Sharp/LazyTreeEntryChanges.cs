using System.Diagnostics;
using System.Globalization;
using LibGit2Sharp.Core;
using System.Runtime.InteropServices;
using System;

namespace LibGit2Sharp
{
    /// <summary>
    /// Holds the changes between two versions of a tree entry.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public unsafe struct LazyTreeEntryChanges
    {
        private GitDiffDeltaUnsafe* delta;

        internal unsafe LazyTreeEntryChanges(GitDiffDeltaUnsafe* delta)
        {
            this.delta = delta;
        }

        /// <summary>
        /// The new Size.
        /// </summary>
        public long Size => delta->NewFile.Size;

        /// <summary>
        /// The new path.
        /// </summary>
        public string Path => LaxUtf8Marshaler.FromNative(delta->NewFile.Path);

        /// <summary>
        /// The new <see cref="Mode"/>.
        /// </summary>
        public Mode Mode => (Mode)delta->NewFile.Mode;

        /// <summary>
        /// The new content hash.
        /// </summary>
        public ObjectId Oid => new ObjectId(RawOid);
        
        /// <summary>
        /// The new raw content hash.
        /// </summary>
        public byte[] RawOid {
            get {
                var id = new byte[GitOid.Size];
                Marshal.Copy((IntPtr)delta->NewFile.Id, id, 0, GitOid.Size);
                return id;
            }
        }

        /// <summary>
        /// The kind of change that has been done (added, deleted, modified ...).
        /// </summary>
        public ChangeKind Status => delta->Status;

        /// <summary>
        /// The old path.
        /// </summary>
        public string OldPath => LaxUtf8Marshaler.FromNative(delta->OldFile.Path);

        /// <summary>
        /// Returns whether the old path pointer points to the same string as the new path <seealso cref="OldPathIfRenamed"/>
        /// </summary>
        public bool PathChanged => delta->NewFile.Path != delta->OldFile.Path;

        /// <summary>
        /// Returns the old path if it is different from the current path otherwise returns null
        /// </summary>
        public string OldPathIfRenamed => PathChanged ? OldPath : null;

        /// <summary>
        /// The old <see cref="Mode"/>.
        /// </summary>
        public Mode OldMode => (Mode)delta->OldFile.Mode;

        /// <summary>
        /// The old content hash.
        /// </summary>
        public ObjectId OldOid => new ObjectId(RawOldOid);
        
        /// <summary>
        /// The raw old content hash.
        /// </summary>
        public byte[] RawOldOid
        {
            get
            {
                var id = new byte[GitOid.Size];
                Marshal.Copy((IntPtr)delta->OldFile.Id, id, 0, GitOid.Size);
                return id;
            }
        }

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
