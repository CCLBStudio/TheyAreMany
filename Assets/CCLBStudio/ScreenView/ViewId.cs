using System;
using CCLBStudio.StringIds;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public sealed class ViewId : StringId
    {
        public static readonly ViewId Empty = new();
        public static readonly ViewId Player1Health = new("Player1Health");
        public static readonly ViewId GameOver = new("GameOver");

        public ViewId() { }
        public ViewId(string viewId) : base(viewId) { }
    }
}
