using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    internal class DefaultUpdater : Updater<IUpdate>
    {
        public static void TickUpdates()
        {
            int count = PrepareBuffer();
            
            for (int i = 0; i < count; i++)
            {
                if (i >= buffer.Length)
                {
                    Debug.LogError($"Index {i} is out of buffer range! Buffer length: {buffer.Length}");
                    continue;
                }
                
                var updatable = buffer[i];

                if (IsNull(updatable))
                {
                    requireUpdateFlush = true;
                    continue;
                }

                if (updates.Contains(updatable))
                {
                    updatable.Tick();
                }
                
                buffer[i] = null;
            }

            if (requireUpdateFlush)
            {
                FlushUpdates();
            }
        }
    }
}