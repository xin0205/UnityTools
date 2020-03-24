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
        private static Dictionary<string, string> cachedCodeBlockDict = new Dictionary<string, string>();
        private static Regex blockRegex = new Regex(@"#S_(?<blockName>.+)\r\n(?<block>(.+\n)+)#E_\k<blockName>");

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

        public static string GenerateCodeBlock(string codeBlockFile, List<CodeBlock> codeBlocks)
        {
            return GenerateCodeBlock(codeBlockFile, codeBlocks, Encoding.UTF8);
        }

        /// <summary>
        /// 生成模板代码块
        /// </summary>
        /// <param name="codeBlockFile"></param>
        /// <param name="codeBlocks"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GenerateCodeBlock(string codeBlockFile, List<CodeBlock> codeBlocks, Encoding encoding)
        {
            string codeBlockStr = "";

            if (!cachedCodeBlockDict.ContainsKey(codeBlockFile))
            {
                codeBlockStr = File.ReadAllText(codeBlockFile, encoding);
                cachedCodeBlockDict.Add(codeBlockFile, codeBlockStr);
            }
            else
            {
                codeBlockStr = cachedCodeBlockDict[codeBlockFile];
            }

            MatchCollection matchBlocks = blockRegex.Matches(codeBlockStr);

            StringBuilder codeContent = new StringBuilder(1024);

            string blockName;
            string block;

            foreach (Match matchBlock in matchBlocks)
            {
                blockName = matchBlock.Groups["blockName"].Value;
                CodeBlock sameBlock = codeBlocks.Find(((b) => b.BlockName == blockName));

                if (sameBlock != null)
                {
                    block = matchBlock.Groups["block"].Value;
                    StringBuilder blockContent = new StringBuilder(block);
                    ReplaceCode(sameBlock.CodeReplacementDict, ref blockContent);
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
    }

}

