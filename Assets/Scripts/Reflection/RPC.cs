using System;
using Network.Enums;

namespace Reflection
{
    public class RPC : Attribute
    {
        public Attributes attributes;

        public RPC()
        {
            attributes = Attributes.None;
        }

        public RPC(Attributes attributes)
        {
            this.attributes = this.attributes;
        }
    }
}