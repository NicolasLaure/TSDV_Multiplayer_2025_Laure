using System;
using Network.Enums;

namespace Utils
{
    public class PrimitiveUtils
    {
        public static PrimitiveType GetObjectType(object obj)
        {
            TypeCode objType = Convert.GetTypeCode(obj);
            PrimitiveType primitiveType;
            switch (objType)
            {
                case TypeCode.Boolean:
                    primitiveType = PrimitiveType.TypeBool;
                    break;
                case TypeCode.Byte:
                    primitiveType = PrimitiveType.TypeByte;
                    break;
                case TypeCode.SByte:
                    primitiveType = PrimitiveType.TypeSbyte;
                    break;
                case TypeCode.Int16:
                    primitiveType = PrimitiveType.TypeShort;
                    break;
                case TypeCode.UInt16:
                    primitiveType = PrimitiveType.TypeUshort;
                    break;
                case TypeCode.Int32:
                    primitiveType = PrimitiveType.TypeInt;
                    break;
                case TypeCode.UInt32:
                    primitiveType = PrimitiveType.TypeUint;
                    break;
                case TypeCode.Int64:
                    primitiveType = PrimitiveType.TypeLong;
                    break;
                case TypeCode.UInt64:
                    primitiveType = PrimitiveType.TypeUlong;
                    break;
                case TypeCode.Single:
                    primitiveType = PrimitiveType.TypeFloat;
                    break;
                case TypeCode.Double:
                    primitiveType = PrimitiveType.TypeDouble;
                    break;
                case TypeCode.Decimal:
                    primitiveType = PrimitiveType.TypeDecimal;
                    break;
                case TypeCode.Char:
                    primitiveType = PrimitiveType.TypeChar;
                    break;
                case TypeCode.String:
                    primitiveType = PrimitiveType.TypeString;
                    break;
                default:
                    primitiveType = PrimitiveType.NonPrimitive;
                    break;
            }

            return primitiveType;
        }
    }
}