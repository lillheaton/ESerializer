using EPiServer.SpecializedProperties;
using System.Linq;

namespace ESerializer.Converters
{
    public class LinkItemCollectionPropertyConverter : IApiPropertyConverter<LinkItemCollection>
    {
        public object Convert(LinkItemCollection target)
        {
            return target?.Select(s => new { s.Title, s.Text, s.Href, s.Attributes, s.Target }).ToArray();
        }
    }
}