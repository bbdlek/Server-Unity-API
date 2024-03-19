using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using _1_Scripts._8_HeliosTest;
using MVS.Helios;
using UnityEngine;

namespace MVS.Realtime
{
    
    
    [AttributeUsage(AttributeTargets.Property)]
    public class LogOnChangeAttribute : Attribute
    {
        public string Message { get; }

        public LogOnChangeAttribute(string message)
        {
            Message = message;
        }
    }
    
    
    
    public class HeliosVariable
    {
        protected bool IsUpdate;

        protected Protocol.HeliosVariable _value;
        
        //Owner
        
        //Index
        
        public HeliosVariable()
        {
            _value = new Protocol.HeliosVariable();
            if(!HeliosNetwork.HeliosVariables.Contains(this))
                HeliosNetwork.HeliosVariables.Add(this);
        }
    }
    
    public class HNInt : HeliosVariable
    {
        public int Value
        {
            get
            {
                Debug.Log("Get");
                return _value.NInt32;
            }
            set
            {
                //Send
                Debug.Log("Set");
                _value.NInt32 = value;
            }
        }
        

        public HNInt(int value)
        {
            Value = value;
        }

        public static implicit operator HNInt(int value)
        {
            Debug.Log("A");
            return new HNInt(value);
        }

        public static implicit operator int(HNInt hnInt)
        {
            Debug.Log("B");
            return hnInt.Value;
        }

        public static HNInt operator +(HNInt a, HNInt b)
        {
            return new HNInt(a.Value + b.Value);
        }

        public static HNInt operator ++(HNInt a)
        {
            a.Value++;
            return a;
        }
        
        public static HNInt operator -(HNInt a, HNInt b)
        {
            return new HNInt(a.Value - b.Value);
        }

        public static HNInt operator --(HNInt a)
        {
            a.Value--;
            return a;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

}