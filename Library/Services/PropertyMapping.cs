using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Services
{
    public class PropertyMapping<TSource, TDeestination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            _mappingDictionary = mappingDictionary;
        }
    }
}
