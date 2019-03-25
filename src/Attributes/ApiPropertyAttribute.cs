using System;

namespace EPiSerializer.Attributes
{
    public class ApiPropertyAttribute : Attribute
    {
        public bool Hide { get; set; }
        public bool Cache { get; set; } = true;
    }
}
