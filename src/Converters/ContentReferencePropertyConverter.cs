using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace ESerializer.Converters
{
    public class ContentReferencePropertyConverter : IApiPropertyConverter<ContentReference>
    {
        private readonly IContentRepository _contentRepository;
        private readonly IUrlResolver _urlResolver;

        public ContentReferencePropertyConverter(IContentRepository contentRepository, IUrlResolver urlResolver)
        {
            _contentRepository = contentRepository;
            _urlResolver = urlResolver;
        }

        public ContentReferencePropertyConverter() : this(
            ServiceLocator.Current.GetInstance<IContentRepository>(),
            ServiceLocator.Current.GetInstance<IUrlResolver>())
        { }

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