using System.Reflection;
using UnityEngine;

public class HotReloadManager
{
    public static void TriggerHotReloadOnDeviceAndLocally(string sourceCodeFilePath)
    {
        // var compiledAssembly = HotReloadCompilation.Compile(sourceCodeFilePath) //called via reflection as not Editor assy not accessible and it's good to have TriggerHotReload method called in tester class for visiblity 
        var compiledAssembly = (Assembly)HarmonyLib.AccessTools.Method("HotReloadCompilation:Compile").Invoke(null, new [] { sourceCodeFilePath });
        GameObject.FindObjectOfType<OnDeviceHotReloadChangesSender>().SendChangesToConnectedDevice(compiledAssembly); //on connected device
        
        HotReloadDynamicAssemblyDetourManager.DynamicallyUpdateMethodsForCreatedAssembly(compiledAssembly); //local
    }
}