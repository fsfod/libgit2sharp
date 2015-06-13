using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    /// <summary>
    /// 
    /// </summary>
    public class LibGit2Options
    {
        /// <summary>
        /// 
        /// </summary>
        public static int CacheMaxSize
        {
            get
            {
                int maxStorage = 0, currentStorage = 0;

                var res = NativeMethods.git_libgit2_opts(LibGit2Opts.GET_CACHED_MEMORY, __arglist(ref currentStorage, ref maxStorage));
                Ensure.ZeroResult(res);

                return maxStorage;
            }

            set
            {
                var res = NativeMethods.git_libgit2_opts(LibGit2Opts.SET_CACHE_MAX_SIZE, __arglist(value));
                Ensure.ZeroResult(res);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int CacheMemoryUsed
        {
            get
            {
                int currentStorage = 0, maxStorage = 0;

                var res = NativeMethods.git_libgit2_opts(LibGit2Opts.GET_CACHED_MEMORY, __arglist(ref currentStorage, ref maxStorage));
                Ensure.ZeroResult(res);

                return currentStorage;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static uint MappedWindowLimit
        {
            get
            {
                uint windowLimit;

                var res = NativeMethods.git_libgit2_opts(LibGit2Opts.GET_MWINDOW_MAPPED_LIMIT, __arglist(out windowLimit));
                Ensure.ZeroResult(res);

                return windowLimit;
            }

            set
            {
                var res = NativeMethods.git_libgit2_opts(LibGit2Opts.SET_MWINDOW_MAPPED_LIMIT, __arglist(value));
                Ensure.ZeroResult(res);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="sizeLimit"></param>
        public static void SetCacheObjectSizeLimit(ObjectType objType, int sizeLimit)
        {
            var res = NativeMethods.git_libgit2_opts(LibGit2Opts.SET_CACHE_OBJECT_LIMIT, __arglist(objType.ToGitObjectType(), sizeLimit));
            Ensure.ZeroResult(res);
        }

    }
}
