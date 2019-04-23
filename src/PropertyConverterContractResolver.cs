using Castle.DynamicProxy;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using ESerializer.Attributes;
using ESerializer.Factory;
using ESerializer.Loader;
using JsonContractSimplifier.Services.Cache;
using JsonContractSimplifier.Services.ConverterLocator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ESerializer
{
    public class PropertyConverterContractResolver : JsonContractSimplifier.ContractResolver
    {
        private readonly ContentTypeLoader _contentTypeLoader;

        public PropertyConverterContractResolver(
            IConverterLocatorService converterLocatorService,
            IContentTypeRepository contentTypeRepository, 
            ICacheService cacheService) 
            : base(converterLocatorService, cacheService)
        {
            _contentTypeLoader = new ContentTypeLoader(contentTypeRepository);            
        }

        public PropertyConverterContractResolver(
            IConverterLocatorService converterLocatorService, 
            IContentTypeRepository contentTypeRepository) 
            : this(converterLocatorService, contentTypeRepository, null)
        {
        }
        
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(IProxyTargetAccessor).IsAssignableFrom(objectType))
            {
                return base.CreateContract(objectType.BaseType);
            }
            return base.CreateContract(objectType);
        }

        protected override bool ShouldIgnore(JsonProperty jsonProperty, MemberInfo member, MemberSerialization memberSerialization)
        {
            var attributes = jsonProperty
                    .AttributeProvider
                    .GetAttributes(false);

            if (attributes.Any(attr => attr.GetType() == typeof(ESerializeIgnoreAttribute)))
            {
                return true;
            }

            return base.ShouldIgnore(jsonProperty, member, memberSerialization);
        }

        private IEnumerable<JsonProperty> CreateJsonProperties(Type type, MemberSerialization memberSerialization)
        {
            if(memberSerialization == MemberSerialization.OptIn)
            {
                return base.CreateProperties(type, memberSerialization);
            }
            
            if (!_contentTypeLoader.TryGetContentType(type, out ContentType contentType))
            {
                if (typeof(IContentData).IsAssignableFrom(type))
                {
                    return TypeUtils
                        .GetProperties(type, _contentTypeLoader)
                        .Where(x => 
                            x.HasAttribute<JsonIgnoreAttribute>() == false &&
                            x.HasAttribute<ESerializeIgnoreAttribute>() == false
                        ) 
                        .Select(x => base.CreateProperty(x, memberSerialization));                    
                }
                
                return base.CreateProperties(type, memberSerialization);
            }
            
            return TypeUtils
                .GetProperties(type, _contentTypeLoader)
                .Where(x => 
                    x.HasAttribute<JsonIgnoreAttribute>() == false &&
                    x.HasAttribute<ESerializeIgnoreAttribute>() == false
                )
                .Select(x => base.CreateProperty(x, memberSerialization));
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            switch (type)
            {
                case Type proxyType when typeof(IProxyTargetAccessor).IsAssignableFrom(type):
                    return CreateJsonProperties(proxyType.BaseType, memberSerialization).ToList();

                case Type epiType when typeof(IContentData).IsAssignableFrom(type):
                    return CreateJsonProperties(epiType, memberSerialization).ToList();
                    
                default:
                    return base.CreateProperties(type, memberSerialization);
            }
        }
    }
}
