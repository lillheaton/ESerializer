using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using JsonContractSimplifier.Services.ConverterLocator;
using System.Collections.Generic;
using System.Linq;

namespace ESerializer.Converters
{
    [ServiceConfiguration(typeof(IConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ContentAreaPropertyConverter : IEPropertyConverter<ContentArea>
    {
        private readonly IContentRepository _contentRepository;

        public ContentAreaPropertyConverter(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        public object Convert(ContentArea target)
        {
            if (target == null) return null;

            return GetContent(target.Items.Select(s => s.ContentLink)).ToArray();
        }

        private IEnumerable<object> GetContent(IEnumerable<ContentReference> references)
        {
            return references.Select(contentRef => _contentRepository.Get<IContent>(contentRef, LanguageSelector.AutoDetect(true)));
        } 
    }
}