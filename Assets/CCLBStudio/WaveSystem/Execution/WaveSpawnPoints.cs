using System;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    public sealed class WaveSpawnPoints : WaveSpawnLocation
    {
        [SerializeField] private Transform[] points = Array.Empty<Transform>();
        [SerializeField] private WaveSpawnPointSelection selection;

        public override Pose GetSpawnPose(in WaveSpawnRequest request)
        {
            if (request.Wave == null)
                throw new ArgumentException("Invalid spawn request.", nameof(request));
            if (points == null || points.Length == 0)
                throw new InvalidOperationException($"Spawn provider '{name}' has no points.");
            foreach (var point in points)
            {
                if (point == null)
                    throw new InvalidOperationException($"Spawn provider '{name}' contains a missing point.");
            }
            if (selection != WaveSpawnPointSelection.RoundRobin && selection != WaveSpawnPointSelection.Random)
                throw new InvalidOperationException($"Spawn provider '{name}' has an invalid selection mode.");

            int index = selection == WaveSpawnPointSelection.Random
                ? UnityEngine.Random.Range(0, points.Length)
                : request.LocalIndex % points.Length;
            var selected = points[index];
            return new Pose(selected.position, selected.rotation);
        }
    }
}
