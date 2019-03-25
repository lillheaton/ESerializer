using EOls.Serialization.Services.Cache;
using EOls.Serialization.Services.ConverterLocator;
using EPiSerializer.Loader;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;

namespace EPiSerializer
{
    [ServiceConfiguration(typeof(IContentSerializer), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentSerializer : IContentSerializer
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ICacheService _cacheService;
        private readonly IConverterLocatorService _converterLocatorService;

        public ContentSerializer(ICacheService cacheService, IContentTypeRepository contentTypeRepository)
        {
            _cacheService = cacheService;
            _converterLocatorService = new ConverterLoader();

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new PropertyConverterContractResolver(contentTypeRepository) { ShouldCache = false },
                Converters = new[] { new EOls.Serialization.TargetsJsonConverter(_converterLocatorService) },
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public string Serialize(object target)
        {
            return JsonConvert.SerializeObject(target, _jsonSerializerSettings);
        }
    }
}
