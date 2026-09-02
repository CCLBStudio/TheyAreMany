using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    internal class DefaultUpdater : Updater<IUpdatable>
    {
        public static void TickUpdatables()
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
                    requireUpdatableFlush = true;
                    continue;
                }

                if (updatables.Contains(updatable))
                {
                    updatable.Tick();
                }
                
                buffer[i] = null;
            }

            if (requireUpdatableFlush)
            {
                FlushUpdatables();
            }
        }
    }
}