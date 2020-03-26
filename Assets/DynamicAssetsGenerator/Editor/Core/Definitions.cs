using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopTools
{
    public static partial class Defitions
    {
        public static class DynamicAssets
        {
            public const string MenuDiretoryPrefix = "Tools/Extension/";

            public static readonly string DirectoryPrefix = "Assets/DynamicAssetsGenerator/Editor/";

            public static readonly string SpriteAssetFile = DirectoryPrefix + "Samples/SpriteAsset.asset";
            public static readonly string PrefabAssetFile = DirectoryPrefix + "Samples/PrefabAsset.asset";
            public static readonly string DynamicAssetFile = DirectoryPrefix + "Samples/DynamicAsset.asset";
            public static readonly string DynamicAssetConfigFile = DynamicAssetPath + "AssetConfigs.txt";
            public static readonly string DefintionTemplateFile = DynamicAssetPath + "DefintionsTemplate.txt";
            public static readonly string DefintionCodeBlockTemplateFile = DynamicAssetPath + "DefintionCodeBlockTemplate.txt";
            public static readonly string DynamicAssetDefintionFile = DynamicAssetPath + "Defintions.cs";

            public static readonly string ResourcePath = DirectoryPrefix + "Samples/Resources/";
            public static readonly string DynamicAssetPath = DirectoryPrefix + "Dynamic/";
        }
    }

}

