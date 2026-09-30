using DamageNumbersPro;
using UnityEngine;

public class DebugAndTest : MonoBehaviour
{
    public DamageNumber damageNumberPrefab;
    public float delay = .1f;
    
    private float _timer;
    
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= delay)
        {
            int value = Random.Range(1, 100);
            var damageNumber = damageNumberPrefab.Spawn(transform.position, value);
            damageNumber.SetColor(value > 50 ? Color.red : Color.green);
            _timer = 0f;
        }
    }
}
