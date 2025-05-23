using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Network.Utilities;

namespace Network.Encryption
{
    public class Encrypter
    {
        public static byte[] Encrypt(int keyIv, byte[] data)
        {
            Logger.Log($"Encrypt Key = {keyIv}");
            Logger.Log($"Data size: {data.Length}");
            Aes algorithm = Aes.Create();
            algorithm.Padding = PaddingMode.Zeros;
            
            byte[] keyIvBytes = BitConverter.GetBytes(keyIv);
            byte[] formattedBytes = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, keyIvBytes[0], keyIvBytes[1], keyIvBytes[2], keyIvBytes[3] };

            ICryptoTransform encryptor = algorithm.CreateEncryptor(formattedBytes, formattedBytes);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            ms.WriteByte(data[0]);
            cs.Write(data, sizeof(byte), data.Length - sizeof(byte));
            cs.FlushFinalBlock();
            ms.Position = 0;
            return ms.ToArray();
        }

        public static byte[] Decrypt(int keyIv, byte[] data)
        {
            Logger.Log($"Decrypt Key = {keyIv}");
            Logger.Log($"Data size: {data.Length}");
            Aes algorithm = Aes.Create();
            algorithm.Padding = PaddingMode.Zeros;
            
            byte[] keyIvBytes = BitConverter.GetBytes(keyIv);
            byte[] formattedBytes = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, keyIvBytes[0], keyIvBytes[1], keyIvBytes[2], keyIvBytes[3] };

            ICryptoTransform encryptor = algorithm.CreateDecryptor(formattedBytes, formattedBytes);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            ms.WriteByte(data[0]);
            cs.Write(data, sizeof(byte), data.Length - sizeof(byte));
            cs.FlushFinalBlock();
            ms.Position = 0;
            return ms.ToArray();
        }
    }
}