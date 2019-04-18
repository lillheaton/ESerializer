using EPiServer.Core;
using EPiServer.DataAbstraction;
using System;
using System.Linq;
using System.Reflection;

namespace ESerializer.Test.SetupHelpers
{
    public class ContentTypeSetupHelper
    {        
        public static ContentType CreateContentType<T>(Type interfaceTypeToCreatePropertiesFrom) where T : PageData
        {
            var propDefinitionItems = interfaceTypeToCreatePropertiesFrom
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => new PropertyDefinition { Name = x.Name, ExistsOnModel = true });

            // Assumes all base class properties are ExistsOnModel = false
            propDefinitionItems = propDefinitionItems
                .Concat(
                    typeof(PageData)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(x => new PropertyDefinition { Name = x.Name, ExistsOnModel = false }));
            
            var contentType = new ContentType
            {
                ModelType = typeof(T)                
            };

            foreach (var item in propDefinitionItems)
            {
                contentType.PropertyDefinitions.Add(item);
            }

            return contentType;
        }
    }
}
