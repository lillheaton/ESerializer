using EOls.Serialization.Services.Cache;
using EOls.Serialization.Services.ConverterLocator;
using EPiSerializer.Loader;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EPiSerializer
{
    [ServiceConfiguration(typeof(IContentSerializer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ICacheService _cacheService;
        private readonly IConverterLocatorService _converterLocatorService;
        private readonly PropertyConverterContractResolver _propertyConverterContractResolver;
        private readonly List<Type> _extraOptInAttributeTypes;

        public ContentSerializer(ICacheService cacheService, IContentTypeRepository contentTypeRepository)
        {
            _cacheService = cacheService;
            _converterLocatorService = new ConverterLoader();
            _extraOptInAttributeTypes = new List<Type>();

            _propertyConverterContractResolver = new PropertyConverterContractResolver(contentTypeRepository, cacheService)
            {
                ShouldCache = cacheService != null
            };

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = _propertyConverterContractResolver,
                Converters = new[] { new EOls.Serialization.TargetsJsonConverter(_converterLocatorService) },
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
