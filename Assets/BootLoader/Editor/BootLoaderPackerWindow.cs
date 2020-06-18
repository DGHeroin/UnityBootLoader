using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BootLoaderPackerWindow : EditorWindow {
    string[] platformOptions = new string[] {
            "Android", "iOS", "Windows", "macOS"
        };
    string platformOption = "Android";
    [SerializeField]
    protected List<Object> gameObjects = new List<Object>();
    protected SerializedObject serializedObject;
    protected SerializedProperty assetLstProperty;
    int versionNumber = 1;
    
    [MenuItem("Window/打包启动器AssetBundle")]
    public static void Init() {
        BootLoaderPackerWindow window = (BootLoaderPackerWindow)EditorWindow.GetWindow(typeof(BootLoaderPackerWindow));
        window.titleContent = new GUIContent("启动器打包");
        window.Show();
    }
    protected void OnEnable() {
        serializedObject = new SerializedObject(this);
        assetLstProperty = serializedObject.FindProperty("gameObjects");
    }
    void OnGUI() {
        #region 选中编译平台
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("平台:");
        if (EditorGUILayout.DropdownButton(new GUIContent(platformOption), FocusType.Keyboard)) {
            GenericMenu menu = new GenericMenu();
            foreach (var item in platformOptions) {
                menu.AddItem(new GUIContent(item), platformOptions.Equals(item), OnValueSelected_Platform, item);
            }
            menu.ShowAsContext();//显示菜单
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 版本号
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("版本号:");
        this.versionNumber = EditorGUILayout.IntField(this.versionNumber);
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 触发打包
        if (GUILayout.Button(new GUIContent("打包"))) {
            var path = EditorUtility.SaveFilePanel("保存文件", "", "data.unity3d", "");
            DoPack(path);
        }
        #endregion

        #region 打包的Prefab列表
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(assetLstProperty, true);
        if (EditorGUI.EndChangeCheck()) {//提交修改
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
    void OnValueSelected_Platform(object p) {
        platformOption = p.ToString();
    }
    
    void DoPack(string savePath) {
        BuildTarget target;
        switch (platformOption) {
            case "Android":
                target = BuildTarget.Android;
                break;
            case "iOS":
                target = BuildTarget.iOS;
                break;
            case "Windows":
                target = BuildTarget.StandaloneWindows;
                break;
            case "macOS":
                target = BuildTarget.StandaloneOSX;
                break;
            default:
                EditorUtility.DisplayDialog("提醒", "没有选中编译的平台", "确定");
                return;
        }
        // 打包
        List<string> names = new List<string>();
        foreach (var go in gameObjects) {
            names.Add(AssetDatabase.GetAssetPath(go));
        }
        var abPath = Path.GetDirectoryName(savePath);
        var abName = Path.GetFileName(savePath);
        
        if(!Directory.Exists(abPath)) {
            Directory.CreateDirectory(abPath);
        }
        
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetNames = names.ToArray();
        build.assetBundleName = abName;

        BuildPipeline.BuildAssetBundles(abPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, target);
    }
}
