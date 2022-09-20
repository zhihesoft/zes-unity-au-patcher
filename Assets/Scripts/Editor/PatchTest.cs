using Au.Patcher;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;

public static class PatchTest
{
    [MenuItem("Test/BuildPatch")]
    public async static void BuildPatch()
    {
        BuildBundles();
        await Task.Delay(1);
        PatcherBuilder.Build("1.0.0", "http://test.com/android", "0.0.0", "AssetBundles");
    }

    private static void BuildBundles()
    {
        var outputDir = "AssetBundles";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        BuildPipeline.BuildAssetBundles(outputDir, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }

}
