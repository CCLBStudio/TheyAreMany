using System.Collections.Generic;
using CCLBStudio.DependencyInjection;
using Services;
using UnityEngine;
using Random = System.Random;

namespace CCLBStudio.Loot
{
    [CreateAssetMenu(fileName = "LootService", menuName = "CCLB Studio/Loot/Service")]
    [Provide]
    public partial class LootService : AppService
    {
        public PickerType CurrentPickerType { get => _currentPickerType; set => SetCurrentPicker(value); }
        
        [Tooltip("If true, a random seed will be used to initialize the random number generator.")]
        [SerializeField] private bool randomSeed = true;
        [SerializeField] private int seed = 12345;
        [SerializeField] private PickerType defaultPicker = PickerType.PrefixSum;

        private Random _rng;
        private PickerType _currentPickerType;
        private readonly Dictionary<LootTable, IPicker> _pickers = new();

        public override void Initialize()
        {
            seed = randomSeed ? UnityEngine.Random.Range(0, int.MaxValue) : seed;
            _rng = new Random(seed);
            _currentPickerType = defaultPicker;
        }

        public ILoot Pick(LootTable table)
        {
            if (!table)
            {
                Debug.LogError("LootTable is null.");
                return null;
            }

            return GetPicker(table).Pick(_rng);
        }
        
        private void SetCurrentPicker(PickerType pickerType)
        {
            _currentPickerType = pickerType;
        }
        
        private IPicker GetPicker(LootTable table)
        {
            if (!_pickers.TryGetValue(table, out var picker))
            {
                picker = CreatePicker(_currentPickerType);
                picker.Rebuild(table.Entries);
                _pickers[table] = picker;
                return picker;
            }
            
            if(picker.Type != _currentPickerType)
            {
                picker = CreatePicker(_currentPickerType);
                picker.Rebuild(table.Entries);
                _pickers[table] = picker;
            }
            
            return picker;
        }
        
        private static IPicker CreatePicker(PickerType pickerType)
        {
            return pickerType switch
            {
                PickerType.PrefixSum => new PrefixSumPicker(),
                PickerType.AliasSum => new AliasSumPicker(),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    }
}