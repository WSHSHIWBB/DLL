using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildAssetBundles :Editor
{
    private static BuildTarget platformTarget;
    private static string versionFile = null;
    [MenuItem("Tools/AssetBundle/BuildWindowsAssetBundles")]
    public static void BuildWindowsAssetBundles()
    {
        platformTarget = BuildTarget.StandaloneWindows;
        versionFile = "EditorVersion.txt";
        DoBuildAssetBundles();
    }

    [MenuItem("Tools/AssetBundle/BuildAndroidAssetBundles")]
    public static void BuildAndroidAssetBundles()
    {
        platformTarget = BuildTarget.Android;
        versionFile = "AndroidVersion.txt";
        DoBuildAssetBundles();
    }

    [MenuItem("Tools/AssetBundle/BuildIOSAssetBundles")]
    public static void BuildIOSAssetBundles()
    {
        platformTarget = BuildTarget.iOS;
        versionFile = "IOSVersion.txt";
        DoBuildAssetBundles();
    }

    public static void DoBuildAssetBundles()
    {
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/AssetBundles", BuildAssetBundleOptions.None,platformTarget);
 
        string newFile = Application.dataPath + "/AssetBundles/" + versionFile;
        if (File.Exists(newFile))
        {
            File.Delete(newFile);
        }

        string[] files = Directory.GetFiles(Application.dataPath + "/AssetBundles", "*", SearchOption.AllDirectories);
        FileStream fs = new FileStream(newFile, FileMode.CreateNew);
        StreamWriter version = new StreamWriter(fs);
        for(int i=0;i<files.Length;++i)
        {
            string fileParth = files[i];
            if (fileParth.EndsWith(".meta") || fileParth.EndsWith(".manifest"))
            {
                continue;
            }
            else
            {
                string name = fileParth.Replace(Application.dataPath + "/AssetBundles\\", "");
                Debug.Log(name);  
                string md5 = MD5Hash.GetFileMD5Hash(Application.dataPath+"/AssetBundles/"+name);
                version.WriteLine(name+"|"+md5);
            }
        }
        version.Close();
        fs.Close();
        AssetDatabase.Refresh();
    }

}
