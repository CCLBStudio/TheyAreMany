using System;
using CCLBStudio.StringIds;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public sealed class ContainerId : StringId
    {
        public static readonly ContainerId Empty = new();
        public static readonly ContainerId Loading = new("Loading");
        public static readonly ContainerId Main = new("Main");

        public ContainerId() { }
        public ContainerId(string containerId) : base(containerId) { }
    }
}
