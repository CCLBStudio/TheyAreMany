using System.Linq;
using CCLBStudio.DependencyInjection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Injector))]
public class InjectorEditor : Editor
{
    private SerializedProperty _autoProviders;
    private string _niceAutoRegisterName;
    private ReorderableList _autoRegisterList;

    private SerializedProperty _autoConsumers;
    private string[] _toExclude;
    private bool _showAutoRegister;

    private void OnEnable()
    {
        _autoProviders = serializedObject.FindProperty(Injector.AutoRegisterProperty);
        _autoConsumers = serializedObject.FindProperty(Injector.AutoConsumersProperty);
        _niceAutoRegisterName = ObjectNames.NicifyVariableName(Injector.AutoRegisterProperty);
        _toExclude = new [] { Injector.AutoRegisterProperty, Injector.AutoConsumersProperty };
        
        _autoRegisterList = new ReorderableList(serializedObject, _autoProviders, true, true, true, true);

        _autoRegisterList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Automatically Registered Providers");
        };

        _autoRegisterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            var element = _autoProviders.GetArrayElementAtIndex(index);
            rect.height -= EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.ObjectField(rect, element, GUIContent.none);
        };

        _autoRegisterList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
            ShowProviderMenu(buttonRect);
        };

        _autoRegisterList.onRemoveCallback = (ReorderableList l) => {
            _autoProviders.DeleteArrayElementAtIndex(l.index);
            serializedObject.ApplyModifiedProperties();
        };
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, _toExclude);
        
        _showAutoRegister = EditorGUILayout.Foldout(_showAutoRegister, _niceAutoRegisterName, true);
        if (_showAutoRegister)
        {
            EditorGUI.indentLevel++;
            _autoRegisterList.DoLayoutList();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_autoConsumers);
        serializedObject.ApplyModifiedProperties();
    }
    
    private void ShowProviderMenu(Rect rect)
    {
        var menu = new GenericMenu();

        var allProviders = AssetDatabase.FindAssets("t:ScriptableObject")
            .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(obj => obj is IDependencyProvider)
            .Distinct()
            .ToList();

        if (allProviders.Count == 0)
        {
            menu.AddDisabledItem(new GUIContent("No provider found"));
        }
        else
        {
            foreach (var provider in allProviders)
            {
                string providerName = provider.name;
                menu.AddItem(new GUIContent(providerName), false, () => AddProviderToList(provider));
            }
        }

        menu.DropDown(rect);
    }

    private void AddProviderToList(ScriptableObject provider)
    {
        serializedObject.Update();
        _autoProviders.arraySize++;
        _autoProviders.GetArrayElementAtIndex(_autoProviders.arraySize - 1).objectReferenceValue = provider;
        serializedObject.ApplyModifiedProperties();
    }
}
