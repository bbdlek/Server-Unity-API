using System;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using UnityEngine.Serialization;
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
                var pkt = new C_UPDATE_NETWORK_OBJECTS();
                var objectInfo = new ObjectInfo
                {
                    ObjectID = new ObjectID
                    {
                        PrefabID = GetComponent<HeliosObject>().prefabId,
                        InstanceID = GetComponent<HeliosObject>().instanceId,
                    },
                    SyncType = ObjectSyncType.PersonalOwn,
                    OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
                };
                objectInfo.NumberProps.Add(new CustomNumberProp
                {
                    Index = PropsID.Position3D,
                    Value = { tr.localPosition.x, tr.localPosition.y, tr.localPosition.z }
                });
                objectInfo.NumberProps.Add(new CustomNumberProp
                {
                    Index = PropsID.Rotation3D,
                    Value = { tr.localRotation.x, tr.localRotation.y, tr.localRotation.z, tr.localRotation.w }
                });
                pkt.ObjectInfos.Add(objectInfo);
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, pkt);
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