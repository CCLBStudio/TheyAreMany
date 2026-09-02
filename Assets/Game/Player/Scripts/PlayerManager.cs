using CCLBStudio.ScriptableValue;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerFacadeListValue players;

    private void Awake()
    {
        players.Clear();
    }
}
