using System;

namespace Au.Patcher
{
    [Serializable]
    public class PatchInfo
    {
        public string app;
        public string version;
        public PatchFileInfo[] files;
    }
}
