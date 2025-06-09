using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumUtils
{
    /// <summary>
    /// Returns all values of a given Enum type as a List.
    /// </summary>
    public static List<T> GetAllEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }
}