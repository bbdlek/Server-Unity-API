using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MVS.Helios.Utility
{
    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(T obj)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(obj));
            }

            if (obj == null)
            {
                return default(T);
            }

            try
            {
                IFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, obj);
                    stream.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                // Debug.Log(e);
                var newObject = obj;
                return newObject;
            }
            
        }
    }
}