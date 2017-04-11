using UnityEngine;
using VRFrameWork;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
        ModuleManager.Instance.RegisterAllModules();
    }

    private void OnDestroy()
    {
        ModuleManager.Instance.UnRegisterAllModule();
    }
}
