using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(HotReloadTest))]
public class HotReloadTestEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var obj = (HotReloadTest)target;

        EditorGUILayout.Space(10);
        if (GUILayout.Button("1) Test - to establish base line"))
        {
            obj.Test();
        }
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("2) Open 'HotReloadTest.cs' and change code"))
        {
            InternalEditorUtility.OpenFileAtLineExternal(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(obj)), 1);
        }
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("3) Trigger Hot Reload"))
        {
            obj.TriggerHotReload();
        }
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("4) Test to see new changes"))
        {
            obj.Test();
        }
    }

    private void OnEnable()
    {
        //auto refresh should not pick up changes as that'll interfere with hot reload
        EditorPrefs.SetInt("kAutoRefreshMode", 0);
        EditorPrefs.SetInt("kAutoRefresh", 0); //older unity versions
    }
}