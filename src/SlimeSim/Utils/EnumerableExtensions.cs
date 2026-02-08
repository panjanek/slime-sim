using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Utils
{
    public static class EnumerableExtensions
    {
        public static double Median<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, double> selector)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            var data = source
                .Select(selector)
                .OrderBy(x => x)
                .ToArray();

            if (data.Length == 0)
                throw new InvalidOperationException("Sequence contains no elements");

            int mid = data.Length / 2;

            return (data.Length % 2 == 0)
                ? (data[mid - 1] + data[mid]) / 2.0
                : data[mid];
        }
    }
}
