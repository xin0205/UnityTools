//using P14;
//using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


namespace DevelopTools
{
    //public static class UnityExtension
    //{
    //    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    //    {
    //        T component = gameObject.GetComponent<T>();
    //        if (component == null)
    //        {
    //            component = gameObject.AddComponent<T>();
    //        }

    //        return component;
    //    }
    //}

    public struct FullTypeName
    {
        public const string Button = "Button";

    }

    public class Variable
    {
        public string ShortType;
        public string VariableName;
        public int Count;

        public Variable(string shortTypeName, string variableName)
        {
            ShortType = shortTypeName;
            VariableName = variableName;
            Count = 1;

        }
    }

    /// <summary>
    /// 热更代码变量声明定义生成
    /// </summary>
    /// 
    public static class VariableGenerator
    {
        private static string m_CodeTemplateFile = Defitions.ScriptGenerator.DirectoryPrefix + "VariableGenerator/VariableTemplate.txt";
        private static string m_UIFormTemplateFile = Defitions.ScriptGenerator.DirectoryPrefix + "VariableGenerator/HotfixUGuiFormTemplate.txt";

        private static string m_UIItemTemplateFile = Defitions.ScriptGenerator.DirectoryPrefix + "VariableGenerator/HotfixMonoBehaviourTemplate.txt";

        private static string m_UIFormDirectory = "Assets/Game/HotfixScripts/UIForms/";

        private static string variableNameRegex = @"^~?#(?<variableType>[A-Za-z0-9]+)_(?<variableName>[A-Za-z0-9]+)(|_(?<variableIndex>\d))$";
        private static string m_ExceptSign = "~";

        private static Encoding m_Encoding = Encoding.UTF8;

        private static string m_OutputFile = "";

        private static string m_ClassName;
        private static string m_VariableClassName;
        private static string m_Declarations;
        private static string m_Definitions;
        private static string m_ButtonListeners;
        private static string m_ButtonCallbacks;

        private static string m_DeclarationFormat = "\t\tpublic {0} {1};\n";
        private static string m_DefinitionFormat = "\t\tpublic {0} {1};\n";
        private static string m_GoNamePrefixFormat = "#{0}_{1}";

        private static string m_Comment = "";

        //只能添加UI控件
        private static Dictionary<string, string> m_TypeDict = new Dictionary<string, string>()
        {
            //Unity
            ["Go"] = "GameObject",
            ["Tsf"] = "Transform",
            ["RTsf"] = "RectTransform",
            ["Img"] = "Image",
            ["Txt"] = "Text",
            ["Btn"] = "Button",
            ["VLG"] = "VerticalLayoutGroup",

            //自定义扩展
            ["RLGIV"] = "ReuseLayoutGroupItemsVertical",
            ["RLGIH"] = "ReuseLayoutGroupItemsHorizontal",
            ["RLGI"] = "ReuseLayoutGroupItems",

            //待移除
            ["OHI"] = "OtherHeroIcon",

        };

        private static Dictionary<string, string> m_CodeReplaceDict = new Dictionary<string, string>();

        private static Dictionary<string, Variable> m_VariableArrayDict = new Dictionary<string, Variable>();

        private static void Init(MenuCommand menuCommand, bool isUIForm = false)
        {

            m_CodeReplaceDict.Clear();
            m_VariableArrayDict.Clear();
            m_ClassName = "";
            m_Declarations = "";
            m_Definitions = "";
            m_ButtonListeners = "";
            m_ButtonCallbacks = "";

            GameObject root = menuCommand.context as GameObject;

            m_ClassName = root.name;
            m_VariableClassName = m_ClassName + "Variable";

            m_Declarations = "\t\tpublic " + m_ClassName + " " + m_ClassName + ";\n";

            ForeachGo(root.transform);

            GenerateArrayCode();

            if (isUIForm)
            {
                m_Definitions += "\t\t\t" + m_ClassName + " = GetUGuiForm<" + m_ClassName + ">();\n";
            }
            else
            {
                m_Comment = "\t\t//public " + m_ClassName + "Variable m_UI;\n" +
                            "\t\t//m_UI = GetComponent<" + m_ClassName + "Variable>();\n";

                m_Definitions += "\t\t\t" + m_ClassName + " = GetComponent<" + m_ClassName + ">();\n";
            }

            m_Comment += m_ButtonCallbacks;

            ReplaceCode();
        }


        [MenuItem("GameObject/Generate UIForm Variable Script", priority = 0)]
        public static void GenerateUIFormVariableScript(MenuCommand menuCommand)
        {
            Init(menuCommand, true);

            string outputDirectory = m_UIFormDirectory + m_ClassName + "/";

            if (!Directory.Exists(outputDirectory))
            {
                //创建目录，同时生成XXUIForm.cs
                Directory.CreateDirectory(outputDirectory);

                GenerateUIFormScript();
            }

            GenerateUIFormVariableScript();

        }

        private static void GenerateUIFormVariableScript()
        {
            string outputDirectory = m_UIFormDirectory + m_ClassName + "/";
            string outputFile = outputDirectory + m_ClassName + "Variable.cs";
            TemplateScriptGenerator.GenerateScriptFile(m_CodeTemplateFile, outputFile, m_CodeReplaceDict, m_Encoding);

        }

        private static void GenerateUIFormScript()
        {

            string outputDirectory = m_UIFormDirectory + m_ClassName + "/";

            string uiformPath = outputDirectory + m_ClassName + ".cs";

            Dictionary<string, string> CodeReplacement = new Dictionary<string, string>()
            {
                ["__Name__"] = m_ClassName,
                ["__Variable_ButtonCallback__"] = m_ButtonCallbacks
            };

            TemplateScriptGenerator.GenerateScriptFile(m_UIFormTemplateFile, uiformPath, CodeReplacement, Encoding.UTF8);

        }

        [MenuItem("GameObject/Generate Variable Script", priority = 0)]
        public static void GenerateVariableScript(MenuCommand menuCommand)
        {
            Init(menuCommand);

            /*
            StandaloneFileBrowser.SaveFilePanelAsync("保存的窗口", m_UIFormDirectory, m_VariableClassName, "cs", (path) =>
            {
                //Debug.Log(path);
                TemplateScriptGenerator.GenerateScriptFile(m_CodeTemplateFile, path, m_CodeReplaceDict, m_Encoding);
                string directory = Path.GetDirectoryName(path);

                string uiItemPath = directory + "/" + m_ClassName + ".cs";

                if (!File.Exists(uiItemPath)) {
                    Dictionary<string, string> CodeReplacement = new Dictionary<string, string>()
                    {
                        ["__Name__"] = m_ClassName,
                        ["__Variable_ButtonCallback__"] = m_ButtonCallbacks
                    };

                    TemplateScriptGenerator.GenerateScriptFile(m_UIItemTemplateFile, uiItemPath, CodeReplacement, m_Encoding);

                }


            });
            */
        }

        public static void ForeachGo(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.name.StartsWith(m_ExceptSign))
                {
                    GenerateCode(child);
                    continue;
                }

                GenerateCode(child);

                if (child.childCount > 0)
                {
                    ForeachGo(child);
                }
            }
        }

        private static void GenerateCode(Transform transform)
        {
            if (new Regex(variableNameRegex).IsMatch(transform.name))
            {
                Match nameMatch = new Regex(variableNameRegex).Match(transform.name);

                string type = nameMatch.Groups["variableType"].Value;
                string name = nameMatch.Groups["variableName"].Value;
                string index = nameMatch.Groups["variableIndex"].Value;

                string fullTypeName = GetFullTypeName(type);

                //生成数组形式
                if (!string.IsNullOrEmpty(index))
                {
                    Variable variable = new Variable(type, name);
                    RecordArrayCode(variable);
                    return;
                }

                m_Declarations += string.Format(m_DeclarationFormat, fullTypeName, name);
                m_Definitions += "\t\t\t" + name + " = GetChildComponentByName<" + fullTypeName + ">(\"" + transform.name + "\");\n";

                switch (fullTypeName)
                {
                    case FullTypeName.Button:
                        GenerateButton(name);
                        break;
                }

            }

        }

        private static void RecordArrayCode(Variable variable)
        {
            if (m_VariableArrayDict.ContainsKey(variable.VariableName))
            {
                m_VariableArrayDict[variable.VariableName].Count += 1;
            }
            else
            {
                m_VariableArrayDict.Add(variable.VariableName, variable);
            }

        }

        private static void GenerateArrayCode()
        {
            foreach (KeyValuePair<string, Variable> kv in m_VariableArrayDict)
            {
                Variable variable = kv.Value;

                string fullType = GetFullTypeName(variable.ShortType);

                m_Declarations += string.Format("\t\tpublic List<{0}> {1}s = new List<{2}>({3});\n", fullType, variable.VariableName, fullType, kv.Value.Count);

                string arrayDefinitionFormat = "\t\t\tfor ( int i = 0; i < {0}; i++)\n" +
                                                   "\t\t\t{{\n" +
                                                   "\t\t\t\t{1}s.Add(GetChildComponentByName<{2}>(\"{3}_\" + (i + 1)));\n" +
                                                   "\t\t\t}}\n";

                string goNamePrefix = string.Format(m_GoNamePrefixFormat, variable.ShortType, variable.VariableName);
                m_Definitions += string.Format(arrayDefinitionFormat, variable.Count, variable.VariableName, fullType, goNamePrefix);

                switch (fullType)
                {
                    case FullTypeName.Button:
                        GenerateButtonArray(variable);
                        break;

                }
            }

        }

        private static void GenerateButtonArray(Variable variable)
        {

            string buttonListernerFormat = "\t\t\tfor ( int i = 0; i < {0}s.Count; i++)\n" +
                                                   "\t\t\t{{\n" +
                                                   "\t\t\t\tint index = i + 1;\n" +
                                                   "\t\t\t\t{1}s[i].onClick.AddListener(() =>\n" +
                                                   "\t\t\t\t{{\n" +
                                                   "\t\t\t\t\t{2}.OnClick{3}(index);\n" +
                                                   "\t\t\t\t}});\n" +
                                                   "\t\t\t}}\n";

            m_ButtonListeners += string.Format(buttonListernerFormat, variable.VariableName, variable.VariableName, m_ClassName, variable.VariableName);

            string buttonCallbackFormat = "\t\tpublic void OnClick{0}(int index)\n\t\t{{\n\t\t}}\n";

            m_ButtonCallbacks += string.Format(buttonCallbackFormat, variable.VariableName);

        }

        private static void GenerateButton(string variableName)
        {
            m_ButtonListeners += "\t\t\t" + variableName + ".onClick.AddListener(" + m_ClassName + ".OnClick" + variableName + ");\n";
            m_ButtonCallbacks += "\t\tpublic void OnClick" + variableName + "()\n\t\t{\n\t\t}\n";

        }

        public static void ReplaceCode()
        {
            m_CodeReplaceDict.Add("__Comment__", m_Comment);
            m_CodeReplaceDict.Add("__Class_Name__", m_VariableClassName);
            m_CodeReplaceDict.Add("__Variable_Declaration__", m_Declarations);
            m_CodeReplaceDict.Add("__Variable_Definition__", m_Definitions);
            m_CodeReplaceDict.Add("__Variable_ButtonListener__", m_ButtonListeners);
            //m_CodeReplaceDict.Add("__Variable_ButtonCallback__", m_ButtonCallbacks);

        }

        public static string GetFullTypeName(string shortName)
        {

            if (m_TypeDict.ContainsKey(shortName))
            {
                return m_TypeDict[shortName];
            }

            Debug.LogWarning("Uncontain type short Name:" + shortName);

            return shortName;

        }
    }

}


