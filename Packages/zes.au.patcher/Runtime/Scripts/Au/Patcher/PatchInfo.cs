using System;

namespace Au.Patcher
{
    [Serializable]
    public class PatchInfo
    {
        public string version;
        public string url;
        public PatchFileInfo[] files;
    }
}
