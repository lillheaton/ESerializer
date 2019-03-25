using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace EPiSerializer.Converters
{
    public class ContentAreaPropertyConverter : IApiPropertyConverter<ContentArea>
    {
        public object Convert(ContentArea target)
        {
            if (target == null) return null;

            return GetContent(target.Items.Select(s => s.ContentLink)).ToArray();
        }

        private IEnumerable<object> GetContent(IEnumerable<ContentReference> references)
        {
            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            return references.Select(contentRef => repo.Get<IContent>(contentRef, LanguageSelector.AutoDetect()));            
        } 
    }
}