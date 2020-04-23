using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopTools
{
    public static partial class VariableGenerator
    {
        public static partial class Toggle
        {
            public static string s_BlockListener = "ToggleListener";
            public static string s_BlockCallback = "ToggleCallback";

            public static string s_ReplaceClass = "__ClassName__";
            public static string s_ReplaceVariable = "__VariableName__";

            public static Dictionary<string, string> GenerateToggleListnerCode(string className, string variableName)
            {
                List<CodeBlock> codeBlocks = new List<CodeBlock>(){
                    new CodeBlock(){
                        BlockName = s_BlockListener,
                        CodeReplacementDict = new Dictionary<string, string>(){
                            [s_ReplaceClass] = className,
                            [s_ReplaceVariable] = variableName,
                        }
                    },

                    new CodeBlock(){
                        BlockName = s_BlockCallback,
                        CodeReplacementDict = new Dictionary<string, string>(){
                            [s_ReplaceVariable] = variableName,
                        }
                    }

                };

                return TemplateScriptGenerator.GenerateCodeBlockDict(Defitions.VariableGenerator.VariableCodeFile, codeBlocks);

            }

            public static void GenerateToggle(string className, string variableName)
            {
                Dictionary<string, string> codeDict = GenerateToggleListnerCode(className, variableName);

                m_ClickListeners += codeDict[s_BlockListener];
                m_ClickCallbacks += codeDict[s_BlockCallback];

            }

        }
    }
}
