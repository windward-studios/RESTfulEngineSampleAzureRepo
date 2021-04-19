using System.Collections.Generic;

namespace RESTfulEngine.Models.version1
{
    public class UtilsV1
    {
        public static T[] MakeArray<T>(IList<T> list)
        {
            T[] array = new T[list.Count];
            list.CopyTo(array, 0);
            return array;
        }
    }
}