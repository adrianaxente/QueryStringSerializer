using System;
using System.Reflection;

namespace Ax.Serialization.QueryStringSerializer
{
    public class MemberMetadata
    {
        public MemberMetadata(MemberInfo memberInfo, string valueName = null)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            MemberInfo = memberInfo;
            ValueName = valueName;
        }

        public MemberInfo MemberInfo { get; private set; }

        public string ValueName { get; private set; }
    }
}
