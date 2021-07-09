namespace RUtil.Debug.Shell
{
    public static class PathConstants
    {
        public const string Root                  = "/";
        public const string Home                  = "~";
        public const string CurrentDir            = ".";
        public const string ParentDir             = "..";
        public const char   Separator             = '/';
        public static readonly string CurrentRelativePrefix = CurrentDir + Separator;
        public static readonly string ParentRelativePrefix  = ParentDir + Separator;
        public static readonly string HomeRelativePrefix    = Home + Separator;
    }
}
