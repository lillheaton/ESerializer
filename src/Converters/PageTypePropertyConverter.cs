using EPiServer.DataAbstraction;

namespace EPiSerializer.Converters
{
    public class PageTypePropertyConverter : IApiPropertyConverter<PageType>
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