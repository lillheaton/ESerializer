using System;

namespace EPiSerializer
{
    public interface IContentSerializer
    {
        string Serialize(object target);
        void AddExtraOptInAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
