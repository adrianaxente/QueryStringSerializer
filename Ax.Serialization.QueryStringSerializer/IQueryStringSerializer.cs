using System;
namespace Ax.Serialization.QueryStringSerializer
{
    public interface IQueryStringSerializer
    {
        string Serialize(object @object);
    }
}
