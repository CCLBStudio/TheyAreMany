using System;
using CCLBStudio.StringIds;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public sealed class ContainerId : StringId
    {
        public static readonly ContainerId Empty = new();

        public ContainerId() { }
        public ContainerId(string containerId) : base(containerId) { }
    }
}