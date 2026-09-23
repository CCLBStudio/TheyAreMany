using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class XJectorBootstrapBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Rebuild();
        }

        [InitializeOnEnterPlayMode]
        private static void Rebuild()
        {
            var bootstrapper = GetBootstrapper();
            BuildProvideRegistry(bootstrapper);
            BuildInjectRegistry(bootstrapper);
            SetBoostrapDirty(bootstrapper);
        }

        private static XJectorSOBootstrap GetBootstrapper()
        {
            string folderPath = "Assets/CCLBStudio/DependencyInjection/Resources";
            string registryPath = $"{folderPath}/{XJectorSOBootstrap.Name}.asset";
        
            var bootstrapper = AssetDatabase.LoadAssetAtPath<XJectorSOBootstrap>(registryPath);
            if (!bootstrapper)
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                    AssetDatabase.CreateFolder("Assets/CCLBStudio/DependencyInjection", "Resources");
                
                bootstrapper = ScriptableObject.CreateInstance<XJectorSOBootstrap>();
                AssetDatabase.CreateAsset(bootstrapper, registryPath);
            }

            return bootstrapper;
        }

        private static void BuildProvideRegistry(XJectorSOBootstrap bootstrapper)
        {
            var typesWithProvide = TypeCache.GetTypesWithAttribute<ProvideAttribute>()
                .Where(t => t.IsSubclassOf(typeof(ScriptableObject)));

            var instancesToProvide = new List<ScriptableObject>();

            foreach (var type in typesWithProvide)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;
                    if (asset)
                    {
                        instancesToProvide.Add(asset);
                    }
                }
            }

            bootstrapper.toProvide = instancesToProvide.ToArray();
        }

        private static void BuildInjectRegistry(XJectorSOBootstrap bootstrapper)
        {
            var typesDeclaringInjectFields = new HashSet<Type>(
                TypeCache.GetFieldsWithAttribute<InjectAttribute>().Select(f => f.DeclaringType));

            var typesWithInject = TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.ContainsGenericParameters && HasInjectMember(t, typesDeclaringInjectFields));

            var instancesToInject = new HashSet<ScriptableObject>();

            foreach (var type in typesWithInject)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;
                    if (asset)
                    {
                        instancesToInject.Add(asset);
                    }
                }
            }
            bootstrapper.toInject = instancesToInject.ToArray();
        }

        // Walks the hierarchy because private members of base classes aren't returned by reflection on the derived type.
        private static bool HasInjectMember(Type type, HashSet<Type> typesDeclaringInjectFields)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            for (var current = type; current != null && current != typeof(ScriptableObject); current = current.BaseType)
            {
                if (typesDeclaringInjectFields.Contains(current))
                    return true;

                if (current.GetProperties(flags).Any(p => p.IsDefined(typeof(InjectAttribute), false)))
                    return true;
            }

            return false;
        }

        private static void SetBoostrapDirty(XJectorSOBootstrap bootstrapper)
        {
            EditorUtility.SetDirty(bootstrapper);
            AssetDatabase.SaveAssets();
        }
    }
}