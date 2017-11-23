using System;
namespace Ax.Serialization.QueryStringSerializer
{
    public interface IMetadataProvider
    {
        TypeMetadata GetMetadata(Type type);
    }
}
