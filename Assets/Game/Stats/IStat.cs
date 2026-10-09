namespace Game.Stats
{
    public interface IStat<T>
    {
        T Value { get; }
        
        void AddModifier(StatModifier<T> modifier);
        bool RemoveModifier(StatModifier<T> modifier);
        bool RemoveAllModifiersFromSource(object source);
    }
}