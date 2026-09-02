using System;

namespace CCLBStudio.GlobalUpdater
{
    [Flags]
    public enum GlobalUpdateType {Update = 1, FixedUpdate = 2, LateUpdate = 4}
}