using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(OnDeviceHotReloadTest))]
public class OnDeviceHotreloadTestEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var obj = (OnDeviceHotReloadTest)target;
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("1) Build and Run \r\n(Windows, development build)"))
        {
            EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
            
        EditorGUILayout.Space(10);
        if (GUILayout.Button("2) Play external build and in editor"))
        {
            EditorApplication.isPlaying = true;
        }
        
        EditorGUILayout.Space(10);
        GUILayout.Label("3) Confirm you can see 'connected' message in console" +
                        "\r\n if not there may be some issues with network connectivity");
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("4) Open 'OnDeviceHotReloadTest.cs' and change code\r\nmake sure to save file"))
        {
            InternalEditorUtility.OpenFileAtLineExternal(AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(obj)), 1);
        }
        
        EditorGUILayout.Space(10);
        if (GUILayout.Button("5) Trigger Hot Reload"))
        {
            obj.TriggerHotReload();
        }
        
        EditorGUILayout.Space(10);
        GUILayout.Label("6) You'll see updated message both in editor and \r\nin running build");
    }

    private void OnEnable()
    {
        //auto refresh should not pick up changes as that'll interfere with hot reload
        EditorPrefs.SetInt("kAutoRefreshMode", 0);
        EditorPrefs.SetInt("kAutoRefresh", 0); //older unity versions
    }
}