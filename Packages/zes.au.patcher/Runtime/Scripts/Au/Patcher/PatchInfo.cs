using System;
using System.IO;
using UnityEngine;

namespace Au.Patcher
{
    namespace Au.Patcher
    {
        [Serializable]
        public class PatchInfo
        {
            public string version;
            public string url;
            public PatchFileInfo[] files;

            public string ToJson()
            {
                return JsonUtility.ToJson(this);
            }

            public static PatchInfo FromJson(string json)
            {
                return JsonUtility.FromJson<PatchInfo>(json);
            }

            public void Save(string path)
            {
                using (StreamWriter writer = new StreamWriter(path, false, Utils.utf8WithoutBOM))
                {
                    writer.Write(ToJson());
                }
            }
        }
    }
}
