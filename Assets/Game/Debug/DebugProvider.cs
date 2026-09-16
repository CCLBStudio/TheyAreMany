using CCLBStudio.DependencyInjection;
using UnityEngine;

[Provide]
public partial class DebugProvider
{
    public void Print()
    {
        Debug.Log("Hello from DebugProvider");
    }
}
