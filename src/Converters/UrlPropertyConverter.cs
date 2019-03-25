using EPiServer;

namespace EPiSerializer.Converters
{
    public class UrlPropertyConverter : IApiPropertyConverter<Url>
    {
        public object Convert(Url target)
        {
            return target?.ToString();
        }
    }
}