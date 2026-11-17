using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Game.Utils;
using UnityEditor;
using UnityEngine;

public class StatColorsEditorWindow : EditorWindow
{
    [Serializable]
    public class ColorEntry
    {
        public string name;
        [ColorUsage(true, true)]
        public Color color;

        public ColorEntry(string name, Color color)
        {
            this.name = name;
            this.color = color;
        }
    }

    private List<ColorEntry> entries = new List<ColorEntry>();
    private Vector2 scrollPosition;
    private string targetPath = "Assets/Scripts/StatColors.cs";

    [MenuItem("Tools/Stat Colors Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<StatColorsEditorWindow>("Stat Colors Editor");
        window.minSize = new Vector2(500, 350);
        window.LoadOrCreate();
    }

    private void OnEnable()
    {
        FindScriptPath();
        LoadOrCreate();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Stat Colors Manager", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Target file: {targetPath}", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < entries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Sanitize identifier to ensure a valid C# variable name
            entries[i].name = EditorGUILayout.TextField(entries[i].name, GUILayout.Width(140));
            entries[i].name = SanitizeIdentifier(entries[i].name);

            // Color field with showAlpha: true and hdr: true
            entries[i].color = EditorGUILayout.ColorField(GUIContent.none, entries[i].color, false, true, true, GUILayout.Width(70));

            // HEX string (RGBA) and TextMeshPro rich text preview
            string hex = ColorUtility.ToHtmlStringRGBA(entries[i].color);
            EditorGUILayout.LabelField($"#{hex}", EditorStyles.miniBoldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField($"<color=#{hex}>■ TextMeshPro</color>", new GUIStyle(EditorStyles.label) { richText = true });

            // Remove button
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕", GUILayout.Width(26), GUILayout.Height(18)))
            {
                entries.RemoveAt(i);
                GUIUtility.ExitGUI();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.Space(6);

        // Add entry button
        if (GUILayout.Button("+ Add Color", GUILayout.Height(24)))
        {
            entries.Add(new ColorEntry("NewColor", Color.white));
        }

        EditorGUILayout.Space(6);

        // Global actions
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Reload from File", GUILayout.Height(28)))
        {
            LoadOrCreate();
        }

        GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
        if (GUILayout.Button("Apply & Save Script", GUILayout.Height(28)))
        {
            SaveToScript();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);
    }

    private void FindScriptPath()
    {
        string[] guids = AssetDatabase.FindAssets($"{nameof(ColorConstants)} t:MonoScript");
        if (guids.Length > 0)
        {
            targetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        }
    }

    private void LoadOrCreate()
    {
        entries.Clear();

        if (File.Exists(targetPath))
        {
            string content = File.ReadAllText(targetPath);
            // Regex matching R, G, B, and optional A channels (including floats > 1.0 for HDR)
            Regex regex = new Regex(@"public\s+static\s+readonly\s+Color\s+([a-zA-Z0-9_]+)\s*=\s*new\s+Color\(\s*([0-9.]+f?)\s*,\s*([0-9.]+f?)\s*,\s*([0-9.]+f?)\s*(?:,\s*([0-9.]+f?)\s*)?\);");
            MatchCollection matches = regex.Matches(content);

            foreach (Match match in matches)
            {
                string varName = match.Groups[1].Value;
                float r = float.Parse(match.Groups[2].Value.Replace("f", ""), System.Globalization.CultureInfo.InvariantCulture);
                float g = float.Parse(match.Groups[3].Value.Replace("f", ""), System.Globalization.CultureInfo.InvariantCulture);
                float b = float.Parse(match.Groups[4].Value.Replace("f", ""), System.Globalization.CultureInfo.InvariantCulture);
                float a = match.Groups[5].Success 
                    ? float.Parse(match.Groups[5].Value.Replace("f", ""), System.Globalization.CultureInfo.InvariantCulture) 
                    : 1f;

                entries.Add(new ColorEntry(varName, new Color(r, g, b, a)));
            }
        }

        // Initialize with default values if file is missing or empty
        if (entries.Count == 0)
        {
            entries.Add(new ColorEntry("Damage", new Color(0.91f, 0.39f, 0.13f, 1f)));
            entries.Add(new ColorEntry("FinalDamagePct", new Color(1.00f, 1.00f, 1.00f, 1f)));
            entries.Add(new ColorEntry("AbilityPower", new Color(0.00f, 0.90f, 1.00f, 1f)));
            entries.Add(new ColorEntry("CritChance", new Color(1.00f, 0.20f, 0.20f, 1f)));
            entries.Add(new ColorEntry("CritDamage", new Color(0.74f, 0.12f, 0.24f, 1f)));
            entries.Add(new ColorEntry("AttackSpeed", new Color(1.00f, 0.82f, 0.25f, 1f)));
            entries.Add(new ColorEntry("ProjectileSpeed", new Color(0.35f, 0.94f, 0.71f, 1f)));
            entries.Add(new ColorEntry("Prismatic", new Color(0.69f, 0.26f, 1.00f, 1f)));
        }
    }

    private void SaveToScript()
    {
        string directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string className = !string.IsNullOrEmpty(targetPath) ? Path.GetFileNameWithoutExtension(targetPath) : "ColorConstants";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated via 'Tools > Stat Colors Editor'");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");

        foreach (var entry in entries)
        {
            string hex = ColorUtility.ToHtmlStringRGBA(entry.color);
            // Serializes RGBA components preserving alpha and HDR intensities
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "    public static readonly Color {0,-18} = new Color({1:0.00}f, {2:0.00}f, {3:0.00}f, {4:0.00}f); // #{5}",
                entry.name, entry.color.r, entry.color.g, entry.color.b, entry.color.a, hex));
        }

        sb.AppendLine();
        sb.AppendLine("    public static string Format(string text, Color color)");
        sb.AppendLine("    {");
        sb.AppendLine("        return $\"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{text}</color>\";");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        File.WriteAllText(targetPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Script {targetPath} has been updated. Unity will now recompile.", "OK");
    }

    private string SanitizeIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "Stat";
        string sanitized = Regex.Replace(input, @"[^\w]", "");
        if (char.IsDigit(sanitized[0])) sanitized = "_" + sanitized;
        return sanitized;
    }
}