using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DevelopTools
{
    public class CodeBlock
    {
        public string BlockName;
        public Dictionary<string, string> CodeReplacementDict = new Dictionary<string, string>();
    }

    /// <summary>
    /// 模板脚本生成
    /// </summary>
    public static class TemplateScriptGenerator
    {
        private static Dictionary<string, string> s_CachedCodeBlockDict = new Dictionary<string, string>();
        private static Regex s_BlockRegex = new Regex(@"#S_(?<blockName>.+)\r\n(?<block>(.+\n)+)#E_\k<blockName>");

        private static int s_CodeBlockCapacity = 100;

        /// <summary>
        /// 生成模板脚本文件
        /// </summary>
        /// <param name="templateFile"></param>
        /// <param name="outputFile"></param>
        /// <param name="codeReplaceDict"></param>
        /// <param name="encoding"></param>
        public static void GenerateScriptFile(string templateFile, string outputFile, Dictionary<string, string> codeReplaceDict, Encoding encoding)
        {
            string scriptTemplate = File.ReadAllText(templateFile, encoding);

            StringBuilder codeContent = new StringBuilder(scriptTemplate);

            ReplaceCode(codeReplaceDict, ref codeContent);

            using (FileStream fileStream = new FileStream(outputFile, FileMode.Create))
            {
                using (StreamWriter stream = new StreamWriter(fileStream, encoding))
                {
                    stream.Write(codeContent.ToString());
                }
            }

            AssetDatabase.ImportAsset(outputFile);
            AssetDatabase.Refresh();

        }

        public static Dictionary<string, string> GenerateCodeBlockDict(string codeBlockFile, List<CodeBlock> codeBlocks)
        {
            Dictionary<string, string> codeBLockDict = new Dictionary<string, string>();

            codeBlocks.ForEach((codeBlock) =>
            {
                codeBLockDict.Add(codeBlock.BlockName, GenerateCodeBlock(codeBlockFile, codeBlock, Encoding.UTF8));
            });

            return codeBLockDict;
        }

        public static string GenerateCodeBlock(string codeBlockFile, List<CodeBlock> codeBlocks, Encoding encoding)
        {
            StringBuilder codeContent = new StringBuilder(s_CodeBlockCapacity);
            codeBlocks.ForEach((codeBlock) =>
            {
                codeContent.Append(GenerateCodeBlock(codeBlockFile, codeBlock, encoding));
            });

            return codeContent.ToString();
        }


        public static string GenerateCodeBlock(string codeBlockFile, CodeBlock codeBlock, Encoding encoding)
        {
            string codeBlockContent = GetCodeBlockFileContent(codeBlockFile, encoding);

            StringBuilder codeContent = new StringBuilder(s_CodeBlockCapacity);

            MatchCollection matchBlocks = s_BlockRegex.Matches(codeBlockContent);

            string blockName;
            string block;

            foreach (Match matchBlock in matchBlocks)
            {
                blockName = matchBlock.Groups["blockName"].Value;

                if (codeBlock.BlockName == blockName)
                {
                    block = matchBlock.Groups["block"].Value;
                    StringBuilder blockContent = new StringBuilder(block);
                    ReplaceCode(codeBlock.CodeReplacementDict, ref blockContent);
                    codeContent.Append(blockContent);
                }
            }

            return codeContent.ToString();

        }

        private static void ReplaceCode(Dictionary<string, string> codeReplaceDict, ref StringBuilder codeContent)
        {
            foreach (KeyValuePair<string, string> codeReplace in codeReplaceDict)
            {
                codeContent.Replace(codeReplace.Key, codeReplace.Value);
            }
        }

        public static List<string> ReadTxtFileByRow(string fileName)
        {
            List<string> rowList = new List<string>();

            StreamReader sr = new StreamReader(fileName, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                rowList.Add(line);
            }

            sr.Close();


            return rowList;

        }


        private static string GetCodeBlockFileContent(string codeBlockFile, Encoding encoding)
        {
            string codeBlockStr = "";

            if (!s_CachedCodeBlockDict.ContainsKey(codeBlockFile))
            {
                codeBlockStr = File.ReadAllText(codeBlockFile, encoding);
                s_CachedCodeBlockDict.Add(codeBlockFile, codeBlockStr);
            }
            else
            {
                codeBlockStr = s_CachedCodeBlockDict[codeBlockFile];
            }

            return codeBlockStr;
        }

    }

}

