using System;
using System.Collections.Generic;
using System.Text;
using Network_dll.Messages.Data;
using Network.Enums;

namespace Network.Messages;

public class PrimitiveMessage : Message<PrimitiveData>
{
    public PrimitiveData data;

    public PrimitiveMessage(PrimitiveData data, Attributes attributes)
    {
        messageType = MessageType.Primitive;
        attribs = attributes;
        messageId++;
        this.data = data;
    }

    public PrimitiveMessage(byte[] message)
    {
        messageType = MessageType.Primitive;
        attribs = Attributes.None;
        this.data = Deserialize(message);
    }

    public override byte[] Serialize()
    {
        List<byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes((short)data.type));
        bytes.AddRange(BitConverter.GetBytes(data.routeLength));
        for (int i = 0; i < data.routeLength; i++)
        {
            bytes.AddRange(BitConverter.GetBytes(data.route[i]));
        }

        switch (data.type)
        {
            case PrimitiveType.TypeSbyte:
            case PrimitiveType.TypeByte:
                bytes.Add((byte)data.obj);
                break;
            case PrimitiveType.TypeShort:
                bytes.AddRange(BitConverter.GetBytes((short)data.obj));
                break;
            case PrimitiveType.TypeUshort:
                bytes.AddRange(BitConverter.GetBytes((ushort)data.obj));
                break;
            case PrimitiveType.TypeInt:
                bytes.AddRange(BitConverter.GetBytes((int)data.obj));
                break;
            case PrimitiveType.TypeUint:
                bytes.AddRange(BitConverter.GetBytes((uint)data.obj));
                break;
            case PrimitiveType.TypeLong:
                bytes.AddRange(BitConverter.GetBytes((long)data.obj));
                break;
            case PrimitiveType.TypeUlong:
                bytes.AddRange(BitConverter.GetBytes((ulong)data.obj));
                break;
            case PrimitiveType.TypeFloat:
                bytes.AddRange(BitConverter.GetBytes((float)data.obj));
                break;
            case PrimitiveType.TypeDouble:
                bytes.AddRange(BitConverter.GetBytes((double)data.obj));
                break;
            case PrimitiveType.TypeDecimal:
                int[] bits = decimal.GetBits((decimal)data.obj);
                for (int i = 0; i < bits.Length; i++)
                    bytes.AddRange(BitConverter.GetBytes(bits[0]));
                break;
            case PrimitiveType.TypeBool:
                bytes.AddRange(BitConverter.GetBytes((bool)data.obj));
                break;
            case PrimitiveType.TypeChar:
                bytes.AddRange(BitConverter.GetBytes((char)data.obj));
                break;
            case PrimitiveType.TypeString:
                string message = data.obj.ToString();
                bytes.AddRange(BitConverter.GetBytes(message.Length));
                for (int i = 0; i < message.Length; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes(message[i]));
                }

                break;
        }

        return GetFormattedData(bytes.ToArray());
    }

    public override PrimitiveData Deserialize(byte[] message)
    {
        byte[] payload = ExtractPayload(message);
        PrimitiveType type = (PrimitiveType)BitConverter.ToInt16(payload);
        PrimitiveData newData;
        newData.type = type;
        int offset = sizeof(short);
        newData.routeLength = BitConverter.ToInt32(payload, offset);
        offset += sizeof(int);
        List<int> route = new List<int>();
        for (int i = 0; i < newData.routeLength; i++)
        {
            route.Add(BitConverter.ToInt32(payload, offset));
            offset += sizeof(int);
        }

        newData.route = route.ToArray();

        switch (newData.type)
        {
            case PrimitiveType.TypeSbyte:
                newData.obj = Convert.ToSByte(payload[offset]);
                break;
            case PrimitiveType.TypeByte:
                newData.obj = payload[offset];
                break;
            case PrimitiveType.TypeShort:
                newData.obj = BitConverter.ToInt16(payload, offset);
                break;
            case PrimitiveType.TypeUshort:
                newData.obj = BitConverter.ToUInt16(payload, offset);
                break;
            case PrimitiveType.TypeInt:
                newData.obj = BitConverter.ToInt32(payload, offset);
                break;
            case PrimitiveType.TypeUint:
                newData.obj = BitConverter.ToUInt32(payload, offset);
                break;
            case PrimitiveType.TypeLong:
                newData.obj = BitConverter.ToInt64(payload, offset);
                break;
            case PrimitiveType.TypeUlong:
                newData.obj = BitConverter.ToUInt64(payload, offset);
                break;
            case PrimitiveType.TypeFloat:
                newData.obj = BitConverter.ToSingle(payload, offset);
                break;
            case PrimitiveType.TypeDouble:
                newData.obj = BitConverter.ToDouble(payload, offset);
                break;
            case PrimitiveType.TypeDecimal:
                List<int> ints = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    ints.Add(BitConverter.ToInt32(payload, offset));
                    offset += sizeof(int);
                }

                newData.obj = new decimal(ints.ToArray());
                break;
            case PrimitiveType.TypeBool:
                newData.obj = BitConverter.ToBoolean(payload, offset);
                break;
            case PrimitiveType.TypeChar:
                newData.obj = BitConverter.ToChar(payload, offset);
                break;
            case PrimitiveType.TypeString:
                int size = BitConverter.ToInt32(payload, offset);
                offset += sizeof(int);
                char[] text = Encoding.Unicode.GetChars(payload, offset, size);
                newData.obj = new string(text);
                break;
            default:
                newData.obj = null;
                break;
        }

        return newData;
    }
}