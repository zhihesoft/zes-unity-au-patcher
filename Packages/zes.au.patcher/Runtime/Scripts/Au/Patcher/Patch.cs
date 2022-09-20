using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Au.Patcher
{
    public class Patch
    {
        private VersionInfo localVersionInfo;
        private PatchFileInfo[] patchFileInfos;
        private string patchDir;

        public async Task<CheckResult> Check(string patchDir)
        {
            this.patchDir = patchDir;
            var streamingJson = await Utils.LoadLocalText(Path.Combine(Application.streamingAssetsPath, Constants.versionInfoFile));
            if (string.IsNullOrEmpty(streamingJson))
            {
                DLog.Error($"cannot find version info in streaming assets fold");
                return CheckResult.Failed;
            }
            var stramingVersionInfo = JsonUtility.FromJson<VersionInfo>(streamingJson);

            localVersionInfo = await LoadLocalJson<VersionInfo>(patchDir, Constants.versionInfoFile);
            if (localVersionInfo == null || Utils.CompareVersion(stramingVersionInfo.version, localVersionInfo.version) > 0)
            {
                await Extract(patchDir);
            }

            var remoteVersionInfo = await LoadRemoteJson<VersionInfo>(localVersionInfo.url, Constants.versionInfoFile);
            if (Utils.CompareVersion(localVersionInfo.version, remoteVersionInfo.minVersion) < 0)
            {
                return CheckResult.Reinstall;
            }

            var compare = Utils.CompareVersion(localVersionInfo.version, remoteVersionInfo.version);

            if (compare == 0)
            {
                return CheckResult.None;
            }

            if (compare > 0)
            {
                DLog.Warn($"local ({localVersionInfo.version}) is great than remote ({remoteVersionInfo.version})");
                await Extract(patchDir);
                return CheckResult.None;
            }

            var localPatchInfo = await LoadLocalJson<PatchInfo>(patchDir, Constants.patchInfoFile);
            var remotePatchInfo = await LoadRemoteJson<PatchInfo>(localVersionInfo.url, Constants.patchInfoFile);

            var localDic = localPatchInfo.files.ToDictionary(i => i.path, i => i);
            var remoteDic = remotePatchInfo.files.ToDictionary(i => i.path, i => i);

            patchFileInfos = remoteDic
                .Where(r => !localDic.ContainsKey(r.Key) || localDic[r.Key].md5 != r.Value.md5)
                .Select(i => i.Value).ToArray();

            return CheckResult.Found;
        }

        public async Task<bool> Apply(Action<float> progress)
        {
            if (patchFileInfos == null || patchFileInfos.Length <= 0)
            {
                DLog.Warn($"no patch files found");
                progress?.Invoke(1);
                return true;
            }

            var totalSize = patchFileInfos.Sum(i => i.size);
            var baseUrl = localVersionInfo.url;
            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
            }

            float downloadSize = 0;
            foreach (var item in patchFileInfos)
            {
                await DownloadFile(baseUrl + item.path,
                    Path.Combine(Application.persistentDataPath, patchDir, item.path),
                    (prog) =>
                    {
                        progress?.Invoke((prog * item.size + downloadSize) / totalSize);
                    });
                downloadSize += item.size;
                progress?.Invoke(downloadSize / totalSize);
            }
            progress?.Invoke(1);
            return true;
        }

        private async Task<bool> Extract(string patchDir)
        {
            string fullpath = Path.Combine(Application.persistentDataPath, patchDir);
            if (Directory.Exists(fullpath))
            {
                Directory.Delete(fullpath, true);
            }
            Directory.CreateDirectory(fullpath);

            File.Copy(
                Path.Combine(Application.streamingAssetsPath, Constants.versionInfoFile),
                Path.Combine(Application.persistentDataPath, patchDir, Constants.versionInfoFile),
                true);
            File.Copy(
                Path.Combine(Application.streamingAssetsPath, Constants.patchInfoFile),
                Path.Combine(Application.persistentDataPath, patchDir, Constants.patchInfoFile),
                true);
            localVersionInfo = await LoadLocalJson<VersionInfo>(patchDir, Constants.versionInfoFile);
            await Task.Yield();
            return true;
        }

        private async Task<T> LoadLocalJson<T>(string patchDir, string filename) where T : class
        {
            string fullpath = Path.Combine(Application.persistentDataPath, patchDir, filename);
            if (!File.Exists(fullpath))
            {
                return null;
            }
            string json = await Utils.LoadLocalText(fullpath);
            return JsonUtility.FromJson<T>(json);
        }

        private async Task<T> LoadRemoteJson<T>(string baseUrl, string filename) where T : class
        {
            var json = await Utils.LoadRemoteText(baseUrl, filename);
            return JsonUtility.FromJson<T>(json);
        }

        private async Task<bool> DownloadFile(string url, string targetPath, Action<float> progress)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                www.downloadHandler = new DownloadHandlerFile(targetPath);
                var webreq = www.SendWebRequest();
                while (!webreq.isDone)
                {
                    progress?.Invoke(webreq.progress);
                    await Task.Yield();
                }
                progress?.Invoke(1);

                if (www.result != UnityWebRequest.Result.Success)
                {
                    DLog.Error($"Download {www.url} failed: \n({www.responseCode}) {www.error})");
                    return false;
                }

                return www.result == UnityWebRequest.Result.Success;
            }

        }

    }
}
