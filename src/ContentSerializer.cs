using Castle.DynamicProxy;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using ESerializer.Factory;
using ESerializer.Loader;
using ESerializer.Utils;
using JsonContractSimplifier.Services.Cache;
using JsonContractSimplifier.Services.ConverterLocator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESerializer
{
    [ServiceConfiguration(typeof(IContentSerializer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentSerializer : IContentSerializer
    {
        private readonly ICacheService _cacheService;
        private readonly ContentTypeLoader _contentTypeLoader;
        private readonly PropertyConverterContractResolver _propertyConverterContractResolver;
        private readonly List<Type> _extraOptInAttributeTypes;        

        public JsonSerializerSettings JsonSerializerSettings { get; private set; }

        public ContentSerializer(
            IConverterLocatorService converterLocatorService, 
            ICacheService cacheService, 
            IContentTypeRepository contentTypeRepository)
        {
            _cacheService = cacheService;            
            _extraOptInAttributeTypes = new List<Type>();
            _contentTypeLoader = new ContentTypeLoader(contentTypeRepository);

            _propertyConverterContractResolver = new PropertyConverterContractResolver(
                converterLocatorService, 
                contentTypeRepository, 
                cacheService)
            {
                ShouldCache = cacheService != null
            };

            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = _propertyConverterContractResolver,                
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public void AddExtraOptInAttribute<TAttribute>() where TAttribute : Attribute
        {
            if (_extraOptInAttributeTypes.Contains(typeof(TAttribute)))
            {
                return;
            }
            _extraOptInAttributeTypes.Add(typeof(TAttribute));

            _propertyConverterContractResolver.ExtraOptInAttributes = _extraOptInAttributeTypes.ToArray();
        }

        public string Serialize(object target)
        {
            return JsonConvert.SerializeObject(target, JsonSerializerSettings);
        }

        /// <summary>
        /// Uses heavy none cached reflection. Should be used with caution
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Dictionary<string, object> Convert(object target)
        {
            Type targetType = target.GetType();
            if (typeof(IProxyTargetAccessor).IsAssignableFrom(targetType))
            {
                targetType = targetType.BaseType;
            }

            var properties = TypeUtils
                .GetProperties(targetType, _contentTypeLoader)
                .Where(x => x.HasAttribute<JsonIgnoreAttribute>() == false);

            return ObjectUtils.ConvertProperties(target, properties);            
        }
    }
}
