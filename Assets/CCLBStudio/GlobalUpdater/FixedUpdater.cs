namespace CCLBStudio.GlobalUpdater
{
    internal class FixedUpdater : Updater<IFixedUpdate>
    {
        public static void TickFixedUpdates()
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
                    updatable.FixedTick();
                }
            }

            if (requireUpdateFlush)
            {
                FlushUpdates();
            }
        }
    }
}