using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using JsonContractSimplifier.Services.ConverterLocator;

namespace ESerializer.Converters
{
    [ServiceConfiguration(typeof(IConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentReferencePropertyConverter : IEPropertyConverter<ContentReference>
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUrlResolver _urlResolver;

        public ContentReferencePropertyConverter(IContentRepository contentRepository, IUrlResolver urlResolver)
        {
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
        }
        
        public object Convert(ContentReference target)
        {
            if (target == null) return null;

            var content = _contentRepository.Get<IContent>(target, LanguageSelector.AutoDetect(true));
            var media = content as MediaData;
            if (media != null)
            {
                return new
                {
                    media.Name,
                    media.Thumbnail,
                    media.MimeType,
                    Url = _urlResolver.GetUrl(media)
                };
            }

            return content;
        }
    }
}