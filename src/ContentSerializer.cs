using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using JsonContractSimplifier.Services.Cache;
using JsonContractSimplifier.Services.ConverterLocator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ESerializer
{
    [ServiceConfiguration(typeof(IContentSerializer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentSerializer : IContentSerializer
    {
        private readonly ICacheService _cacheService;        
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


    }
}
