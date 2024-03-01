namespace Core.Extensions;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }

        if (str.Length == 1)
        {
            return str.ToLower();
        }

        return char.ToLowerInvariant(str[0]) + str[1..];
    }

}