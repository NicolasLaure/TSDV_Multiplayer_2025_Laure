using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;

public static class ByteFormat
{
    public static byte[] ToByteArray(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();

        bf.Serialize(ms, obj);
        return ms.ToArray();
    }

    public static byte[] GetVector3Bytes(Vector3 input)
    {
        byte[] vec3Bytes = new byte[12];
        Buffer.BlockCopy(BitConverter.GetBytes(input.X), 0, vec3Bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.Y), 0, vec3Bytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.Z), 0, vec3Bytes, 8, 4);

        return vec3Bytes;
    }

    public static Vector3 GetVector3FromBytes(byte[] bytes, int offset)
    {
        List<byte[]> components = new List<byte[]>();
        for (int i = 0; i < 3; i++)
        {
            byte[] componentBytes = new byte[4];
            Buffer.BlockCopy(bytes, offset + i * 4, componentBytes, 0, 4);
            components.Add(componentBytes);
        }

        return new Vector3(BitConverter.ToSingle(components[0]), BitConverter.ToSingle(components[1]),
            BitConverter.ToSingle(components[2]));
    }

    public static Vector4 GetVector4FromBytes(byte[] bytes, int offset)
    {
        List<byte[]> components = new List<byte[]>();
        for (int i = 0; i < 4; i++)
        {
            byte[] componentBytes = new byte[4];
            Buffer.BlockCopy(bytes, offset + i * 4, componentBytes, 0, 4);
            components.Add(componentBytes);
        }

        return new Vector4(BitConverter.ToSingle(components[0]), BitConverter.ToSingle(components[1]),
            BitConverter.ToSingle(components[2]), BitConverter.ToSingle(components[3]));
    }
}