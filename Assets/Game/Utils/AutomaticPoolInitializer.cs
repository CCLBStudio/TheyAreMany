using System.Collections.Generic;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

public class AutomaticPoolInitializer : MonoBehaviour
{
    [SerializeField] private List<ScriptablePool> poolsToInitialize;
    
    private void Start()
    {
        foreach (var p in poolsToInitialize)
        {
            p.Initialize();
        }

        Destroy(this);
    }
}
