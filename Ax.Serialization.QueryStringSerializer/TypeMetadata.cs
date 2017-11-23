using System;
using System.Collections.Generic;

namespace Ax.Serialization.QueryStringSerializer
{
    public class TypeMetadata
    {
        private ISet<MemberMetadata> _memberMetadataSet;

        public ISet<MemberMetadata> MembersMetadata 
        { 
            get
            {
                return _memberMetadataSet ?? (_memberMetadataSet = new HashSet<MemberMetadata>());
            }
        }
    }
}
