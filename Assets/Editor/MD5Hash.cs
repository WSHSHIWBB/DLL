using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;

public class MD5Hash : Editor
{
    [MenuItem("Tools/AssetBundle/ShowMD5Hash")]
    private static void ShowFileMD5Hash()
    {
        object[] selectedFile = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        foreach(Object obj in selectedFile)
        {
            string fileParth = AssetDatabase.GetAssetPath(obj);
            fileParth=fileParth.Replace("Assets", "");
            fileParth = fileParth.Replace("Streaming", "StreamingAssets");
            Debug.Log(Application.dataPath+fileParth);
            string md5 = GetFileMD5Hash(Application.dataPath + fileParth);
            Debug.Log(md5);
        }
    }

    public static string GetFileMD5Hash(string file)
    {
        FileStream fs = new FileStream(file, FileMode.Open);
        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] retVal = md5.ComputeHash(fs);
        fs.Close();

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }

        return sb.ToString();
    }
}
