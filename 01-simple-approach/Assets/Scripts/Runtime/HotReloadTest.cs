using UnityEditor;
using UnityEngine;

public class HotReloadTest : MonoBehaviour
{
    //Make sure Auto Refresh in Edit -> Preferences -> Asset Pipeline is set to Disabled, otherwise Unity will pick up your change on save and trigger full recompile
    
    public void Test()
    {
        Debug.Log("Test Message to change at runtime - 1");
        //Make sure to save file after change
    }
    
    public void TriggerHotReload()
    {
        HotReloadManager.TriggerHotReload(MonoScript.FromMonoBehaviour(this), nameof(Test));
    }
}