//using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public sealed class SpriteAsset
{
    public string Name;
    public Sprite Asset;
}


public class SpriteAssets : ScriptableObject
{
    public List<SpriteAsset> Assets = new List<SpriteAsset>();

    public void AddAssets(Dictionary<string, Sprite> assets)
    {
        if (assets == null)
            return;

        foreach (KeyValuePair<string, Sprite> asset in assets)
        {
            SpriteAsset sameAsset = Assets.Find((_asset) => _asset.Name == asset.Key);

            if (sameAsset == null)
            {
                Assets.Add(new SpriteAsset() { Name = asset.Key, Asset = asset.Value });
            }
            else
            {
                Debug.LogError("assets contains key " + asset.Key);
            }

        }
    }
}