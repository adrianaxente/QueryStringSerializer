using System;
using System.Reflection;
using System.Linq;

namespace Ax.Serialization.QueryStringSerializer
{
    public class QueryStringValueMetadataProvider: IMetadataProvider
    {
        public QueryStringValueMetadataProvider()
        {
        }

        public TypeMetadata GetMetadata(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException();
            }

            var result = new TypeMetadata(type);

            var membersMetadata = 
                type.GetRuntimeFields()
                    .Cast<MemberInfo>()
                    .Concat(type.GetRuntimeProperties()
                                .Cast<MemberInfo>())
                    .Select(GetMemberMetadata)
                    .Where(mi => mi != null);

            foreach (var memberMetadata in membersMetadata)
            {
                result.MembersMetadata.Add(memberMetadata);
            }

            return result;
        }

        private MemberMetadata GetMemberMetadata(MemberInfo memberInfo)
        {
            var attribute =
                memberInfo
                    .GetCustomAttributes()
                    .FirstOrDefault(ca => ca is QueryStringValueAttribute);

            return attribute != null 
                    ? new MemberMetadata(
                        memberInfo, 
                        (attribute as QueryStringValueAttribute).Name)
                    : null;
            
        }
    }
}
