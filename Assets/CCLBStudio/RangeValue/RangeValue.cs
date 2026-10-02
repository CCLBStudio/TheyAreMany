using System;
using Random = UnityEngine.Random;

public interface IRangeValue<out T>
{
    T GetRandom();
}

[Serializable]
public struct RangeFloat : IRangeValue<float>
{
    public float min;
    public float max;

    public RangeFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float GetRandom() => Random.Range(min, max);
}

[Serializable]
public struct RangeInt : IRangeValue<int>
{
    public int min;
    public int max;

    public RangeInt(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public readonly int GetRandom() 
    {
        return Random.Range(min, max + 1); 
    }
}
