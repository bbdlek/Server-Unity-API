using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MVS.Helios;
using MVS.Realtime;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace _1_Scripts._8_HeliosTest
{
    
    public class HeliosVariable2
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
        
        public HeliosVariable2()
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
    public struct HNInt2
    {
        private HeliosVariable2 _heliosVariable2;
        
        [SerializeField]
        private int _value;
        
        public int Value 
        {
            get
            {
                Debug.Log("Get");
                // return base._value.NInt32;
                return _value;
            }
            set
            {
                if(_value != value)
                {
                    // Debug.Log($"Set {Index}");
                    SetFlag(true);
                    // base._value.NInt32 = value;
                    _value = value;
                }
            }
        }
        
        public HNInt2(int value)
        {
            _heliosVariable2 = new HeliosVariable2();
            _heliosVariable2.IsUpdate = false;
            // IsUpdate = false;
            // base._value.NInt32 = value;
            _value = value;
        }

        public void SetFlag(bool flag)
        {
            _heliosVariable2.SetFlag(flag);
        }
    }
    
    public class VarTest : MonoBehaviour
    {
        [SerializeField] private HNInt2 hp = new HNInt2(1);
        
        [SerializeField]
        private HNInt score = new HNInt(1);
        [SerializeField]
        private HNInt score2 = new HNInt(2);

        private void Start()
        {
            hp.Value = 1;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                score.Value++;
            }
        }
    }
}