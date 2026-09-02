using UnityEngine;

namespace CCLBStudio.Attributes
{
    public class NullInfoAttribute : PropertyAttribute
    {
        public string InfoText { get; private set; }
        public Color TextColor { get; private set; }
        
        public NullInfoAttribute(string infoText, NullInfoColor color = NullInfoColor.Yellow, bool addArrow = true)
        {
            InfoText = addArrow ? AddArrow(infoText) : infoText;
            TextColor = color switch
            {
                NullInfoColor.Gray => Color.gray,
                NullInfoColor.Black => Color.black,
                NullInfoColor.Blue => Color.blue,
                NullInfoColor.Green => Color.green,
                NullInfoColor.Cyan => Color.cyan,
                NullInfoColor.Magenta => Color.magenta,
                NullInfoColor.Red => Color.red,
                NullInfoColor.White => Color.white,
                NullInfoColor.Yellow => Color.yellow,
                _ => Color.gray
            };
        }
        
        public NullInfoAttribute(string infoText, float r, float g, float b, bool addArrow = true)
        {
            InfoText = addArrow ? AddArrow(infoText) : infoText;
            TextColor = new Color(Mathf.Clamp(r, 0f, 1f), Mathf.Clamp(g, 0f, 1f), Mathf.Clamp(b, 0f, 1f), 1f);
        }

        private string AddArrow(string text)
        {
            return $"=> {text}";
        }
    }
}