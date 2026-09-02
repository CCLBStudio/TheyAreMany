using System;
using System.Collections.Generic;
using System.IO;
using CCLBStudio.ScriptableValue;
using ReaaliStudio.Utils.Extensions;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Reaali/Systems/Scriptable Value/Database", fileName = "ScriptableValueDatabase")]
public class ScriptableValueDatabase : ScriptableObject
{
    #region Editor

    public static string ValueTemplateProperty => nameof(scriptableValueTemplate);
    public static string ValueListTemplateProperty => nameof(scriptableValueListTemplate);

    #endregion
    
    public TemplateFile ScriptableValueTemplate => scriptableValueTemplate;
    public TemplateFile ScriptableValueListTemplate => scriptableValueListTemplate;
    
    [SerializeField] private TemplateFile scriptableValueTemplate;
    [SerializeField] private TemplateFile scriptableValueListTemplate;
    
    private static string _baseFolderPath;
    private static string _assetCreationPath;
    
    public static T CreateValueAsset<T>(string assetName, bool ping = true) where T : BaseScriptableValue
    {
        string path = CheckDirectory(typeof(T));
        return CreateScriptableAsset<T>(assetName, path, ping);
    }

    public static BaseScriptableValue CreateValueAsset(Type valueType, string assetName, bool ping = true)
    {
        string path = CheckDirectory(valueType);
        return CreateScriptableAsset(valueType, assetName, path, ping);
    }

    public static string GetBaseFolderPath()
    {
        if (!string.IsNullOrEmpty(_baseFolderPath))
        {
            return _baseFolderPath;
        }

        var dbAsset = EditorExtender.LoadScriptableAsset<ScriptableValueDatabase>();
        string dbPath = AssetDatabase.GetAssetPath(dbAsset);
        _baseFolderPath = Path.GetDirectoryName(Path.GetDirectoryName(dbPath)) + "/";
        return _baseFolderPath;
    }

    public static string GetAssetCreationPath()
    {
        if (!string.IsNullOrEmpty(_assetCreationPath))
        {
            return _assetCreationPath;
        }

        string baseFolderPath = GetBaseFolderPath();
        _assetCreationPath = baseFolderPath + "ValueObjects/";
        return _assetCreationPath;
    }

    private static string CheckDirectory(Type valueType)
    {
        if (!typeof(BaseScriptableValue).IsAssignableFrom(valueType))
        {
            Debug.LogError($"Type {valueType.Name} do not derives from {nameof(BaseScriptableValue)}");
            return string.Empty;
        }
        
        string typeName = valueType.Name;
        if (!typeName.EndsWith("s"))
        {
            typeName += "s";
        }

        string path = GetAssetCreationPath() + typeName;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
    
    private static T CreateScriptableAsset<T>(string assetName, string folderPath, bool ping = true) where T : ScriptableObject
    {
        T newSo = CreateInstance<T>();
        assetName = $"/{(string.IsNullOrEmpty(assetName) ? $"New{typeof(T).Name}" : assetName)}.asset";

        if (!folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }
        string fullPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + assetName);
        AssetDatabase.CreateAsset(newSo, fullPath);
        AssetDatabase.SaveAssets();
        if (ping)
        {
            EditorGUIUtility.PingObject(newSo);
        }

        AssetDatabase.Refresh();
        return newSo;
    }

    private static BaseScriptableValue CreateScriptableAsset(Type valueType, string assetName, string folderPath, bool ping = true)
    {
        if (!typeof(BaseScriptableValue).IsAssignableFrom(valueType))
        {
            Debug.LogError($"Type {valueType.Name} do not derives from {nameof(BaseScriptableValue)}");
            return null;
        }

        BaseScriptableValue newSo = (BaseScriptableValue)CreateInstance(valueType);
        assetName = $"/{(string.IsNullOrEmpty(assetName) ? $"New{valueType.Name}" : assetName)}.asset";
        
        if (!folderPath.EndsWith("/"))
        {
            folderPath += "/";
        }
        string fullPath = AssetDatabase.GenerateUniqueAssetPath(folderPath + assetName);
        AssetDatabase.CreateAsset(newSo, fullPath);
        AssetDatabase.SaveAssets();
        if (ping)
        {
            EditorGUIUtility.PingObject(newSo);
        }

        AssetDatabase.Refresh();
        return newSo;
    }

    public void RefreshAllTemplateIndexes()
    {
        scriptableValueTemplate.RefreshIndexes();
        scriptableValueListTemplate.RefreshIndexes();
    }

    [Serializable]
    public struct TemplateFile
    {
        [TextArea(12, 100)]
        public string fileContent;
        [TextArea(12, 100)]
        public string fileContentTest;
        
        [HideInInspector]
        public List<int> scriptNameStartIndexes;
        [HideInInspector]
        public List<int> niceScriptNameStartIndexes;
        [HideInInspector]
        public List<int> typeStartIndexes;

        public void RefreshIndexes()
        {
            fileContentTest = fileContent;
            scriptNameStartIndexes = FindAllIndexes("#SCRIPTNAME#");
            niceScriptNameStartIndexes = FindAllIndexes("#NICESCRIPTNAME#");
            typeStartIndexes = FindAllIndexes("TYPE");

            int length = "#SCRIPTNAME#".Length;
            for (int i = 0; i < scriptNameStartIndexes.Count; i++)
            {
                fileContentTest = fileContentTest.Remove(scriptNameStartIndexes[i], length);
                for (int j = i + 1; j < scriptNameStartIndexes.Count; j++)
                {
                    scriptNameStartIndexes[j] -= length;
                }
            }
            
            // foreach (var index in scriptNameStartIndexes)
            // {
            //     fileContentTest = fileContentTest.Remove(index, "#SCRIPTNAME#".Length);
            // }
        }

        private List<int> FindAllIndexes(string search)
        {
            List<int> indexes = new List<int>();
            int index = 0;

            while ((index = fileContent.IndexOf(search, index, StringComparison.Ordinal)) != -1)
            {
                indexes.Add(index);
                index += search.Length;
            }

            return indexes;
        }
    }
}
