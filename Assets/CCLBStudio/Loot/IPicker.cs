namespace CCLBStudio.Loot
{
    public interface IPicker
    {
        public PickerType Type { get; }
        void Rebuild(WeightedEntry[] entries);
        ILoot Pick(System.Random rng);
    }
}