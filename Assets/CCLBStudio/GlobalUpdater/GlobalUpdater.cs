namespace CCLBStudio.GlobalUpdater
{
    public static class GlobalUpdater
    {
        public static void RegisterComplexUpdatable(object listener)
        {
            if (listener is IUpdatable u) RegisterUpdatable(u);
            if (listener is IFixedUpdatable f) RegisterFixedUpdatable(f);
            if (listener is ILateUpdatable l) RegisterLateUpdatable(l);
        }

        public static void UnregisterComplexUpdatable(object listener)
        {
            if (listener is IUpdatable u) UnregisterUpdatable(u);
            if (listener is IFixedUpdatable f) UnregisterFixedUpdatable(f);
            if (listener is ILateUpdatable l) UnregisterLateUpdatable(l);
        }

        public static void RegisterUpdatable(IUpdatable u)
        {
            DefaultUpdater.RegisterUpdatable(u);
        }

        public static void UnregisterUpdatable(IUpdatable u)
        {
            DefaultUpdater.UnregisterUpdatable(u);
        }

        public static void RegisterFixedUpdatable(IFixedUpdatable u)
        {
            FixedUpdater.RegisterUpdatable(u);
        }

        public static void UnregisterFixedUpdatable(IFixedUpdatable u)
        {
            FixedUpdater.UnregisterUpdatable(u);
        }

        public static void RegisterLateUpdatable(ILateUpdatable u)
        {
            LateUpdater.RegisterUpdatable(u);
        }

        public static void UnregisterLateUpdatable(ILateUpdatable u)
        {
            LateUpdater.UnregisterUpdatable(u);
        }

        public static void RegisterUpdatableMonoBehaviour(UpdatableMonoBehaviour u)
        {
            if ((u.UpdateType & GlobalUpdateType.Update) == GlobalUpdateType.Update)
            {
                DefaultUpdater.RegisterUpdatable(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.FixedUpdate) == GlobalUpdateType.FixedUpdate)
            {
                FixedUpdater.RegisterUpdatable(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.LateUpdate) == GlobalUpdateType.LateUpdate)
            {
                LateUpdater.RegisterUpdatable(u);
            }
        }

        public static void UnregisterUpdatableMonoBehaviour(UpdatableMonoBehaviour u)
        {
            if ((u.UpdateType & GlobalUpdateType.Update) == GlobalUpdateType.Update)
            {
                DefaultUpdater.UnregisterUpdatable(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.FixedUpdate) == GlobalUpdateType.FixedUpdate)
            {
                FixedUpdater.UnregisterUpdatable(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.LateUpdate) == GlobalUpdateType.LateUpdate)
            {
                LateUpdater.UnregisterUpdatable(u);
            }
        }
    }
}
