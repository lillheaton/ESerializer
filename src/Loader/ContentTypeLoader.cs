using EPiServer.DataAbstraction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ESerializer.Loader
{
    public class ContentTypeLoader
    {
        private readonly IEnumerable<ContentType> _registredContentTypes;

        public ContentTypeLoader(IContentTypeRepository contentTypeRepository)
        {
            _registredContentTypes = contentTypeRepository
                .List()
                .Where(contentType => contentType.ModelType != null);
        }

        public bool TryGetContentType(Type targetModelType, out ContentType contentType)
        {
            contentType = _registredContentTypes
                .FirstOrDefault(x =>
                    x.ModelType == targetModelType ||
                    targetModelType.IsAssignableFrom(x.ModelType)
                );

            return contentType != null;
        }
    }
}
