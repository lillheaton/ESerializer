using EPiServer.DataAbstraction;
using Moq;
using System;
using System.Linq;
using System.Reflection;

namespace EPiSerializer.Test.SetupHelpers
{
    public class ContentTypeSetupHelper
    {        
        public static ContentType CreateContentType<T>(Type interfaceTypeToCreatePropertiesFrom)
        {
            var propDefinitionItems = interfaceTypeToCreatePropertiesFrom
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => new PropertyDefinition { Name = x.Name, ExistsOnModel = true });
            
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
