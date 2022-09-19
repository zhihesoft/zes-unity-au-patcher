using System;

namespace Au.Patcher
{
    [Serializable]
    public class PatchFileInfo
    {
        public string path;
        public string md5;
        public int size;
    }
}
