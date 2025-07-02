using System;
using Network.Enums;

namespace Reflection.RPC
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RPCAttribute : Attribute
    {
        public Attributes attributes;
        private int[] route;

        public int[] Route
        {
            get => route;
            set => route = value;
        }

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