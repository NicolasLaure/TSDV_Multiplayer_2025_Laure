using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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
        Buffer.BlockCopy(BitConverter.GetBytes(input.x), 0, vec3Bytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.y), 0, vec3Bytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.z), 0, vec3Bytes, 8, 4);

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

    public static byte[] Get4X4Bytes(Matrix4x4 matrix4X4)
    {
        byte[] matrixBytes = new byte[sizeof(float) * 12];
        Buffer.BlockCopy(GetVector4Bytes(matrix4X4.GetRow(0)), 0, matrixBytes, 0, 4);
        Buffer.BlockCopy(GetVector4Bytes(matrix4X4.GetRow(1)), 0, matrixBytes, 0, 4);
        Buffer.BlockCopy(GetVector4Bytes(matrix4X4.GetRow(2)), 0, matrixBytes, 0, 4);
        return matrixBytes;
    }

    public static Matrix4x4 Get4X4FromBytes(byte[] bytes, int offset)
    {
        Matrix4x4 trs = Matrix4x4.identity;
        trs.SetRow(0, GetVector4FromBytes(bytes, offset));
        trs.SetRow(1, GetVector4FromBytes(bytes, offset + sizeof(float) * 4));
        trs.SetRow(2, GetVector4FromBytes(bytes, offset + sizeof(float) * 8));
        return trs;
    }

    public static byte[] GetVector4Bytes(Vector4 input)
    {
        byte[] vectorBytes = new byte[sizeof(float) * 4];
        Buffer.BlockCopy(BitConverter.GetBytes(input.x), 0, vectorBytes, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.y), 0, vectorBytes, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.z), 0, vectorBytes, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(input.w), 0, vectorBytes, 12, 4);
        return vectorBytes;
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