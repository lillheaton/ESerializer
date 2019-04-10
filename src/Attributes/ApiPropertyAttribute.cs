using System;

namespace ESerializer.Attributes
{
    public class ApiPropertyAttribute : Attribute
    {
        public bool Hide { get; set; }
    }
}
