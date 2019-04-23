using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ESerializer
{
    public interface IContentSerializer
    {
        JsonSerializerSettings JsonSerializerSettings { get; }
        string Serialize(object target);
        Dictionary<string, object> Convert(object target);
        void AddExtraOptInAttribute<TAttribute>() where TAttribute : Attribute;    
    }
}
