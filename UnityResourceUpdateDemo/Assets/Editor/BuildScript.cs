using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildScript {

    [MenuItem("Tools/Build Assetbundles", false)]
    public static void BuildActivePlatformAssetbundles()
    {
        BuildAssetbundles(EditorUserBuildSettings.activeBuildTarget);
    }

    public static void BuildAssetbundles(BuildTarget targetPlatform)
    {
        string inputPath = "Assets/Data";
        string outputPath = "Assets/StreamingAssets/" + GetPlatformName(Application.platform);

        if (Directory.Exists(outputPath) == false)
            Directory.CreateDirectory(outputPath);
        List<AssetBundleBuild> builds = CollectAssetbundleBuilds(inputPath, outputPath);
        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle | 
                                            BuildAssetBundleOptions.ChunkBasedCompression | 
                                            BuildAssetBundleOptions.DisableWriteTypeTree;
        BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), options, targetPlatform);
    }

    private static List<AssetBundleBuild> CollectAssetbundleBuilds(string inputPath, string outputPath)
    {
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        CollectSingleFileBuilds(inputPath, "Atlas/", "*.prefab", SearchOption.AllDirectories, outputPath, builds);
        CollectSingleFileBuilds(inputPath, "Config/", "*.text", SearchOption.TopDirectoryOnly, outputPath, builds);
        CollectSingleFileBuilds(inputPath, "Shader/", "*.shader", SearchOption.TopDirectoryOnly, outputPath, builds);

        return builds;
    }

    private static void CollectSingleFileBuilds(string rootPath, string searchSubPath, string filePattern, SearchOption searchOption, string outputPath, List<AssetBundleBuild> builds)
    {
        string[] files = Directory.GetFiles(rootPath + searchSubPath, filePattern, searchOption);
        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i].ToLower();
            string filePath2 = string.Empty;
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = filePath.Substring(rootPath.Length, filePath.Length - rootPath.Length) + ".ab";
            if (filePattern == "*.shader" && filePath.Contains("standardinst"))
            {
                build.assetNames = new string[2];
                build.assetNames[0] = filePath2;
                build.assetNames[1] = filePath;
            }
            else
            {
                build.assetNames = new string[1];
                build.assetNames[0] = filePath;
            }
            builds.Add(build);
        }
    }

    public static string GetPlatformName(RuntimePlatform platform)
    {
        switch (platform)
        {
            case RuntimePlatform.Android:
                return "android";
            case RuntimePlatform.IPhonePlayer:
                return "ios";
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                return "windows";
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return "osx";
            default:
                return "others";
        }
    }

}
