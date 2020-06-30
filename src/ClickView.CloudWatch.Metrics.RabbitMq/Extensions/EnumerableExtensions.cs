namespace ClickView.CloudWatch.Metrics.RabbitMq.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> GroupInto<T>(this IEnumerable<T> list, int size)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            
            return list.Select((v, i) => new { Index = i, Value = v })
                .GroupBy(m => m.Index / size)
                .Select(s => s.Select(v => v.Value));
        }
    }
}