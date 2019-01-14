using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Convert Scene to a prefab
/// </summary>
public class ConvertScene2Prefab : EditorWindow
{
    private string[] _scenePaths = null;
    private int _scenePathIdx = 0;

    [MenuItem("Tools/Convert Scene to Prefab")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConvertScene2Prefab>();
        window.minSize = new Vector2(400, 380);
        window.Show();
    }

    private void OnGUI()
    {
        if (_scenePaths == null)
            _scenePaths = FindAllScenesPath().ToArray();

        _scenePathIdx = EditorGUILayout.Popup("Scene: ", _scenePathIdx, _scenePaths);
    }

    private List<string> FindAllScenesPath()
    {
        var ret = new List<string>();

        var guids = AssetDatabase.FindAssets("t:scene", new string[] {"Assets"});

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            ret.Add(path.Split('/').LastOrDefault());
        }

        return ret;
    }
}