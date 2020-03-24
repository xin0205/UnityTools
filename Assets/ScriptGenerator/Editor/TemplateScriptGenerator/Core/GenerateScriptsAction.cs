using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

/// <summary>
/// 重命名文件名，同时修改类名
/// </summary>
class GenerateScriptsAction : EndNameEditAction
{
    public Dictionary<string, string> contentReplaceDict = new Dictionary<string, string>();

    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        Object obj = CreateAssetFormTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }

    internal static Object CreateAssetFormTemplate(string pathName, string resourceFile)
    {
        string fullName = Path.GetFullPath(pathName);
        StreamReader reader = new StreamReader(resourceFile);
        string content = reader.ReadToEnd();
        reader.Close();

        string fileName = Path.GetFileNameWithoutExtension(pathName);
        content = content.Replace("__Name__", fileName);

        StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8);
        writer.Write(content);
        writer.Close();

        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}
