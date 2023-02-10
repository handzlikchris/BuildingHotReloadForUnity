using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

public class HotReloadManager : MonoBehaviour
{
    public static void TriggerHotReload(MonoScript fileToHotReload, string methodNameToHotReload)
    {
        var compiledAssembly = HotReloadCompilation.Compile(new System.IO.FileInfo(Application.dataPath + @"\..\" + AssetDatabase.GetAssetPath(fileToHotReload)).FullName);
        DynamicallyUpdateMethodsForCreatedAssembly(compiledAssembly, fileToHotReload, methodNameToHotReload );
    }

    public static void DynamicallyUpdateMethodsForCreatedAssembly(Assembly dynamicallyLoadedAssemblyWithUpdates, MonoScript fileToHotReload, string methodNameToHotReload)
    {
        var createdType = dynamicallyLoadedAssemblyWithUpdates.GetType(fileToHotReload.GetClass().Name);
       
        var originalMethod = fileToHotReload.GetClass().GetMethod(methodNameToHotReload, BindingFlags.Instance | BindingFlags.Public);
        var createdMethodToHotReload = createdType.GetMethod(methodNameToHotReload, BindingFlags.Instance | BindingFlags.Public);
        
        Memory.DetourMethod(originalMethod, createdMethodToHotReload);
        Debug.Log($"Hot Reload for '{methodNameToHotReload}' performed, you can retest now.");
    }
}