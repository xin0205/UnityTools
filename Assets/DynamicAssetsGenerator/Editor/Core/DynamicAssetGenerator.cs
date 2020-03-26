using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DevelopTools
{
    public static class DynamicAssetGenerator
    {
        public static void GenerateSpriteAssetFile(string assetFilePath, Dictionary<string, Sprite> assets)
        {
            SpriteAssets assetFile = ScriptableObject.CreateInstance<SpriteAssets>();
            assetFile.AddAssets(assets);
            AssetDatabase.CreateAsset(assetFile, assetFilePath);
            AssetDatabase.Refresh();
        }

        public static void GenerateGameObjectAssetFile(string assetFilePath, Dictionary<string, GameObject> assets)
        {
            GameObjectAssets assetFile = ScriptableObject.CreateInstance<GameObjectAssets>();
            assetFile.AddAssets(assets);
            AssetDatabase.CreateAsset(assetFile, assetFilePath);
            AssetDatabase.Refresh();
        }

        public static List<string> GeneratePaths(List<string> specifiedPaths, string extension)
        {
            List<string> paths = new List<string>();

            foreach (string path in specifiedPaths)
            {
                if (File.Exists(path) && Path.GetExtension(path) == extension)
                {
                    paths.Add(path);
                }
                if (Directory.Exists(path))
                {
                    paths.AddRange(Directory.GetFiles(path, "*" + extension, SearchOption.AllDirectories).ToList<string>());
                }

            }

            return paths;
        }

        public static Dictionary<string, T> GenerateAssets<T>(List<string> files) where T : Object
        {
            if (files.Count <= 0)
            {
                return new Dictionary<string, T>(0);
            }

            Dictionary<string, T> assets = new Dictionary<string, T>();

            foreach (string file in files)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(file);
                //assetImporter.SupportsRemappedAssetType(typeof(TextureImporter))
                if (assetImporter.GetType() == typeof(TextureImporter))
                {
                    TextureImporter textureImporter = assetImporter as TextureImporter;

                    if (textureImporter.textureType == TextureImporterType.Sprite)
                    {
                        if (textureImporter.spriteImportMode == SpriteImportMode.Single)
                        {
                            T asset = AssetDatabase.LoadAssetAtPath<T>(file);
                            if (asset == null)
                            {
                                Debug.LogError(string.Format("asset {0} is null", file));
                                continue;
                            }

                            AddAssets(assets, asset);
                        }
                        else if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                        {
                            T[] subAssets = AssetDatabase.LoadAllAssetsAtPath(file).OfType<T>().ToArray();

                            foreach (T subAsset in subAssets)
                            {
                                AddAssets(assets, subAsset);
                            }
                        }

                    }

                }
                else
                {
                    T asset = AssetDatabase.LoadAssetAtPath<T>(file);
                    if (asset == null)
                    {
                        Debug.LogError(string.Format("asset {0} is null", file));
                        continue;
                    }

                    AddAssets(assets, asset);
                }

            }

            return assets;

        }

        private static void AddAssets<T>(Dictionary<string, T> assets, T asset) where T : Object
        {
            if (asset == null)
                return;

            if (assets.ContainsKey(asset.name))
            {
                Debug.LogError(string.Format("assets contains key {0}", asset.name));
                return;
            }

            assets.Add(asset.name, asset);
        }

        public static void GenerateConfigAssets(string configAssetPath, string configPath, List<string> specifiedPathList)
        {

            List<string> paths = DynamicAssetGenerator.GeneratePaths(specifiedPathList, ".png");
            Dictionary<string, Sprite> assets = DynamicAssetGenerator.GenerateAssets<Sprite>(paths);

            List<DynamicAssetConfig> dynamicAssetConfigs = LoadDynamicAssetConfig(configPath);

            Dictionary<string, Sprite> configAssets = new Dictionary<string, Sprite>(dynamicAssetConfigs.Count);

            string variableDefinitioStr = "";

            foreach (DynamicAssetConfig config in dynamicAssetConfigs)
            {
                if (assets.ContainsKey(config.Resource))
                {
                    configAssets.Add(config.Resource, assets[config.Resource]);

                    if (config.Variable != "Null")
                    {
                        variableDefinitioStr += GetDefintionCode(config.Variable, config.Comment, config.Index);
                    
                    }

                }

            }

            GenerateSpriteAssetFile(configAssetPath, configAssets);


            Dictionary<string, string> replaceStrDict = new Dictionary<string, string>
            {
                ["__Variable_Definition__"] = variableDefinitioStr,
            };

            TemplateScriptGenerator.GenerateScriptFile(Defitions.DynamicAssets.DefintionTemplateFile, Defitions.DynamicAssets.DynamicAssetDefintionFile, replaceStrDict);
        }

        private static List<DynamicAssetConfig> LoadDynamicAssetConfig(string configPath)
        {
            Regex lineRegex = new Regex(@"^(?<atlas>\w+)/(?<resource>\w+)/(?<variable>(?<comment>\w+))/\w+$");

            List<DynamicAssetConfig> dynamicAssetConfigList = new List<DynamicAssetConfig>();

            string[] lines = File.ReadAllLines(Path.GetFullPath(configPath), Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (lineRegex.IsMatch(line))
                {
                    DynamicAssetConfig dynamicAssetConfig = new DynamicAssetConfig();

                    Match nameMatch = lineRegex.Match(line);

                    dynamicAssetConfig.Atlas = nameMatch.Groups["atlas"].Value;
                    dynamicAssetConfig.Resource = nameMatch.Groups["resource"].Value;
                    dynamicAssetConfig.Variable = nameMatch.Groups["variable"].Value;
                    dynamicAssetConfig.Comment = nameMatch.Groups["comment"].Value;
                    dynamicAssetConfig.Index = i;

                    dynamicAssetConfigList.Add(dynamicAssetConfig);
                }
                else
                {
                    Debug.LogError("DynamicAssetConfig Error Line:" + (i + 1) + "Error Format:" + line);

                }

            }

            return dynamicAssetConfigList;

        }

        private static string GetDefintionCode(string variable, string comment, int index)
        {

            List<CodeBlock> codeBlocks = new List<CodeBlock>() {
                    new CodeBlock(){
                        BlockName = "AssetDefintion",
                        CodeReplacementDict = new Dictionary<string, string>(){
                            ["__Variable__"] = "" + variable,
                            ["__Commnet__"] = "" + comment,
                            ["__Index__"] = "" + index,
                        }
                    }
                };

            return GetCodeBlock(codeBlocks);

        }

        private static string GetCodeBlock(List<CodeBlock> codeBlocks)
        {
            return TemplateScriptGenerator.GenerateCodeBlock(Defitions.DynamicAssets.DefintionCodeBlockTemplateFile, codeBlocks);

        }


    }

    public class DynamicAssetConfig
    {
        /// <summary>
        /// 图集名
        /// </summary>
        public string Atlas;

        /// <summary>
        /// 资源名
        /// </summary>
        public string Resource;

        /// <summary>
        /// 变量名
        /// </summary>
        public string Variable;

        /// <summary>
        /// 注释
        /// </summary>
        public string Comment;

        /// <summary>
        /// 引用索引
        /// </summary>
        public int Index;
    }
}
