using System.Linq;
using CCLBStudio.ScriptableValue;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFov : MonoBehaviour
{
    [SerializeField] private PlayerFacadeListValue players;
    [SerializeField] [Min(30f)] private float minFov = 60f;
    [SerializeField] [Min(30f)] private float maxFov = 90f;
    [SerializeField] [Min(0f)] private float distanceForMinFov = 10f;
    [SerializeField] [Min(0f)] private float distanceForMaxFov = 25f;

    private float FovRange => Mathf.Abs(maxFov - minFov);
    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (players.Value.Count <= 1)
        {
            return;
        }
        
        var points = Vector3Utils.GetFarthestPoints(players.Value.Select(x => x.transform.position).ToList());
        float normalizedRange = Mathf.Clamp01((Vector3.Distance(points.Item1, points.Item2) - distanceForMinFov) / distanceForMaxFov);

        _cam.fieldOfView = minFov + normalizedRange * FovRange;
    }
}
