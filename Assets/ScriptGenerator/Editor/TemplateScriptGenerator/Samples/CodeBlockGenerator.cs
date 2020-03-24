using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DevelopTools
{
    /// <summary>
    /// 根据代码块模板创建脚本
    /// </summary>
    public static class CodeBlockGenerator
    {
        //Hotfix MonoBehaviour
        [MenuItem(Defitions.ScriptGenerator.MenuDiretoryPrefix + "Template/CodeBlockTemplate", priority = 0)]
        public static void CreateCodeBlockTemplateScript()
        {
            CreateScript(Defitions.ScriptGenerator.SampleFolder + "/CodeBlockTemplate.txt", "NewCodeBlockTemplate.cs");
        }

        private static void CreateScript(string templateFile, string outputFile)
        {
            UnityEngine.Object activeObject = Selection.activeObject;
            string directory = AssetDatabase.GetAssetPath(activeObject);

            Dictionary<string, string> codeReplaceDict = new Dictionary<string, string>()
            {
                ["__Name__"] = "NewCodeBlockTemplate",
                ["__Test_Code_Block__"] = GetCodeBlockTest(10)
            };

            TemplateScriptGenerator.GenerateScriptFile(templateFile, directory + "/" + outputFile, codeReplaceDict, Encoding.UTF8);

        }

        private static string GetCodeBlockTest(int count)
        {

            List<CodeBlock> codeBlocks = new List<CodeBlock>() {
                    new CodeBlock(){
                        BlockName = "CodeBlockTest",
                        CodeReplacementDict = new Dictionary<string, string>(){
                            ["__Count__"] = "" + count,

                        }
                    }
                };

            return GetVariableCode(codeBlocks);

        }

        private static string GetVariableCode(List<CodeBlock> codeBlocks)
        {
            return TemplateScriptGenerator.GenerateCodeBlock(Defitions.ScriptGenerator.SampleFolder + "/CodeBlock.txt", codeBlocks);

        }
    }
}
