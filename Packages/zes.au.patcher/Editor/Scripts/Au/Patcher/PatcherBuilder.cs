﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Au.Patcher
{
    public static class PatcherBuilder
    {
        /// <summary>
        /// when true, use the first 8 hash code.
        /// </summary>
        public static bool useShortHash = true;

        /// <summary>
        /// output pretty print json
        /// </summary>
        public static bool prettyPrint = true;

        /// <summary>
        /// Build patcher
        /// </summary>
        /// <param name="version"></param>
        /// <param name="url"></param>
        /// <param name="minVersion"></param>
        /// <param name="bundlesDir"></param>
        /// <param name="copyToStreaming"></param>
        public static void Build(
            string version,
            string url,
            string minVersion,
            string bundlesDir)
        {
            string versionInfoPath = Path.Combine(bundlesDir, Constants.versionInfoFile);
            var versionInfo = new VersionInfo
            {
                version = version,
                url = url,
                minVersion = minVersion,
            };
            SaveJson(versionInfo, versionInfoPath);

            string patchInfoPath = Path.Combine(bundlesDir, Constants.patchInfoFile);
            var patchInfo = new PatchInfo
            {
                version = version,
                url = url,
                files = CalcBundleHash(bundlesDir),
            };
            SaveJson(patchInfo, patchInfoPath);

            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            File.Copy(versionInfoPath, Path.Combine(Application.streamingAssetsPath, Constants.versionInfoFile), true);
            File.Copy(patchInfoPath, Path.Combine(Application.streamingAssetsPath, Constants.patchInfoFile), true);

            //if (copyToStreaming)
            //{
            //    var bundles = GetBundles(bundlesDir);
            //    bundles.ToList().ForEach(i =>
            //    {
            //        File.Copy(Path.Combine(bundlesDir, i), Path.Combine(Application.streamingAssetsPath, i), true);
            //    });
            //}
        }

        /// <summary>
        /// Calc md5 for all bundle in outputPath dir
        /// </summary>
        /// <param name="bundlesDir"></param>
        /// <returns></returns>
        public static PatchFileInfo[] CalcBundleHash(string bundlesDir)
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

        private static void SaveJson(object obj, string path)
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
