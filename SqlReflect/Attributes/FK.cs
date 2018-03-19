using System;

namespace SqlReflect.Attributes
{
    public class FKAttribute : Attribute
    {
        public string Name { get; set; }

        public FKAttribute(string name)
        {
            Name = name;
        }
    }
}
