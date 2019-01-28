using Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;


namespace Library.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }

            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }

            //separated by "," so split it
            var orderByAfterSplit = orderBy.Split(',');

            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                //pode conter espaços vindos do url. Não dá para meter no var
                var trimmeOrderByClause = orderByClause.Trim();

                var orderDescending = trimmeOrderByClause.EndsWith(" desc");

                //remove " asc" or " desc" to get the property name to look in the mapping dictionary
                var indexOfFirstSpace = trimmeOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmeOrderByClause : trimmeOrderByClause.Remove(indexOfFirstSpace);

                //find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"key mapping for {propertyName} is missing");
                }

                //get the PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];

                if (propertyMappingValue == null)
                {
                    throw new ArgumentException("propertyMappingValue");
                }

                //senão order by fica ao contrário
                foreach ( var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    //revert sort if necessary
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }
                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }

            }
            return source;

        }
    }
}
