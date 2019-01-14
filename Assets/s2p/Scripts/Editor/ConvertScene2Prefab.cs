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
    private string _errorMsg = "";

    [MenuItem("Tools/Convert Scene to Prefab")]
    public static void ShowWindow()
    {
        var window = GetWindow<ConvertScene2Prefab>();
        window.minSize = new Vector2(200, 100);
        window.Show();
    }

    private void OnGUI()
    {
        if (_scenePaths == null)
            _scenePaths = FindAllScenesPath().ToArray();

        _scenePathIdx = EditorGUILayout.Popup("Scene: ", _scenePathIdx, _scenePaths);
        if (GUILayout.Button("Refresh Scenes"))
        {
            _scenePaths = FindAllScenesPath().ToArray();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Start"))
        {
            var path = EditorUtility.SaveFolderPanel("Save textures to folder", "", "");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Hint", "Output path cannot be empty", "Ok");
                return;
            }

            if (!SceneToPrefab(_scenePaths[_scenePathIdx]))
            {
                EditorUtility.DisplayDialog("Failed", _errorMsg, "Ok");
                _errorMsg = "";
                return;
            }
        }
    }

    private List<string> FindAllScenesPath()
    {
        var ret = new List<string>();

        var guids = AssetDatabase.FindAssets("t:scene", new string[] {"Assets"});

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            ret.Add(path);
        }

        return ret;
    }

    private bool SceneToPrefab(string scenePath)
    {
        var currentActiveScene = EditorSceneManager.GetActiveScene();
        if (currentActiveScene.isDirty)
        {
            _errorMsg = "Current Scene is Dirty, you should save first.";
            return false;
        }
        
        var currentActiveScenePath = currentActiveScene.path;
        if (currentActiveScenePath != scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
        }

        var scene = EditorSceneManager.GetSceneByPath(scenePath);
        
        Debug.Log(scene.name);

        EditorSceneManager.OpenScene(currentActiveScenePath);

        return true;
    }
}