using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    /// <summary>
    /// Dispatches each spawn to a location according to the <see cref="WaveSpawnGroup"/> of its enemy.
    /// Enemies without group, or whose group has no route, use the default location.
    /// </summary>
    public sealed class WaveSpawnLocationRouter : WaveSpawnLocation
    {
        [Serializable]
        private struct Route
        {
            public WaveSpawnGroup group;
            public WaveSpawnLocation location;
        }

        [SerializeField] private WaveSpawnLocation defaultLocation;
        [SerializeField] private Route[] routes = Array.Empty<Route>();

        private WavePlan cachedPlan;
        private int[] cachedLocalIndices;

        public override Pose GetSpawnPose(in WaveSpawnRequest request)
        {
            if (request.Wave == null)
                throw new ArgumentException("Invalid spawn request.", nameof(request));
            var location = Resolve(request.Enemy);
            int localIndex = GetLocalIndices(request.Wave)[request.SpawnIndex];
            return location.GetSpawnPose(new WaveSpawnRequest(request.Wave, request.SpawnIndex, localIndex));
        }

        public WaveSpawnLocation Resolve(WaveEnemyDefinition enemy)
        {
            if (routes == null)
                throw new InvalidOperationException($"Spawn router '{name}' has no route list.");

            WaveSpawnLocation location = null;
            var group = enemy != null ? enemy.SpawnGroup : null;
            if (group != null)
            {
                foreach (var route in routes)
                {
                    if (route.group == group)
                    {
                        location = route.location;
                        if (location == null)
                            throw new InvalidOperationException($"Spawn router '{name}' has no location for group '{group.name}'.");
                        break;
                    }
                }
            }

            if (location == null)
                location = defaultLocation;
            if (location == null)
                throw new InvalidOperationException(
                    $"Spawn router '{name}' has no location for enemy '{(enemy != null ? enemy.name : "null")}' and no default location.");
            if (location == this)
                throw new InvalidOperationException($"Spawn router '{name}' cannot route to itself.");
            return location;
        }

        // Spawns sent to the same location get consecutive local indices so round-robin stays even per location.
        private int[] GetLocalIndices(WavePlan plan)
        {
            if (cachedPlan == plan && cachedLocalIndices != null)
                return cachedLocalIndices;

            var counters = new Dictionary<WaveSpawnLocation, int>();
            var indices = new int[plan.Spawns.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                var location = Resolve(plan.Spawns[i].Enemy);
                counters.TryGetValue(location, out int count);
                indices[i] = count;
                counters[location] = count + 1;
            }

            cachedPlan = plan;
            cachedLocalIndices = indices;
            return indices;
        }
    }
}
