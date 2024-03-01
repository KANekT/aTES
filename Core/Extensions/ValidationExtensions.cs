using System.Collections;

namespace Core.Extensions;

public static class ValidationExtensions
{
    public static T ThrowIfNull<T>(this T? cl) where T : class
    {
        if (cl == null)
        {
            throw new ArgumentException("Argument cannot be null");
        }

        return cl;
    }

    public static T ThrowIfNullOrEmpty<T>(this T list) where T : IDictionary
    {
        if (list == null)
        {
            throw new ArgumentException("Argument cannot be null");
        }

        if(list.Count == 0)
        {
            throw new ArgumentException("Argument cannot be empty");
        }

        return list;
    }
}