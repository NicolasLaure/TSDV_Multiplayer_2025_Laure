using System;
using Network.Enums;

namespace Reflection.RPC
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCAttribute : Attribute
    {
        public Attributes attributes = Attributes.None;

        public RPCAttribute()
        {
            attributes = Attributes.None;
        }

        public RPCAttribute(Attributes attributes)
        {
            this.attributes = attributes;
        }
    }
}