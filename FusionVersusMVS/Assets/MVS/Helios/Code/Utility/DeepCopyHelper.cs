using System;
using System.Collections;
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
                Debug.Log($"The type must be serializable. {nameof(obj)}");
            }

            if (obj == null)
            {
                return default(T);
            }

            if (HeliosUtility.IsListType(obj.GetType()))
            {
                IList list = (IList)obj;
                IList copiedList = (IList)Activator.CreateInstance(obj.GetType());
                
                foreach (var item in list)
                {
                    if (item is ICloneable)
                    {
                        copiedList.Add(((ICloneable)item).Clone());
                    }
                    else if (item != null && item.GetType().IsValueType)
                    {
                        copiedList.Add(item);
                    }
                    else
                    {
                        copiedList.Add(item);
                    }
                }
                return (T)copiedList;
            }
            else
            {
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
                    // Debug.LogError(e);
                    var newObject = obj;
                    return newObject;
                }   
            }
        }
    }
}