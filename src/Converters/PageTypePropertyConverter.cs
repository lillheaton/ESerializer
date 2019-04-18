using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using JsonContractSimplifier.Services.ConverterLocator;

namespace ESerializer.Converters
{
    [ServiceConfiguration(typeof(IConverter), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PageTypePropertyConverter : IEPropertyConverter<PageType>
    {
        public object Convert(PageType target)
        {
            if (target == null) return null;
            return
                new
                {
                    target.ID,
                    target.Name,
                    target.FullName,
                    target.ModelType,
                    target.DefaultMvcController,
                    target.DefaultMvcPartialView,
                    target.DefaultWebFormTemplate
                };
        }
    }
}