using EPiServer;
using EPiServer.ServiceLocation;
using JsonContractSimplifier.Services.ConverterLocator;

namespace ESerializer.Converters
{
    [ServiceConfiguration(typeof(IConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class UrlPropertyConverter : IEPropertyConverter<Url>
    {
        public object Convert(Url target)
        {
            return target?.ToString();
        }
    }
}