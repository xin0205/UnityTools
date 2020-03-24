using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace DevelopTools
{
    /// <summary>
    /// 根据脚本模板创建脚本
    /// </summary>
    public static class ScriptGenerator
    {

        //Hotfix MonoBehaviour
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/Hotfix MonoBehaviour", priority = 0)]
        public static void CreateHotfixMonoBehaviourScript()
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/NewHotfixMonoBehaviour.cs", null,
                Defitions.ScriptGenerator.ScriptTemplateFolder + "/HotfixMonoBehaviourTemplate.txt");
        }

        //Hotfix UGuiForm
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/Hotfix UGuiForm", priority = 0)]
        public static void CreateHotfixUGuiFormScript()
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/NewHotfixUGuiForm.cs", null,
                Defitions.ScriptGenerator.ScriptTemplateFolder + "/HotfixUGuiFormTemplate.txt");

        }

        //NetworkRequest
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/NetworkRequest", priority = 0)]
        public static void CreateNetworkRequestScript()
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/NewNetworkRequest.cs", null,
                Defitions.ScriptGenerator.ScriptTemplateFolder + "/NetworkRequestTemplate.txt");

        }

        //Manager
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/Manager", priority = 0)]
        public static void CreateManagerScript()
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/NewManager.cs", null,
                Defitions.ScriptGenerator.ScriptTemplateFolder + "/ManagerTemplate.txt");
        }

        //EventArgs
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/EventArgs", priority = 0)]
        public static void CreateEventArgsScript()
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/NewEventArgs.cs", null,
                Defitions.ScriptGenerator.ScriptTemplateFolder + "/EventArgsTemplate.txt");

        }

    }
}
