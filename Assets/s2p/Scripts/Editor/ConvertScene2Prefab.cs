using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Convert Scene to a prefab
/// by ruihanyang
/// </summary>
public class ConvertScene2Prefab : EditorWindow
{
    private string[] _scenePaths = null;
    private int _scenePathIdx = 0;
    private string _errorMsg = "";
    private string _defaultOutputName = "output";
    private string _savePath = "";

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

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Prefab Name: ");
        _defaultOutputName = EditorGUILayout.TextField(_defaultOutputName);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Start"))
        {
            _savePath = EditorUtility.SaveFolderPanel("Save textures to folder", "", "");
            if (string.IsNullOrEmpty(_savePath))
            {
                EditorUtility.DisplayDialog("Hint", "Output path cannot be empty", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(_defaultOutputName))
            {
                EditorUtility.DisplayDialog("Hint", "Output prefab name cannot be empty", "Ok");
                return;
            }

            if (!SceneToPrefab(_scenePaths[_scenePathIdx]))
            {
                EditorUtility.DisplayDialog("Failed", _errorMsg, "Ok");
                _errorMsg = "";
                return;
            }
            else
            {
                EditorUtility.DisplayDialog("Hint",
                    string.Format("Create success!\nOutput Path: {0}", _savePath + "/" +  _defaultOutputName + ".prefab"),
                    "Ok");
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

        if (scene.IsValid())
            Debug.Log("Load scene " + scene.name + " success!");

        var root = new GameObject(_defaultOutputName);
        foreach (var gameObject in scene.GetRootGameObjects())
        {
            if (gameObject.name == root.name)
                continue;
            
            var obj = Object.Instantiate(gameObject, root.transform);
            obj.name = obj.name.Replace("(Clone)", "");
        }

        if (!SavePrefab(GetResRelativePath(_savePath), root))
            return false;

        EditorSceneManager.OpenScene(currentActiveScenePath);

        return true;
    }

    private bool SavePrefab(string outputPath, GameObject rootObj)
    {
        if (rootObj == null)
        {
            _errorMsg = "Create prefab failed, root object is null";
            return false;
        }

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var prefabOutputPath = outputPath + "/" + _defaultOutputName + ".prefab";

        var oldPrefab = AssetDatabase.LoadAssetAtPath(prefabOutputPath, typeof(GameObject));
        var newPrefab = oldPrefab == null
            ? PrefabUtility.CreatePrefab(prefabOutputPath, rootObj)
            : PrefabUtility.ReplacePrefab(rootObj, oldPrefab);

        if (newPrefab == null)
        {
            _errorMsg = "Create prefab failed";
            return false;
        }

        PrefabUtility.ConnectGameObjectToPrefab(rootObj, newPrefab);

        DestroyImmediate(rootObj, true);

        AssetDatabase.Refresh();

        return true;
    }

    private static string GetResAbsolutePath(string relativePath)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(relativePath))
            return Application.dataPath;

        return string.Format("{0}/{1}", Application.dataPath, relativePath.Substring("Assets/".Length));
#else
            return "";
#endif
    }

    private static string GetResRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
            return "";

        absolutePath = absolutePath.Replace("\\", "/");
        var index = absolutePath.IndexOf("Assets/", StringComparison.Ordinal);
        return absolutePath.Substring(index > 0 ? index : 0);
    }
}