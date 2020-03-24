using DevelopTools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace _98duP14
{
    /// <summary>
    /// 根据脚本模板创建脚本
    /// </summary>
    public static class ScriptGenerator
    {

        //MonoBehaviour
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/MonoBehaviour", priority = 0)]
        public static void CreateHotfixMonoBehaviourScript()
        {
            CreateScript("NewMonoBehaviour.cs", "MonoBehaviourTemplate.txt");
        }

        private static void CreateScript(string generateFile, string templateFile) {

            UnityEngine.Object activeObject = Selection.activeObject;
            string generatedPath = AssetDatabase.GetAssetPath(activeObject);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<GenerateScriptsAction>(),
                generatedPath + "/" + generateFile, EditorGUIUtility.FindTexture("cs Script Icon"),
                Defitions.ScriptGenerator.SampleFolder + "/" + templateFile);
        }

    }
}
