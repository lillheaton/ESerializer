using EPiServer.Core;
using EPiServer.DataAbstraction;
using ESerializer.Attributes;
using ESerializer.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESerializer.Factory
{
    public class TypeUtils
    {
        /// <summary>
        /// If target type is defined in EPiServer's content type, filter based on defined properties. 
        /// Fallback if is IContentData and no ContentType (local block) filter properties on IContentData
        /// Otherwise return properties as is
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contentTypeLoader"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetProperties(Type targetType, ContentTypeLoader contentTypeLoader)
        {            
            var properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            if (!contentTypeLoader.TryGetContentType(targetType, out ContentType contentType))
            {
                if (typeof(IContentData).IsAssignableFrom(targetType))
                {
                    var contentDataPropertyNames = typeof(ContentData).GetProperties().Select(x => x.Name);

                    // If type does not have contentType and is ContentData, filter IContentData properties
                    return properties
                        .Where(prop => !contentDataPropertyNames.Contains(prop.Name));
                }

                return properties;
            }

            return properties
                .Where(prop =>                    
                    contentType.PropertyDefinitions.Any(x => x.Name == prop.Name && x.ExistsOnModel) ||
                    prop.HasAttribute<ESerializePropertyAttribute>()
                );
        }    
    }
}
