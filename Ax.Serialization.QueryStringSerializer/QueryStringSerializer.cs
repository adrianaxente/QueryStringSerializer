using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace Ax.Serialization.QueryStringSerializer
{
    public class QueryStringSerializer: IQueryStringSerializer
    {
        private IMetadataProvider _metadataProvider;

        private static readonly Lazy<ConcurrentDictionary<Type, SerializeDelegate>> _serializationDelegateDictionary =
            new Lazy<ConcurrentDictionary<Type, SerializeDelegate>>(
                () => new ConcurrentDictionary<Type, SerializeDelegate>(),
                true);

        public QueryStringSerializer(IMetadataProvider metadataProvider)
        {
            if (metadataProvider == null)
            {
                throw new ArgumentNullException(nameof(metadataProvider));
            }

            _metadataProvider = metadataProvider;
        }

        public string Serialize(object @object)
        {
            if (@object == null)
            {
                return null;
            }

            return GetSerializationDelegate(@object.GetType())(@object);
        }

        private SerializeDelegate GetSerializationDelegate(Type type)
        {
            return _serializationDelegateDictionary.Value.GetOrAdd(type, t => BuildSerializationDelegate(t));
        }

        //TODO: Change the implementation to use expression trees and Lambda.Compile
        private SerializeDelegate BuildSerializationDelegate(Type type)
        {
            var typeMetadata = _metadataProvider.GetMetadata(type);

            var result = new SerializeDelegate(o =>
            {
                var stringBuilder = new StringBuilder();

                foreach (var memberMetadata in typeMetadata.MembersMetadata)
                {
                    var memberValue =
                        memberMetadata.MemberInfo is FieldInfo
                                      ? (memberMetadata.MemberInfo as FieldInfo).GetValue(o) 
                                      : (memberMetadata.MemberInfo as PropertyInfo).GetValue(o);

                    if (memberValue == null)
                    {
                        continue;
                    }

                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append("&");
                    }

                    stringBuilder
                        .Append(memberMetadata.ValueName)
                        .Append("=")
                        .Append(memberValue);
                }

                return stringBuilder.ToString();
            });

            return result;
        }
    }
}
