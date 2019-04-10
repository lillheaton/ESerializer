using Castle.DynamicProxy;
using EOls.Serialization.Services.Cache;
using ESerializer.Attributes;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESerializer
{
    public class PropertyConverterContractResolver : EOls.Serialization.ContractResolver
    {
        private readonly IEnumerable<ContentType> _registredContentTypes;

        public PropertyConverterContractResolver(IContentTypeRepository contentTypeRepository, ICacheService cacheService) : base(cacheService)
        {
            _registredContentTypes = contentTypeRepository
                .List()
                .Where(contentType => contentType.ModelType != null);
        }
        public PropertyConverterContractResolver(IContentTypeRepository contentTypeRepository) : this(contentTypeRepository, null)
        {
        }

        private bool TryGetContentType(Type targetModelType, out ContentType contentType)
        {
            contentType = _registredContentTypes
                .FirstOrDefault(x =>
                    x.ModelType == targetModelType ||
                    targetModelType.IsAssignableFrom(x.ModelType)
                );

            return contentType != null;
        }

        private IEnumerable<JsonProperty> CreateJsonProperties(Type type, MemberSerialization memberSerialization)
        {
            if (!TryGetContentType(type, out ContentType contentType))
            {
                return base.CreateProperties(type, memberSerialization);
            }

            return contentType
                .PropertyDefinitions
                .Where(propDefinition => propDefinition.ExistsOnModel)
                .Select(propDefinition => contentType.ModelType.GetProperty(propDefinition.Name))
                .Where(x => x.HasAttributeWithConditionOrTrue<ApiPropertyAttribute>(attr => attr.Hide == false))
                .Select(x => base.CreateProperty(x, memberSerialization));
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            switch (type)
            {
                case Type proxyType when typeof(IProxyTargetAccessor).IsAssignableFrom(type):
                    return CreateJsonProperties(proxyType.BaseType, memberSerialization).ToList();

                case Type epiType when typeof(IContent).IsAssignableFrom(type):
                    return CreateJsonProperties(epiType, memberSerialization).ToList();

                default:
                    return base.CreateProperties(type, memberSerialization);
            }
        }
    }
}
