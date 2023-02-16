using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

public class HotReloadManager : MonoBehaviour
{
    const BindingFlags ALL_DECLARED_METHODS_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic |
                                                            BindingFlags.Static | BindingFlags.Instance |
                                                            BindingFlags.DeclaredOnly; //only declared methods can be redirected, otherwise it'll result in hang
    
    private static readonly List<Type> ExcludeMethodsDefinedOnTypes = new List<Type>
    {
        typeof(MonoBehaviour),
        typeof(Behaviour),
        typeof(UnityEngine.Object),
        typeof(Component),
        typeof(System.Object)
    };
    
    public static void TriggerHotReload(MonoScript fileToHotReload)
    {
        var compiledAssembly = HotReloadCompilation.Compile(new System.IO.FileInfo(Application.dataPath + @"\..\" + AssetDatabase.GetAssetPath(fileToHotReload)).FullName);
        DynamicallyUpdateMethodsForCreatedAssembly(compiledAssembly );
    }

    public static void DynamicallyUpdateMethodsForCreatedAssembly(Assembly dynamicallyLoadedAssemblyWithUpdates)
    {
        foreach (var createdType in dynamicallyLoadedAssemblyWithUpdates.GetTypes())
        {
            var allTypesInNonDynamicGeneratedAssemblies = GetAllTypesInNonDynamicGeneratedAssemblies();
            var matchingTypeInExistingAssemblies = allTypesInNonDynamicGeneratedAssemblies[createdType.FullName];
            var allDeclaredMethodsInExistingType = matchingTypeInExistingAssemblies.GetMethods(ALL_DECLARED_METHODS_BINDING_FLAGS).Where(m => !ExcludeMethodsDefinedOnTypes.Contains(m.DeclaringType)).ToList();
            
            foreach (var createdTypeMethodToUpdate in createdType.GetMethods(ALL_DECLARED_METHODS_BINDING_FLAGS).Where(m => !ExcludeMethodsDefinedOnTypes.Contains(m.DeclaringType)))
            {
                var matchingMethodInExistingType = allDeclaredMethodsInExistingType.SingleOrDefault(m => m.FullDescription() == createdTypeMethodToUpdate.FullDescription());
                if (matchingMethodInExistingType != null)
                {

                    Memory.DetourMethod(matchingMethodInExistingType, createdTypeMethodToUpdate);
                }
            }
        }
        
        Debug.Log($"Hot Reload performed, you can retest now.");
    }

    private static Dictionary<string, Type> _AllTypesInNonDynamicGeneratedAssemblies;
    private static Dictionary<string, Type> GetAllTypesInNonDynamicGeneratedAssemblies()
    {
        if (_AllTypesInNonDynamicGeneratedAssemblies == null)
        {
            _AllTypesInNonDynamicGeneratedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.FullName.Contains(HotReloadCompilation.HotReloadAssemblyNamePostfix))
                .SelectMany(a => a.GetTypes())
                .GroupBy(t => t.FullName)
                .Select(g => g.First())
                .ToDictionary(t => t.FullName, t => t);
        }

        return _AllTypesInNonDynamicGeneratedAssemblies;
    }
}