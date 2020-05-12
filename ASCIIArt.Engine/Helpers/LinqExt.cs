using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCIIArt.Engine.Helpers
{
    internal static class LinqExt
    {
        public static T MaxBy<T, Key>(this IEnumerable<T> @this, Func<T, Key> keySelector, IComparer<Key> comparer = null)
        {
            comparer = comparer ?? Comparer<Key>.Default;
            var result = @this.FirstOrDefault();
            foreach(var item in @this.Skip(1))
            {
                if (comparer.Compare(keySelector(item), keySelector(result)) > 0)
                {
                    result = item;
                }
            }
            return result;
        }
    }
}
