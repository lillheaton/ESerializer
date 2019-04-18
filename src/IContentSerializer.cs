using Newtonsoft.Json;
using System;

namespace ESerializer
{
    public interface IContentSerializer
    {
        JsonSerializerSettings JsonSerializerSettings { get; }
        string Serialize(object target);
        void AddExtraOptInAttribute<TAttribute>() where TAttribute : Attribute;        
    }
}
