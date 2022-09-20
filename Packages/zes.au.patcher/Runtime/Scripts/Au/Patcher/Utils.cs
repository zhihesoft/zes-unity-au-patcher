using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Au.Patcher
{
    internal static class Utils
    {
        public readonly static Encoding utf8WithoutBOM = new UTF8Encoding(false);

        /// <summary>
        /// Compare two version
        /// if v1<v2 return -1
        /// if v1=v2 return 0
        /// if v1>v2 return 1
        /// </summary>
        public static int CompareVersion(string version1, string version2)
        {

            if (string.IsNullOrEmpty(version1) || string.IsNullOrEmpty(version2))
            {
                return 1;
            }

            string[] v1 = version1.Split('.');
            string[] v2 = version2.Split('.');
            for (int i = 0; i < v1.Length; i++)
            {
                if (i >= v2.Length)
                {
                    return 1;
                }

                int.TryParse(v1[i], out int a);
                int.TryParse(v2[i], out int b);
                if (a != b)
                {
                    return a - b;
                }
            }
            if (v2.Length > v1.Length)
            {
                return -1;
            }
            return 0;
        }


        public static async Task<string> LoadLocalText(string path)
        {
            string localfilePath = Path.Combine(Application.persistentDataPath, path);
            if (!File.Exists(localfilePath))
            {
                DLog.Error($"{localfilePath} not existed");
                return null;
            }

            var json = await File.ReadAllTextAsync(localfilePath, utf8WithoutBOM);
            return json;
        }

        public static async Task<string> LoadRemoteText(string baseUrl, string filePath)
        {
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl + "/";
            }

            if (filePath.StartsWith("/"))
            {
                filePath = filePath.Substring(1);
            }

            string url = baseUrl + filePath;

            using (var www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                var op = www.SendWebRequest();
                while (!op.isDone)
                {
                    await Task.Yield();
                    continue;
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    DLog.Error($"Get {www.url} failed: \n({www.responseCode}) {www.error})");
                    int code = (int)(www.responseCode == 0 ? 500 : www.responseCode);
                    return string.Empty;
                }
                return www.downloadHandler.text;
            }

        }

    }
}
