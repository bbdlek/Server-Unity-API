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
        // [HideInInspector]
        public int Index;

        public void SetIndex(int idx)
        {
            Index = idx;
            _value.Key = idx;
        }
        
        public HeliosVariable()
        {
            _value = new Protocol.HeliosVariable();
        }

        public void SetOwner(HeliosMonoBehavior heliosMonoBehavior)
        {
            _owner = heliosMonoBehavior;
        }

        public Protocol.HeliosVariable GetValue()
        {
            return _value;
        }

        public virtual void SetFlag(bool flag)
        {
            IsUpdate = flag;
            if(_owner != null)
                _owner.hasUpdate = flag;
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
            SetFlag(false);
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
            SetFlag(false);
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
            SetFlag(false);
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
            SetFlag(false);
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
            SetFlag(false);
            base._value.NString = value;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNVector : HeliosVariable
    {
        [SerializeField]
        private Vector3 _value;
        
        public Vector3 Value 
        {
            get
            {
                Debug.Log("Get");
                var vec = new Vector3((float)base._value.NVector.X, (float)base._value.NVector.X, (float)base._value.NVector.X);
                return vec;
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    var vec = new Protocol.Vector3
                    {
                        X = value.x,
                        Y = value.y,
                        Z = value.z
                    };
                    base._value.NVector = vec;
                    _value = value;
                }
            }
        }
        
        public HNVector(Vector3 value)
        {
            SetFlag(false);
            var vec = new Protocol.Vector3
            {
                X = value.x,
                Y = value.y,
                Z = value.z
            };
            base._value.NVector = vec;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }
    
    [Serializable]
    public class HNQuaternion : HeliosVariable
    {
        [SerializeField]
        private Quaternion _value;
        
        public Quaternion Value 
        {
            get
            {
                Debug.Log("Get");
                var vec = new Vector3((float)base._value.NVector.X, (float)base._value.NVector.X, (float)base._value.NVector.X);
                return Quaternion.Euler(vec);
            }
            set
            {
                if(_value != value)
                {
                    Debug.Log($"Set {Index}");
                    SetFlag(true);
                    Vector3 euler = value.eulerAngles;
                    var vec = new Protocol.Vector3
                    {
                        X = euler.x,
                        Y = euler.y,
                        Z = euler.z
                    };
                    base._value.NVector = vec;
                    _value = value;
                }
            }
        }
        
        public HNQuaternion(Quaternion value)
        {
            SetFlag(false);
            Vector3 euler = value.eulerAngles;
            var vec = new Protocol.Vector3
            {
                X = euler.x,
                Y = euler.y,
                Z = euler.z
            };
            base._value.NVector = vec;
            _value = value;
        }

        public override void SetFlag(bool flag)
        {
            base.SetFlag(flag);
        }
    }

}