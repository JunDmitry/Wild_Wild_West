using System;

public static class ThrowIf
{
    public static void Null(object obj, string paramName, string message = null)
    {
        if (obj == null)
            throw new ArgumentNullException(paramName, message);
    }

    public static void Invalid(bool condition, string message)
    {
        if (condition)
            throw new InvalidOperationException(message);
    }
}