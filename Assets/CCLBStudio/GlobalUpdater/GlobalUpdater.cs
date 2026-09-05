namespace CCLBStudio.GlobalUpdater
{
    public static class GlobalUpdater
    {
        public static void RegisterUpdatedObject(object listener)
        {
            if (listener is IUpdate u) RegisterUpdate(u);
            if (listener is IFixedUpdate f) RegisterFixedUpdate(f);
            if (listener is ILateUpdate l) RegisterLateUpdate(l);
        }

        public static void UnregisterUpdatedObject(object listener)
        {
            if (listener is IUpdate u) UnregisterUpdate(u);
            if (listener is IFixedUpdate f) UnregisterFixedUpdate(f);
            if (listener is ILateUpdate l) UnregisterLateUpdate(l);
        }

        public static void RegisterUpdate(IUpdate u)
        {
            DefaultUpdater.RegisterUpdate(u);
        }

        public static void UnregisterUpdate(IUpdate u)
        {
            DefaultUpdater.UnregisterUpdate(u);
        }

        public static void RegisterFixedUpdate(IFixedUpdate u)
        {
            FixedUpdater.RegisterUpdate(u);
        }

        public static void UnregisterFixedUpdate(IFixedUpdate u)
        {
            FixedUpdater.UnregisterUpdate(u);
        }

        public static void RegisterLateUpdate(ILateUpdate u)
        {
            LateUpdater.RegisterUpdate(u);
        }

        public static void UnregisterLateUpdate(ILateUpdate u)
        {
            LateUpdater.UnregisterUpdate(u);
        }

        public static void RegisterUpdatedMonoBehaviour(UpdatedMonoBehaviour u)
        {
            if ((u.UpdateType & GlobalUpdateType.Update) == GlobalUpdateType.Update)
            {
                DefaultUpdater.RegisterUpdate(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.FixedUpdate) == GlobalUpdateType.FixedUpdate)
            {
                FixedUpdater.RegisterUpdate(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.LateUpdate) == GlobalUpdateType.LateUpdate)
            {
                LateUpdater.RegisterUpdate(u);
            }
        }

        public static void UnregisterUpdatedMonoBehaviour(UpdatedMonoBehaviour u)
        {
            if ((u.UpdateType & GlobalUpdateType.Update) == GlobalUpdateType.Update)
            {
                DefaultUpdater.UnregisterUpdate(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.FixedUpdate) == GlobalUpdateType.FixedUpdate)
            {
                FixedUpdater.UnregisterUpdate(u);
            }
            
            if ((u.UpdateType & GlobalUpdateType.LateUpdate) == GlobalUpdateType.LateUpdate)
            {
                LateUpdater.UnregisterUpdate(u);
            }
        }
    }
}
