using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InteractiveDirectory.Services
{
    public static class LinqDynamicMultiSortingUtility
    {
        /// <summary>
        /// This utility allows us to sort an IEnumerable object on multiple fields in the object.
        /// Code from CodeProject.com:
        /// http://www.codeproject.com/Articles/280952/Multiple-Column-Sorting-by-Field-Names-Using-Linq
        /// Note: If a property name shows up more than once in the "sortExpressions", only the 
        /// first takes effect.
        /// </summary>
        /// <typeparam name="T">An IEnumerable type.</typeparam>
        /// <param name="data">And list of data of type T to sort.</param>
        /// <param name="sortExpressions">List of Tuples: the first item of the tuples is the field name, the second item of the tuples is the sorting order (asc/desc).  Both are case sensitive.</param>
        /// <returns>Sorted list of type T.</returns>
        public static IEnumerable<T> MultipleSort<T>(this IEnumerable<T> data, List<Tuple<string, string>> sortExpressions)
        {
            // No sorting needed
            if ((sortExpressions == null) || (sortExpressions.Count <= 0)) return data;

            // Let us sort it
            IEnumerable<T> query = from item in data select item;
            IOrderedEnumerable<T> orderedQuery = null;

            for (int i = 0; i < sortExpressions.Count; i++)
            {
                // We need to keep the loop index, not sure why it is altered by the Linq.
                var index = i;
                Func<T, object> expression = item => item.GetType()
                                .GetProperty(sortExpressions[index].Item1)
                                .GetValue(item, null);

                if (sortExpressions[index].Item2 == "asc")
                {
                    orderedQuery = (index == 0) ? query.OrderBy(expression)
                      : orderedQuery.ThenBy(expression);
                }
                else
                {
                    orderedQuery = (index == 0) ? query.OrderByDescending(expression)
                             : orderedQuery.ThenByDescending(expression);
                }
            }

            query = orderedQuery;

            return query;
        }
    }
}