using System;
using Network.Enums;

namespace Reflection
{
    public class Sync : Attribute
    {
        public Attributes attribs = Attributes.None;

        public Sync()
        {
            attribs = Attributes.None;
        }

        public Sync(Attributes attributes)
        {
            attribs = attributes;
        }
    }
}