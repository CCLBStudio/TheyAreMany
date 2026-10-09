namespace Game.Stats
{
    public class StatModifier<T>
    {
        public T Value { get; }
        public object Source { get; }

        public StatModifier(T value, object source)
        {
            Value = value;
            Source = source;
        }
    }
}