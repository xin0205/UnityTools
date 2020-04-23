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
    /// <summary>
    /// 生成Proto请求回调代码文件
    /// </summary>
    public static class ProtoInterfaceGenerator
    {
        private static string m_CodeTemplateFile = Defitions.ScriptGenerator.DirectoryPrefix + "ProtoInterfaceGenerator/NetworkRequestTemplate.txt";


        private static string m_SCInterfaceRegex = @"^public static final int SC(?<interfaceName>\w+) = (?<interfaceIndex>\d+);$";
        private static string m_CSInterfaceRegex = @"^public static final int CS(?<interfaceName>\w+) = (?<interfaceIndex>\d+);$";
        private static string m_CommentRegex = @"^/\*\* (?<comment>.+) \*/$";
        //private static string m_NameRegex = @"^name:(?<comment>\w+)$";
        private static string m_NameCommentRegex = @"^comment:(?<comment>.+)$";

        private static string m_SCEventSubFormat = "\t\t\tGameEntry.Event.Subscribe(SC{0}EventArgs.EventId, OnSC{1});\n";
        private static string m_SCEventUnsubFormat = "\t\t\tGameEntry.Event.Unsubscribe(SC{0}EventArgs.EventId, OnSC{1});\n";
        private static string m_SCEventCallbackFormat = "\t\t//{0}回调\n" +
                                                        "\t\tprivate void OnSC{1}(object sender, GameEventArgs gameEventArgs)\n" +
                                                        "\t\t{{\n" +
                                                        "\t\t\tSC{2} sC{3} = (gameEventArgs as SC{4}EventArgs).SC{5};\n\n" +
                                                        "\t\t}}\n\n";

        private static string m_CSRequsetFormat = "\t\t//{0}请求\n" +
                                                  "\t\tpublic void {1}Request()\n" +
                                                  "\t\t{{\n" +
                                                  "\t\t\tCS{2} cS{3} = new CS{4}();\n\n" +
                                                  "\t\t\tNetworkTcpHelper.Instance.Send(cS{5});\n" +
                                                  "\t\t}}\n\n";

        private static string m_SCEventSubStrs = "";
        private static string m_SCEventUnsubStrs = "";
        private static string m_SCCallbackStrs = "";
        private static string m_CSRequsetStrs = "";

        private static string m_NowComment = "";
        private static string m_Name = "";
        private static string m_NameCommnet = "";
        
        private static Dictionary<string, string> m_CodeReplaceDict = new Dictionary<string, string>();

        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Generate ProtoInterface", priority = 0)]
        public static void GenerateCodeFile(MenuCommand menuCommand)
        {
            m_CodeReplaceDict.Clear();
            m_SCEventSubStrs = "";
            m_SCEventUnsubStrs = "";
            m_SCCallbackStrs = "";
            m_CSRequsetStrs = "";
            m_NowComment = "";
            m_Name = "";

            //UnityEngine.GameObject txt2 = menuCommand.context as UnityEngine.GameObject;

            UnityEngine.Object txt = Selection.activeObject;

            string selectionPath = AssetDatabase.GetAssetPath(txt);
            string directory = Path.GetDirectoryName(selectionPath);
            string fileName = Path.GetFileNameWithoutExtension(selectionPath);

            m_Name = fileName;

            GenerateCode(selectionPath);
            AddCodeReplace();

            string outputDirectory = Defitions.ScriptGenerator.DirectoryPrefix + "ProtoInterfaceGenerator/Generation";
            string outputFile = outputDirectory + "/" + fileName + "NetworkRequest.txt";

            //string outputFile = directory + "/" + fileName + "NetworkRequest.txt";

            TemplateScriptGenerator.GenerateScriptFile(m_CodeTemplateFile, outputFile, m_CodeReplaceDict, Encoding.UTF8);

            AssetDatabase.ImportAsset(outputFile);
            AssetDatabase.Refresh();

            Debug.Log("生成完成");
        }


        private static void GenerateCode(string selectionPath)
        {
            List<string> rowStr = TemplateScriptGenerator.ReadTxtFileByRow(selectionPath);

            foreach (string str in rowStr)
            {
                if (new Regex(m_NameCommentRegex).IsMatch(str))
                {
                    Match commnetMatch = new Regex(m_NameCommentRegex).Match(str);

                    m_NameCommnet = commnetMatch.Groups["comment"].Value;

                }

                if (new Regex(m_CommentRegex).IsMatch(str))
                {
                    Match commnetMatch = new Regex(m_CommentRegex).Match(str);

                    m_NowComment = commnetMatch.Groups["comment"].Value;

                }


                if (new Regex(m_SCInterfaceRegex).IsMatch(str))
                {

                    Match nameMatch = new Regex(m_SCInterfaceRegex).Match(str);

                    string name = nameMatch.Groups["interfaceName"].Value;
                    string index = nameMatch.Groups["interfaceIndex"].Value;

                    m_SCEventSubStrs += string.Format(m_SCEventSubFormat, name, name);
                    m_SCEventUnsubStrs += string.Format(m_SCEventUnsubFormat, name, name);
                    m_SCCallbackStrs += string.Format(m_SCEventCallbackFormat, m_NowComment, name, name, name, name, name);

                }

                if (new Regex(m_CSInterfaceRegex).IsMatch(str))
                {

                    Match nameMatch = new Regex(m_CSInterfaceRegex).Match(str);

                    string name = nameMatch.Groups["interfaceName"].Value;
                    string index = nameMatch.Groups["interfaceIndex"].Value;

                    m_CSRequsetStrs += string.Format(m_CSRequsetFormat, m_NowComment, name, name, name, name, name);


                }
            }

        }

        public static void AddCodeReplace()
        {
            m_CodeReplaceDict.Add("__Name_Comment__", m_NameCommnet);
            m_CodeReplaceDict.Add("__Name__", m_Name);
            m_CodeReplaceDict.Add("__Event_Sub__", m_SCEventSubStrs);
            m_CodeReplaceDict.Add("__Event_Unsub__", m_SCEventUnsubStrs);
            m_CodeReplaceDict.Add("__SC_Callback__", m_SCCallbackStrs);
            m_CodeReplaceDict.Add("__CS_Request__", m_CSRequsetStrs);

        }


    }

}


