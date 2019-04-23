using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESerializer.Utils
{
    public class ObjectUtils
    {
        public static Dictionary<string, object> ConvertProperties(object target, IEnumerable<PropertyInfo> properties)
        {
            return properties
                .Select(prop =>
                {
                    var jsonPropName = prop
                        .GetCustomAttributes<JsonPropertyAttribute>()
                        ?.FirstOrDefault()
                        ?.PropertyName;

                    return new Tuple<string, object>
                    (
                        string.IsNullOrWhiteSpace(jsonPropName) ? prop.Name : jsonPropName,
                        prop.GetValue(target)
                    );
                })
                .ToDictionary(x => x.Item1, x => x.Item2);
        }
    }
}
