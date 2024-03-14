using System;
using _1_Scripts._8_HeliosTest;
using Google.Protobuf.WellKnownTypes;
using MVS.Helios;
using Protocol;
using UnityEngine;

namespace MVS.Realtime
{
    [Serializable]
    public class HeliosVariable
    {
        public HeliosVariable()
        {
            if(!HeliosNetwork.HeliosVariables.Contains(this))
                HeliosNetwork.HeliosVariables.Add(this);
        }
    }
    
    [Serializable]
    public class HNInt : HeliosVariable
    {
        private int _value;
        
        public int Value
        {
            get { return _value; }
            set
            {
                if(_value != value && HeliosNetwork.IsConnected)
                {
                    Debug.Log($"{_value} to {value}");
                    // TODO : SendQueue, Flag?
                    this._value = value;
                    Protocol.HeliosVariable netVariable = new Protocol.HeliosVariable();
                    netVariable.NInt32 = value;
                    Debug.Log(netVariable.NInt32);
                    HeliosNetwork.RaiseEvent(CustomEventCode.Variable, netVariable);
                }
            }
        }

        public HNInt(int value)
        {
            Value = value;
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

        public static implicit operator int(HNInt HNInt)
        {
            return HNInt.Value;
        }
        
        public static implicit operator HNInt(int value)
        {
            return new HNInt(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

}