using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using ESerializer.Loader;
using JsonContractSimplifier.Services.Cache;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ESerializer
{
    [ServiceConfiguration(typeof(IContentSerializer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ICacheService _cacheService;        
        private readonly PropertyConverterContractResolver _propertyConverterContractResolver;
        private readonly List<Type> _extraOptInAttributeTypes;

        public ContentSerializer(ICacheService cacheService, IContentTypeRepository contentTypeRepository)
        {
            _cacheService = cacheService;            
            _extraOptInAttributeTypes = new List<Type>();

            _propertyConverterContractResolver = new PropertyConverterContractResolver(
                new ConverterLoader(), 
                contentTypeRepository, 
                cacheService)
            {
                ShouldCache = cacheService != null
            };

            _jsonSerializerSettings = new JsonSerializerSettings
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
            return JsonConvert.SerializeObject(target, _jsonSerializerSettings);
        }


    }
}
