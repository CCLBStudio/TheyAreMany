using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.Loot.Editor
{
    public class LootDistributionTester : EditorWindow
    {
        [SerializeField] private LootService lootService;
        [SerializeField] private LootTable lootTable;
        [SerializeField] private int pickCount = 10000;
        [SerializeField] private PickerType pickerType = PickerType.PrefixSum;

        private string _result = string.Empty;
        private Vector2 _scroll;
        private GUIStyle _resultStyle;

        [MenuItem("Tools/CCLB Studio/Loot Distribution Tester")]
        private static void Open()
        {
            GetWindow<LootDistributionTester>("Loot Tester");
        }

        private void OnEnable()
        {
            if (lootService)
            {
                return;
            }

            var guid = AssetDatabase.FindAssets("t:" + nameof(LootService)).FirstOrDefault();
            if (guid != null)
            {
                lootService = AssetDatabase.LoadAssetAtPath<LootService>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }

        private void OnGUI()
        {
            _resultStyle ??= new GUIStyle(EditorStyles.textArea)
            {
                font = Font.CreateDynamicFontFromOSFont("Consolas", 12),
                wordWrap = false
            };

            lootService = (LootService)EditorGUILayout.ObjectField("Loot Service", lootService, typeof(LootService), false);
            lootTable = (LootTable)EditorGUILayout.ObjectField("Loot Table", lootTable, typeof(LootTable), false);
            pickCount = Mathf.Max(1, EditorGUILayout.IntField("Pick Count", pickCount));
            pickerType = (PickerType)EditorGUILayout.EnumPopup("Picker", pickerType);

            using (new EditorGUI.DisabledScope(!lootService || !lootTable))
            {
                if (GUILayout.Button("Run Test"))
                {
                    _result = RunTest(lootService, lootTable, pickCount, pickerType);
                    Debug.Log(_result);
                }
            }

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_result, _resultStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        public static string RunTest(LootService serviceTemplate, LootTable table, int picks, PickerType picker = PickerType.PrefixSum)
        {
            if (!serviceTemplate || !table)
            {
                return "LootService or LootTable is missing.";
            }

            if (picks <= 0)
            {
                return "Pick count must be greater than 0.";
            }

            // Work on a clone so the asset (seed, picker cache) is never modified.
            var service = Instantiate(serviceTemplate);
            service.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                service.Initialize();
                service.CurrentPickerType = picker;

                var counts = new Dictionary<string, int>();
                for (int i = 0; i < picks; i++)
                {
                    var loot = service.Pick(table);
                    var id = loot != null ? loot.GetId() : "<null>";
                    counts[id] = counts.GetValueOrDefault(id) + 1;
                }

                var expectedWeights = new Dictionary<string, float>();
                var totalWeight = 0f;
                foreach (var entry in table.Entries ?? System.Array.Empty<WeightedEntry>())
                {
                    var id = entry.loot != null ? entry.loot.GetId() : "<null>";
                    expectedWeights[id] = expectedWeights.GetValueOrDefault(id) + entry.weight;
                    totalWeight += entry.weight;
                }

                var ids = counts.Keys.Union(expectedWeights.Keys).ToList();
                var nameWidth = Mathf.Max(8, ids.Count > 0 ? ids.Max(id => id.Length) : 0);

                var sb = new StringBuilder();
                sb.AppendLine($"Table: {table.name} | Picks: {picks} | Picker: {picker}");
                sb.AppendLine($"{"Loot".PadRight(nameWidth)} | {"Count",8} | {"Actual %",9} | {"Expected %",10} | {"Delta",8}");
                sb.AppendLine(new string('-', nameWidth + 47));

                foreach (var id in ids.OrderByDescending(id => counts.GetValueOrDefault(id)))
                {
                    var count = counts.GetValueOrDefault(id);
                    var actual = 100f * count / picks;
                    var expected = totalWeight > 0f ? 100f * expectedWeights.GetValueOrDefault(id) / totalWeight : 0f;
                    sb.AppendLine($"{id.PadRight(nameWidth)} | {count,8} | {actual,8:F2}% | {expected,9:F2}% | {(actual - expected).ToString("+0.00;-0.00;0.00"),7}%");
                }

                return sb.ToString();
            }
            finally
            {
                DestroyImmediate(service);
            }
        }
    }
}
