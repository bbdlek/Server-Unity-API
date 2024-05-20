using System.Linq;
using MVS.Realtime;
using Protocol;
using UnityEngine;
using EventCode = MVS.Realtime.EventCode;
using HeliosVariable = Protocol.HeliosVariable;
using Vector3 = UnityEngine.Vector3;

namespace MVS.Helios
{
    [AddComponentMenu("Helios/HeliosTransform")]
    public class HeliosTransform : HeliosMonoBehavior
    {
        //Tolerance
        public float positionTolerance = 0.01f; // 위치 변경 허용 범위
        public float rotationTolerance = 1f; // 회전 변경 허용 범위 (예: 1도)
        public float scaleTolerance = 0.01f; // 크기 변경 허용 범위

        private bool HasPositionChanged(Vector3 newPosition) {
            return Vector3.Distance(newPosition, _storedPosition) > positionTolerance;
        }

        private bool HasRotationChanged(Quaternion newRotation) {
            return Quaternion.Angle(newRotation, _storedRotation) > rotationTolerance;
        }

        private bool HasScaleChanged(Vector3 newScale) {
            return Vector3.Distance(newScale, _storedScale) > scaleTolerance;
        }

        [HideInInspector]
        public Vector3 networkPosition;
        private Vector3 _storedPosition;
        
        [HideInInspector]
        public Quaternion networkRotation;
        private Quaternion _storedRotation;
        
        [HideInInspector]
        public Vector3 networkScale;
        private Vector3 _storedScale;

        public bool syncPosition = true;
        public bool syncRotation = true;
        public bool syncScale = true;

        public float smoothness;

        private bool _isMine;

        private ObjectInfo _objectInfo;
        private HeliosObject _heliosObject;
        
        // TODO : Local Lossy

        private void Awake()
        {
            _heliosObject = GetComponent<HeliosObject>();
            _isMine = _heliosObject.IsMine;
            _storedPosition = transform.position;
            networkPosition = _storedPosition;

            _storedRotation = transform.rotation;
            networkRotation = _storedRotation;

            _storedScale = transform.localScale;
            networkScale = _storedScale;
        }

        private void Update()
        {
            var tr = transform;
            if (_isMine && (HasPositionChanged(tr.localPosition) || HasRotationChanged(tr.rotation) || HasScaleChanged(tr.localScale))) {
                //Send?
                var fixedData = new C_UPDATE_NETWORK_OBJECTS();
                _objectInfo =_heliosObject.ObjectInfo;
                //////TestValue 같이
                // var _objectInfo = new ObjectInfo
                // {
                //     ObjectID = new ObjectID
                //     {
                //         PrefabID = GetComponent<HeliosObject>().PrefabId,
                //         InstanceID = GetComponent<HeliosObject>().InstanceId,
                //         ClientInstanceID = GetComponent<HeliosObject>().ClientInstanceId,
                //     },
                //     SyncType = ObjectSyncType.PersonalOwn,
                //     OwnerPlayerID = HeliosNetwork.LocalPlayer.UserId
                // };

                _objectInfo.TestValues.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("position"))!.NVector =
                    new Protocol.Vector3
                    {
                        X = tr.position.x,
                        Y = tr.position.y,
                        Z = tr.position.z
                    };
                
                _objectInfo.TestValues.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("rotation"))!.NVector =
                    new Protocol.Vector3
                    {
                        X = tr.eulerAngles.x,
                        Y = tr.eulerAngles.y,
                        Z = tr.eulerAngles.z
                    };
                
                _objectInfo.TestValues.LastOrDefault(x => x.Key == CustomVariables.GetKeyByName("scale"))!.NVector =
                    new Protocol.Vector3
                    {
                        X = tr.localScale.x,
                        Y = tr.localScale.y,
                        Z = tr.localScale.z
                    };

                // _objectInfo.TestValues.Add(new HeliosVariable
                // {
                //     Key = CustomVariables.GetKeyByName("position"),
                //     NVector = new Protocol.Vector3
                //     {
                //         X = tr.localPosition.x,
                //         Y = tr.localPosition.y,
                //         Z = tr.localPosition.z
                //     }
                // });
                //
                // _objectInfo.TestValues.Add(new HeliosVariable
                // {
                //     Key = CustomVariables.GetKeyByName("rotation"),
                //     NVector = new Protocol.Vector3
                //     {
                //         X = tr.eulerAngles.x,
                //         Y = tr.eulerAngles.y,
                //         Z = tr.eulerAngles.z
                //     }
                // });
                //
                // _objectInfo.TestValues.Add(new HeliosVariable
                // {
                //     Key = CustomVariables.GetKeyByName("scale"),
                //     NVector = new Protocol.Vector3
                //     {
                //         X = tr.localScale.x,
                //         Y = tr.localScale.y,
                //         Z = tr.localScale.z
                //     }
                // });
                
                fixedData.ObjectInfos.Add(_objectInfo);
                
                HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, fixedData);
                
                _storedPosition = tr.position;
                networkPosition = _storedPosition;
                
                _storedRotation = tr.rotation;
                networkRotation = _storedRotation;
                
                _storedScale = tr.localScale;
                networkScale = _storedScale;
            }
            
            
            //Read?
            if(!_isMine)
            {
                if(syncPosition)
                {
                    // 부드러운 이동을 위해 Lerp 사용
                    tr.position = Vector3.Lerp(tr.position, networkPosition, Time.deltaTime * smoothness);
                }
                if(syncRotation)
                {
                    // 부드러운 회전을 위해 Slerp 사용
                    tr.rotation = Quaternion.Slerp(tr.rotation, networkRotation, Time.deltaTime * smoothness);
                }
                if (syncScale)
                {
                    // 부드러운 크기 변경을 위해 Lerp 사용
                    tr.localScale = Vector3.Lerp(tr.localScale, networkScale, Time.deltaTime * smoothness);
                }
            }
        }
    }
}