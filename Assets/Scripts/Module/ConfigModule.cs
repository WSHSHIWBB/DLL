using UnityEngine;
using VRFrameWork;
using System.IO;
using LitJson;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ConfigValueType
{
    CaptureDuration,
    LongestOutOfFOV,
    PetMoveSpeed,
    PetMoveDistance,
}

public class DLLConfig
{
    public struct CaptureDuration
    {
        public double Tx1;
        public double Tx2;
        public double Tx3;
        public double Tx4;
        public double Tx5;
    }
    public struct LongestOutOfFOV
    {
        public double T1;
        public double T2;
        public double T3;
    }
    public struct PetMoveSpeed
    {
        public double V1;
        public double V2;
        public double V3;
        public double V4;
    }
    public struct PetMoveDistance
    {
        public double L1;
        public double L2;
        public double L3;
        public double L4;
        public double L5;
    }
    public CaptureDuration captureDuration;
    public LongestOutOfFOV longestOutOfFOV;
    public PetMoveSpeed petMoveSpeed;
    public PetMoveDistance petMoveDistance;
}

public class ConfigModule : BaseModule
{
    public DLLConfig dllConfig;
    private readonly string FILENAME = "DLLConfigFile";

    protected override void OnLoad()
    {
        ReadConfigInfo(FILENAME);
    }

    protected override void OnRelease()
    {
        //SaveConfigInfo(FILENAME);
    }

    private void ReadConfigInfo(string fileName)
    {
        string path = Application.streamingAssetsPath + "/Config/" + fileName + ".json";
        TextAsset ta = Resources.Load<TextAsset>("File/DLLConfigFile");
        string json = ta.text.Split('\n')[0];
        dllConfig = JsonMapper.ToObject<DLLConfig>(json);
        if (File.Exists(path))
        {
            /*
#if !UNITY_ANDROID
            FileStream fs = new FileStream(path, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string json = sr.ReadLine();
            dllConfig = JsonMapper.ToObject<DLLConfig>(json);
            fs.Close();
            fs.Dispose();
#endif

#if UNITY_ANDROID
*/       
        }
        else
        {
            Debug.Log("The file of path:" + path + " Doesn't exit!");
        }
    }

    private void SaveConfigInfo(string fileName)
    {
        if(!Directory.Exists(Application.streamingAssetsPath+"/Config"))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath + "/Config");
        }     
        string path = Application.streamingAssetsPath + "/Config/" + fileName + ".json";
        FileStream fs = new FileStream(path, FileMode.Create);
        StreamWriter sw = new StreamWriter(fs);
        DLLConfig newConfig = new DLLConfig();
        newConfig.captureDuration.Tx1 = 6;
        newConfig.captureDuration.Tx2 = 10;
        newConfig.captureDuration.Tx3 = 15;
        newConfig.captureDuration.Tx4 = 20;
        newConfig.captureDuration.Tx5 = 30;
        newConfig.longestOutOfFOV.T1 = 3;
        newConfig.longestOutOfFOV.T2 = 5;
        newConfig.longestOutOfFOV.T3 = 7;
        newConfig.petMoveSpeed.V1 = 1;
        newConfig.petMoveSpeed.V2 = 2;
        newConfig.petMoveSpeed.V3 = 3;
        newConfig.petMoveSpeed.V4 = 4;
        newConfig.petMoveDistance.L1 = 10;
        newConfig.petMoveDistance.L2 = 15;
        newConfig.petMoveDistance.L3 = 30;
        newConfig.petMoveDistance.L4 = 60;
        newConfig.petMoveDistance.L5 = 90;
        string json = JsonMapper.ToJson(newConfig);
        Debug.Log(json);
        sw.WriteLine(json);
        sw.Close();
        sw.Dispose();
        fs.Close();   
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

}
