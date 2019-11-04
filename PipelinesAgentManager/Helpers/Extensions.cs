namespace PipelinesAgentManager
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool HasValue(this string str) => !str.IsNullOrEmpty();
    }
}