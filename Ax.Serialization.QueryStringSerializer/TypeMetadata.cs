using System;
using System.Collections.Generic;

namespace Ax.Serialization.QueryStringSerializer
{
    public class TypeMetadata
    {
        private ISet<MemberMetadata> _memberMetadataSet;

        public TypeMetadata(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Type = type;
        }

        public ISet<MemberMetadata> MembersMetadata 
        { 
            get
            {
                return _memberMetadataSet ?? (_memberMetadataSet = new HashSet<MemberMetadata>());
            }
        }

        public Type Type { get; private set; }
    }
}
