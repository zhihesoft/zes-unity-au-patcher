using System;
using System.IO;
using UnityEngine;

namespace Au.Patcher
{
    [Serializable]
    public class VersionInfo
    {
        public string version;
        public string url;
        public string minVersion;

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static VersionInfo FromJson(string json)
        {
            return JsonUtility.FromJson<VersionInfo>(json);
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
