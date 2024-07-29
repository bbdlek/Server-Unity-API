using System;
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

        [Range(1, 100)]
        public float smoothness = 50f;
        
        private HeliosObject _heliosObject;
        
        // TODO : Local Lossy

        private void Awake()
        {
            _heliosObject = GetComponent<HeliosObject>();
            
            _storedPosition = transform.position;
            networkPosition = _storedPosition;

            _storedRotation = transform.rotation;
            networkRotation = _storedRotation;

            _storedScale = transform.lossyScale;
            networkScale = _storedScale;
        }

        private void Update()
        {
            //Read?
            if(!IsMine)
            {
                if(syncPosition)
                {
                    // 부드러운 이동을 위해 Lerp 사용
                    transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * smoothness);
                }
                if(syncRotation)
                {
                    // 부드러운 회전을 위해 Slerp 사용
                    transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * smoothness);
                }
                if (syncScale)
                {
                    // 부드러운 크기 변경을 위해 Lerp 사용
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(networkScale.x/transform.lossyScale.x, networkScale.y/transform.lossyScale.y, networkScale.z/transform.lossyScale.z), Time.deltaTime * smoothness);
                }
            }
        }
        
        private float _elapsedTime = 0f;

        private void LateUpdate()
        {
            if(!HeliosNetwork.InGroup) return;
            _elapsedTime += Time.deltaTime;
            
            if (_elapsedTime >= 1f / HeliosNetwork.SendRate)
            {
                if (IsMine && (HasPositionChanged(transform.position) || HasRotationChanged(transform.rotation) || HasScaleChanged(transform.localScale))) {
                    var fixedData = new C_UPDATE_NETWORK_OBJECTS();
                    var _objectInfo = new ObjectInfo
                    {
                        ObjectID = _heliosObject.ObjectInfo.ObjectID,
                        SyncType = _heliosObject.ObjectInfo.SyncType,
                        OwnerPlayerID = _heliosObject.ObjectInfo.OwnerPlayerID
                    };

                    _objectInfo.Values.Add(new HeliosVariable
                    {
                        Key = CustomVariablesUnity.PosKey,
                        NVector = new Protocol.Vector3
                        {
                            X = transform.position.x,
                            Y = transform.position.y,
                            Z = transform.position.z
                        }
                    });
                    
                    _objectInfo.Values.Add(new HeliosVariable
                    {
                        Key = CustomVariablesUnity.RotKey,
                        NVector = new Protocol.Vector3
                        {
                            X = transform.eulerAngles.x,
                            Y = transform.eulerAngles.y,
                            Z = transform.eulerAngles.z
                        }
                    });
                    
                    _objectInfo.Values.Add(new HeliosVariable
                    {
                        Key = CustomVariablesUnity.ScaleKey,
                        NVector = new Protocol.Vector3
                        {
                            X = transform.lossyScale.x,
                            Y = transform.lossyScale.y,
                            Z = transform.lossyScale.z
                        }
                    });
                    
                    fixedData.ObjectInfos.Add(_objectInfo);
                    
                    HeliosNetwork.RaiseEvent(EventCode.PKT_C_UPDATE_NETWORK_OBJECTS, fixedData);
                    
                    _storedPosition = transform.position;
                    networkPosition = _storedPosition;
                    
                    _storedRotation = transform.rotation;
                    networkRotation = _storedRotation;
                    
                    _storedScale = transform.lossyScale;
                    networkScale = _storedScale;
                }
                _elapsedTime = 0f;
            }
        }
    }
}