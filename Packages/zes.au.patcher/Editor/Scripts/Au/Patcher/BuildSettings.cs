using System.IO;
using UnityEngine;

namespace Au.Patcher
{
    /// <summary>
    /// Build Settings
    /// </summary>
    public class BuildSettings
    {
        /// <summary>
        /// Application id
        /// </summary>
        public string app;
        /// <summary>
        /// Current version
        /// </summary>
        public string version;
        /// <summary>
        /// Patch download url
        /// </summary>
        public string url;
        /// <summary>
        /// Minimun version
        /// </summary>
        public string minVersion;
        /// <summary>
        /// Bundles directory
        /// </summary>
        public string bundlesDir;
        /// <summary>
        /// Use short hash in patch file info
        /// </summary>
        public bool shortHash = true;
        /// <summary>
        /// Use pretty print json format
        /// </summary>
        public bool prettyPrint = true;

        /// <summary>
        /// Create new version info based on settings
        /// </summary>
        /// <returns></returns>
        public VersionInfo CreateVersionInfo()
        {
            var versionInfo = new VersionInfo
            {
                app = app,
                version = version,
                url = url,
                minVersion = minVersion,
            };
            return versionInfo;
        }

        public bool Validate()
        {
            if (!Directory.Exists(bundlesDir))
            {
                Debug.LogError($"BuildSettings: AssetBundles directory ({bundlesDir}) not exist");
                return false;
            }

            if (string.IsNullOrEmpty(version))
            {
                Debug.LogError($"BuildSettings: version cannot be null");
                return false;
            }

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError($"BuildSettings: url cannot be null");
                return false;
            }

            if (string.IsNullOrEmpty(app))
            {
                Debug.LogError($"BuildSettings: app cannot be null");
                return false;
            }

            return true;
        }
    }
}
