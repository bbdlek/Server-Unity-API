using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace MVS.Helios.Utility
{
    public static class HeliosUtility
    {
        public static ulong Compute64BitHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // 문자열을 바이트 배열로 변환
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                // 해시 계산
                byte[] hash = sha256.ComputeHash(bytes);
                // 해시의 첫 8바이트를 64비트 정수로 변환
                ulong hash64Bit = BitConverter.ToUInt64(hash, 0);
                return hash64Bit;
            }
        }
        
        public static byte[] SerializeParameters(params object[] parameters)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, parameters);
                return memoryStream.ToArray();
            }
        }

        public static object[] DeserializeParameters(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                return (object[])binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}