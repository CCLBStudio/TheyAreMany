namespace CCLBStudio.GlobalUpdater
{
    internal class FixedUpdater : Updater<IFixedUpdatable>
    {
        public static void TickFixedUpdatables()
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
                    updatable.FixedTick();
                }
            }

            if (requireUpdatableFlush)
            {
                FlushUpdatables();
            }
        }
    }
}