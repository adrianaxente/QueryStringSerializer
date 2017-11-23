using System;
namespace Ax.Serialization.QueryStringSerializer
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class QueryStringValueAttribute: Attribute
    {
        public QueryStringValueAttribute(string name = null)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
