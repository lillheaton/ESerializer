using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using JsonContractSimplifier.Services.ConverterLocator;
using System.Linq;

namespace ESerializer.Converters
{
    [ServiceConfiguration(typeof(IConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class LinkItemCollectionPropertyConverter : IEPropertyConverter<LinkItemCollection>
    {
        public object Convert(LinkItemCollection target)
        {
            return target?.Select(s => new { s.Title, s.Text, s.Href, s.Attributes, s.Target }).ToArray();
        }
    }
}