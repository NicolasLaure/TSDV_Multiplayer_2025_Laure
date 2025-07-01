using System;
using Network.Enums;

namespace Reflection
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCAttribute : Attribute
    {
        public Attributes attributes;

        public RPCAttribute()
        {
            attributes = Attributes.None;
        }

        public RPCAttribute(Attributes attributes)
        {
            this.attributes = this.attributes;
        }
    }
}