namespace CCLBStudio.GlobalUpdater
{
    internal class LateUpdater : Updater<ILateUpdatable>
    {
        public static void TickLateUpdatables()
        {
            int count = PrepareBuffer();
            
            for (int i = 0; i < count; i++)
            {
                var updatable = buffer[i];
                if (IsNull(updatable))
                {
                    requireUpdatableFlush = true;
                    continue;
                }

                if (updatables.Contains(updatable))
                {
                    updatable.LateTick();
                }
            }

            if (requireUpdatableFlush)
            {
                FlushUpdatables();
            }
        }
    }
}