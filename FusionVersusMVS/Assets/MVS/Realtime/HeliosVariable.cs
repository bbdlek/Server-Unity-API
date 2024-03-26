using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using _1_Scripts._8_HeliosTest;
using MVS.Helios;
using UnityEngine;

namespace MVS.Realtime
{
    public class HeliosVariable
    {
        public bool IsUpdate = false;

        protected Protocol.HeliosVariable _value;
        
        //Owner
        private HeliosMonoBehavior _owner;
        
        //Index
        [HideInInspector]
        public int Index;

        public void SetIndex(int idx)
        {
            Index = idx;
        }
        
        public HeliosVariable()
        {
            _value = new Protocol.HeliosVariable();
        }

        public Protocol.HeliosVariable GetValue()
        {
            return _value;
        }

        public virtual void SetFlag(bool flag)
        {
            IsUpdate = flag;
        }
    }
    
    [Serializable]
    public class HNInt : HeliosVariable
    {
        [SerializeField]
        private int _value;
        
        public int Value 
        {
            get
            {
                Debug.Log("Get");
                return base._value.NInt32;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    base._value.NInt32 = value;
                    _value = value;
                }
            }
        }
        
        public HNInt(int value)
        {
            IsUpdate = false;
            base._value.NInt32 = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNLong : HeliosVariable
    {
        [SerializeField]
        private long _value;
        
        public long Value 
        {
            get
            {
                Debug.Log("Get");
                return base._value.NInt64;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    base._value.NInt64 = value;
                    _value = value;
                }
            }
        }
        
        public HNLong(long value)
        {
            IsUpdate = false;
            base._value.NInt64 = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNFloat : HeliosVariable
    {
        [SerializeField]
        private float _value;
        
        public float Value 
        {
            get
            {
                Debug.Log("Get");
                return base._value.NFloat;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    base._value.NFloat = value;
                    _value = value;
                }
            }
        }
        
        public HNFloat(float value)
        {
            IsUpdate = false;
            base._value.NFloat = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNDouble : HeliosVariable
    {
        [SerializeField]
        private double _value;
        
        public double Value 
        {
            get
            {
                Debug.Log("Get");
                return base._value.NInt32;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    base._value.NDouble = value;
                    _value = value;
                }
            }
        }
        
        public HNDouble(double value)
        {
            IsUpdate = false;
            base._value.NDouble = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNString : HeliosVariable
    {
        [SerializeField]
        private string _value;
        
        public string Value 
        {
            get
            {
                Debug.Log("Get");
                return base._value.NString;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    base._value.NString = value;
                    _value = value;
                }
            }
        }
        
        public HNString(string value)
        {
            IsUpdate = false;
            base._value.NString = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }

}