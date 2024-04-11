using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using Transform = Protocol.Transform;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosTransform")]
    public class HeliosTransform : HeliosMonoBehavior
    {
        public Vector3 networkPosition;
        private Vector3 _storedPosition;
        
        public Quaternion networkRotation;
        private Quaternion _storedRotation;
        
        public Vector3 networkScale;
        private Vector3 _storedScale;

        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool syncScale = true;

        private bool _isMine;
        
        // TODO : Local Lossy

        private void Awake()
        {
            _isMine = GetComponent<HeliosObject>().IsMine;
            _storedPosition = transform.localPosition;
            networkPosition = Vector3.zero;

            _storedRotation = transform.localRotation;
            networkRotation = Quaternion.identity;

            _storedScale = transform.localScale;
            networkScale = Vector3.zero;
        }

        private void Update()
        {
            var tr = transform;
            if ((tr.localPosition != _storedPosition || tr.localRotation != _storedRotation || tr.localScale != _storedScale) && _isMine)
            {
                //Send?
                var fixedData = new C_UPDATE_NETWORK_OBJECTS();
                var ObjectInfo = new ObjectInfo
                {
                    ObjectID = new ObjectID
                    {
                        PrefabID = GetComponent<HeliosObject>().PrefabId,
                        InstanceID = GetComponent<HeliosObject>().InstanceId,
                    },
                    SyncType = ObjectSyncType.PersonalOwn,
                    OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
                };
                
                ObjectInfo.TestValues.Add(new HeliosVariable
                {
                    Key = CustomVariables.GetKeyByName("position"),
                    NVector = new Protocol.Vector3
                    {
                        X = tr.localPosition.x,
                        Y = tr.localPosition.y,
                        Z = tr.localPosition.z
                    }
                });
                
                ObjectInfo.TestValues.Add(new HeliosVariable
                {
                    Key = CustomVariables.GetKeyByName("rotation"),
                    NVector = new Protocol.Vector3
                    {
                        X = tr.eulerAngles.x,
                        Y = tr.eulerAngles.y,
                        Z = tr.eulerAngles.z
                    }
                });
                
                ObjectInfo.TestValues.Add(new HeliosVariable
                {
                    Key = CustomVariables.GetKeyByName("scale"),
                    NVector = new Protocol.Vector3
                    {
                        X = tr.localScale.x,
                        Y = tr.localScale.y,
                        Z = tr.localScale.z
                    }
                });
                
                fixedData.ObjectInfos.Add(ObjectInfo);
                
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, fixedData);
                
                _storedPosition = tr.localPosition;
                networkPosition = _storedPosition;
                
                _storedPosition = tr.localEulerAngles;
                networkRotation = _storedRotation;
                
                _storedScale = tr.localScale;
                networkScale = _storedScale;
            }
            
            
            //Read?
            if(!_isMine)
            {
                if(syncPosition)
                    tr.localPosition = networkPosition;
                if(syncRotation)
                    tr.localRotation = networkRotation;
                if (syncScale)
                    tr.localScale = networkScale;
            }
        }
    }
}