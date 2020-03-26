using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevelopTools
{
    public class DynamicAssetGenerateMenu
    {
        [MenuItem(Defitions.DynamicAssets.MenuDiretoryPrefix + "Generate Sprite Asset")]
        public static void GenerateSpriteAsset()
        {
            List<string> specifiedPathList = new List<string>() {
                    Defitions.DynamicAssets.ResourcePath + "Equip",
                    Defitions.DynamicAssets.ResourcePath + "Hero",
                };

            List<string> pathList = DynamicAssetGenerator.GeneratePaths(specifiedPathList, ".png");
            Dictionary<string, Sprite> assets = DynamicAssetGenerator.GenerateAssets<Sprite>(pathList);

            DynamicAssetGenerator.GenerateSpriteAssetFile(Defitions.DynamicAssets.SpriteAssetFile, assets);

        }

        [MenuItem(Defitions.DynamicAssets.MenuDiretoryPrefix + "Generate Prefab Asset")]
        public static void GeneratePrefabAsset()
        {
            List<string> specifiedPathList = new List<string>() {
                    Defitions.DynamicAssets.ResourcePath + "Prefab",
                    Defitions.DynamicAssets.ResourcePath + "TestPrefab.prefab",
                };

            List<string> pathList = DynamicAssetGenerator.GeneratePaths(specifiedPathList, ".prefab");
            Dictionary<string, GameObject> assets = DynamicAssetGenerator.GenerateAssets<GameObject>(pathList);

            DynamicAssetGenerator.GenerateGameObjectAssetFile(Defitions.DynamicAssets.PrefabAssetFile, assets);

        }

        [MenuItem("Tools/Generate Config Asset")]
        public static void GenerateConfigAsset()
        {

            List<string> specifiedPathList = new List<string>() {
                    Defitions.DynamicAssets.ResourcePath + "Dynamic",
                };

            DynamicAssetGenerator.GenerateConfigAssets(Defitions.DynamicAssets.DynamicAssetFile, Defitions.DynamicAssets.DynamicAssetConfigFile, specifiedPathList);

        }
    }
}
