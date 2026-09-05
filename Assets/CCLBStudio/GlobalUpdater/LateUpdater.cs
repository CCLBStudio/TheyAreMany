namespace CCLBStudio.GlobalUpdater
{
    internal class LateUpdater : Updater<ILateUpdate>
    {
        public static void TickLateUpdates()
        {
            int count = PrepareBuffer();
            
            for (int i = 0; i < count; i++)
            {
                var updatable = buffer[i];
                if (IsNull(updatable))
                {
                    requireUpdateFlush = true;
                    continue;
                }

                if (updates.Contains(updatable))
                {
                    updatable.LateTick();
                }
            }

            if (requireUpdateFlush)
            {
                FlushUpdates();
            }
        }
    }
}