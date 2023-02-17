using System.Collections;
using UnityEngine;

public class OnDeviceHotReloadTest : MonoBehaviour
{
    //Make sure Auto Refresh in Edit -> Preferences -> Asset Pipeline is set to Disabled, otherwise Unity will pick up your change on save and trigger full recompile

    void Start()
    {
        StartCoroutine(PrintTestMessage());
    }

    IEnumerator PrintTestMessage()
    {
        while (true)
        {
            Debug.LogError("Test Message to change at runtime - 2"); //use error to show in development console in build
            yield return new WaitForSeconds(1);
        }
    }

#if UNITY_EDITOR
    public void TriggerHotReload()
    {
        HotReloadManager.TriggerHotReloadOnDeviceAndLocally(new System.IO.FileInfo(Application.dataPath + @"\..\" + UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.MonoScript.FromMonoBehaviour(this))).FullName);
    }
#endif
}