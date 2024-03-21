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

        public void Initialize(HeliosMonoBehavior heliosMonoBehavior)
        {
            _owner = heliosMonoBehavior;
            if(!HeliosNetwork.HeliosVariables.Contains(this))
            {
                HeliosNetwork.HeliosVariables.Add(this);
                Index = HeliosNetwork.HeliosVariables.IndexOf(this);
            }
        }

        public HeliosMonoBehavior GetMonoBehavior()
        {
            return _owner;
        }

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

}