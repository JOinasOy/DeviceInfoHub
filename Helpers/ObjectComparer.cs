using System;
using System.Reflection;

public static class ObjectComparer
{
    public static bool AreObjectsEqual<T>(T obj1, T obj2)
    {
        if (obj1 == null && obj2 == null)
            return true;
        if (obj1 == null || obj2 == null)
            return false;

        var type = typeof(T);
        foreach (PropertyInfo property in type.GetProperties())
        {
            if (property.CanRead)
            {
                object val1 = property.GetValue(obj1);
                object val2 = property.GetValue(obj2);
                if (!Equals(val1, val2))
                    return false;
            }
        }
        return true;
    }
}