using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Au.Patcher
{
    /// <summary>
    /// Patch Builder
    /// </summary>
    public static class PatcherBuilder
    {
        /// <summary>
        /// Build patcher
        /// </summary>
        /// <param name="settings">Build settings</param>
        public static void Build(BuildSettings settings)
        {
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Validate());

            string versionInfoPath = Path.Combine(settings.bundlesDir, Constants.versionInfoFile);
            var versionInfo = settings.CreateVersionInfo();
            SaveJson(versionInfo, versionInfoPath, settings.prettyPrint);

            string patchInfoPath = Path.Combine(settings.bundlesDir, Constants.patchInfoFile);
            var patchInfo = new PatchInfo
            {
                app = settings.app,
                version = versionInfo.version,
                files = CalcBundleHash(settings.bundlesDir, settings.shortHash),
            };
            SaveJson(patchInfo, patchInfoPath, settings.prettyPrint);


            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.Copy(versionInfoPath, Path.Combine(Application.streamingAssetsPath, Constants.versionInfoFile), true);
            File.Copy(patchInfoPath, Path.Combine(Application.streamingAssetsPath, Constants.patchInfoFile), true);
        }

        /// <summary>
        /// Calc md5 for all bundle in outputPath dir
        /// </summary>
        /// <param name="bundlesDir"></param>
        /// <returns></returns>
        private static PatchFileInfo[] CalcBundleHash(string bundlesDir, bool useShortHash)
        {
            AssetBundle.UnloadAllAssetBundles(true);

            var bundles = GetBundles(bundlesDir);

            var ret = bundles.Select(i =>
            {
                string path = Path.Combine(bundlesDir, i);
                var item = AssetBundle.LoadFromFile(path);
                string[] assets = item.isStreamedSceneAssetBundle ? item.GetAllScenePaths() : item.GetAllAssetNames();
                item.Unload(true);

                string md5sum = "";
                int size = (int)new FileInfo(path).Length;
                md5sum = assets.AsParallel().Aggregate("", (last, value) => CalcAssetMD5(value) + last);
                md5sum = CalcMD5(md5sum);
                if (useShortHash)
                {
                    md5sum = md5sum.Substring(0, 8);
                }
                return new PatchFileInfo
                {
                    size = size,
                    path = i,
                    md5 = md5sum,
                }; // CalcMD5(md5sum);
            });

            AssetBundle.UnloadAllAssetBundles(true);
            return ret.ToArray();
        }

        private static string[] GetBundles(string bundlesDir)
        {
            DirectoryInfo man = new DirectoryInfo(bundlesDir);
            Assert.IsTrue(man.Exists);
            string allbundlepath = Path.Combine(bundlesDir, man.Name);
            var allbundle = AssetBundle.LoadFromFile(allbundlepath);
            var manifest = allbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            var bundles = manifest.GetAllAssetBundles();
            allbundle.Unload(true);
            return bundles;
        }

        private static string CalcAssetMD5(string path)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var m1 = CalcFileMD5(path);
            var m2 = CalcFileMD5(path + ".meta");
            return CalcMD5(m1 + m2);
        }

        private static string CalcFileMD5(string path)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bytes = md5.ComputeHash(fs);
                var ret = BitConverter.ToString(bytes).ToLower().Replace("-", "");
                return ret;
            }
        }

        private static string CalcMD5(string text)
        {
            return CalcMD5(Encoding.UTF8.GetBytes(text));
        }

        private static string CalcMD5(byte[] bytes)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var md5bytes = md5.ComputeHash(bytes);
            var ret = BitConverter.ToString(md5bytes).ToLower().Replace("-", "");
            return ret;
        }

        private static void SaveJson(object obj, string path, bool prettyPrint)
        {
            Encoding utf8WithoutBOM = new UTF8Encoding(false);
            using (StreamWriter writer = new StreamWriter(path, false, utf8WithoutBOM))
            {
                var json = JsonUtility.ToJson(obj, prettyPrint);
                writer.Write(json);
            }
        }
    }
}
