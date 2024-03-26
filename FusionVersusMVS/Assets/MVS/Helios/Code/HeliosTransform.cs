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

        [HideInInspector]
        public Quaternion networkRotation;
        private Quaternion _storedRotation;

        public bool syncPosition = true;
        public bool syncRotation = true;
        
        // TODO : Local Lossy

        private void Awake()
        {
            _storedPosition = transform.localPosition;
            networkPosition = Vector3.zero;

            _storedRotation = transform.localRotation;
            networkRotation = Quaternion.identity;
        }

        private void Update()
        {
            var tr = transform;
            if ((tr.localPosition != _storedPosition || tr.localRotation != _storedRotation) && isMine)
            {
                //Send?
                var fixedData = new C_UPDATE_NETWORK_OBJECTS();
                var ObjectInfo = new ObjectInfo
                {
                    ObjectID = new ObjectID
                    {
                        PrefabID = GetComponent<HeliosObject>().prefabId,
                        InstanceID = GetComponent<HeliosObject>().instanceId,
                    },
                    SyncType = ObjectSyncType.PersonalOwn,
                    OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
                };
                
                // Version 1
                var paramDic = new CustomDic();
                paramDic.Params.Add("Position", new HeliosVariable
                {
                    NVector = new Protocol.Vector3
                    {
                        X = tr.localPosition.x,
                        Y = tr.localPosition.y,
                        Z = tr.localPosition.z
                    }
                });
                paramDic.Params.Add("Rotation", new HeliosVariable
                {
                    NVector = new Protocol.Vector3
                    {
                        X = tr.eulerAngles.x,
                        Y = tr.eulerAngles.y,
                        Z = tr.eulerAngles.z
                    }
                });
                ObjectInfo.CustomValues = paramDic;
                
                // Version 2
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
                
                fixedData.ObjectInfos.Add(ObjectInfo);
                
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, fixedData);
                _storedPosition = tr.localPosition;
                networkPosition = _storedPosition;
            }
            
            
            //Read?
            if(!isMine)
            {
                tr.localPosition = networkPosition;
                tr.localRotation = networkRotation;
            }

            // IF IsMine
            // {
            //     tr.localPosition = Vector3.MoveTowards(tr.localPosition, _networkPosition, Time.deltaTime);
            //     tr.localRotation = Quaternion.RotateTowards(tr.localRotation, _networkRotation, Time.deltaTime);
            // }
        }
    }
}