using System.Collections;using System.Collections.Generic;using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class CreateVuforiaToolsConfigurationFile{


#if UNITY_EDITOR    public static void CreateConfigFile(){        VuforiaToolsConfiguration asset = ScriptableObject.CreateInstance<VuforiaToolsConfiguration>();        AssetDatabase.CreateAsset(asset, "Assets/Resources/VuforiaToolsConfiguration.asset");        AssetDatabase.SaveAssets();        EditorUtility.FocusProjectWindow();        Selection.activeObject = asset;    }
#endif}