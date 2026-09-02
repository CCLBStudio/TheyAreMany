namespace Utils
{
    public static class StringExtender
    {
        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str[..1].ToUpper() + str[1..];
        }
    }
}
