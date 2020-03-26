//using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public sealed class Assets<T> where T : UnityEngine.Object
{
    public string Name;
    public T Asset;
}

public class MultiAssets<T> : ScriptableObject where T : UnityEngine.Object//OdinSerializer.SerializedScriptableObject where T : UnityEngine.Object
{
    public List<Assets<T>> Assets = new List<Assets<T>>();

    public void AddAssets(Dictionary<string, T> assets)
    {
        if (assets == null)
            return;

        foreach (KeyValuePair<string, T> asset in assets)
        {
            Assets<T> sameAsset = Assets.Find((_asset) => _asset.Name == asset.Key);

            if (sameAsset == null)
            {
                Assets.Add(new Assets<T>() { Name = asset.Key, Asset = asset.Value });
            }
            else {
                Debug.LogError("assets contains key " + asset.Key);
            }

        }
    }


}


