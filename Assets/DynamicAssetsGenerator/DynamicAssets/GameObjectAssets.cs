using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class GameObjectAsset
{
    public string Name;
    public GameObject Asset;
}

public class GameObjectAssets : ScriptableObject
{
    public List<GameObjectAsset> Assets = new List<GameObjectAsset>();

    public void AddAssets(Dictionary<string, GameObject> assets)
    {
        if (assets == null)
            return;

        foreach (KeyValuePair<string, GameObject> asset in assets)
        {
            GameObjectAsset sameAsset = Assets.Find((_asset) => _asset.Name == asset.Key);

            if (sameAsset == null)
            {
                Assets.Add(new GameObjectAsset() { Name = asset.Key, Asset = asset.Value });
            }
            else
            {
                Debug.LogError("assets contains key " + asset.Key);
            }

        }
    }
}