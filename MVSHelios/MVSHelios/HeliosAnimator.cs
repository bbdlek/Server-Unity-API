using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MVS.Helios
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Helios/HeliosAnimator")]
    public class HeliosAnimator : HeliosMonoBehavior
    {
        private Animator _animator;

        [HNSync] private Dictionary<string, bool> _syncParametersBools = new Dictionary<string, bool>();
        [HNSync] private Dictionary<string, float> _syncParametersFloats = new Dictionary<string, float>();
        [HNSync] private Dictionary<string, int> _syncParametersInts = new Dictionary<string, int>();
        [HNSync] private Dictionary<string, bool> _syncParametersTriggers = new Dictionary<string, bool>();

        [HNSync] private Dictionary<int, int> _syncStateHashes = new Dictionary<int, int>();
        [HNSync] private Dictionary<int, float> _syncNormalizedTimes = new Dictionary<int, float>();
        [HNSync] private Dictionary<int, bool> _syncLoopBools = new Dictionary<int, bool>();

        [HNSync] private Dictionary<int, float> _syncLayerWeights = new Dictionary<int, float>();

        [HNSync] private Vector3 _syncRootPosition;
        [HNSync] private Quaternion _syncRootRotation;
        [HNSync] private float _syncAnimatorSpeed;

        public override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            InitAnimatorParameters();
        }

        [HNSync] public float testFloat = 3f;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                testFloat = Random.Range(0f, 3f);
            }
        }

        private void LateUpdate()
        {
            if (IsMine)
            {
                UpdateAnimatorParameters();
            }
            else
            {
                ApplyAnimatorParameters();
            }
        }

        private void InitAnimatorParameters()
        {
            foreach (var animatorParameter in _animator.parameters)
            {
                switch (animatorParameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _syncParametersBools[animatorParameter.name] = animatorParameter.defaultBool;
                        break;
                    case AnimatorControllerParameterType.Float:
                        _syncParametersFloats[animatorParameter.name] = animatorParameter.defaultFloat;
                        break;
                    case AnimatorControllerParameterType.Int:
                        _syncParametersInts[animatorParameter.name] = animatorParameter.defaultInt;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        _syncParametersTriggers[animatorParameter.name] = false;
                        break;
                }
            }

            // 레이어 초기화
            for (int i = 0; i < _animator.layerCount; i++)
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);
                _syncStateHashes[i] = stateInfo.fullPathHash;
                _syncNormalizedTimes[i] = stateInfo.normalizedTime;
                _syncLoopBools[i] = stateInfo.loop;
                _syncLayerWeights[i] = _animator.GetLayerWeight(i);
            }

            // 루트 모션 초기화
            if(_animator.applyRootMotion)
            {
                _syncRootPosition = transform.position;
                _syncRootRotation = transform.rotation;
            }

            // 애니메이터 속도 초기화
            _syncAnimatorSpeed = _animator.speed;
        }

        private void UpdateAnimatorParameters()
        {
            // 파라미터 동기화
            foreach (var animatorParameter in _animator.parameters)
            {
                switch (animatorParameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _syncParametersBools[animatorParameter.name] = _animator.GetBool(animatorParameter.name);
                        break;
                    case AnimatorControllerParameterType.Float:
                        _syncParametersFloats[animatorParameter.name] = _animator.GetFloat(animatorParameter.name);
                        break;
                    case AnimatorControllerParameterType.Int:
                        _syncParametersInts[animatorParameter.name] = _animator.GetInteger(animatorParameter.name);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        _syncParametersTriggers[animatorParameter.name] = _animator.GetBool(animatorParameter.name);
                        break;
                }
            }

            // 애니메이터 상태 및 레이어 동기화
            for (int i = 0; i < _animator.layerCount; i++)
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);
                _syncStateHashes[i] = stateInfo.fullPathHash;
                _syncNormalizedTimes[i] = stateInfo.normalizedTime;
                _syncLoopBools[i] = stateInfo.loop;
                _syncLayerWeights[i] = _animator.GetLayerWeight(i);
            }

            // 루트 모션 동기화
            if (_animator.applyRootMotion)
            {
                _syncRootPosition = transform.position;
                _syncRootRotation = transform.rotation;
            }

            // 애니메이터 속도 동기화
            _syncAnimatorSpeed = _animator.speed;
        }

        private void ApplyAnimatorParameters()
        {
            // 파라미터 적용
            foreach (var kvp in _syncParametersBools)
            {
                _animator.SetBool(kvp.Key, kvp.Value);
            }
            foreach (var kvp in _syncParametersInts)
            {
                _animator.SetInteger(kvp.Key, kvp.Value);
            }
            foreach (var kvp in _syncParametersFloats)
            {
                _animator.SetFloat(kvp.Key, kvp.Value);
            }
            foreach (var kvp in _syncParametersTriggers)
            {
                if (kvp.Value)
                    _animator.SetTrigger(kvp.Key);
            }

            // 애니메이터 상태 및 레이어 적용
            foreach (var kvp in _syncStateHashes)
            {
                if (_syncNormalizedTimes.TryGetValue(kvp.Key, out float normalizedTime) &&
                    _syncLoopBools.TryGetValue(kvp.Key, out bool loop))
                {
                    _animator.Play(kvp.Value, kvp.Key, normalizedTime);
                }
            }

            foreach (var kvp in _syncLayerWeights)
            {
                _animator.SetLayerWeight(kvp.Key, kvp.Value);
            }

            // 루트 모션 적용
            if (_animator.applyRootMotion)
            {
                transform.position = _syncRootPosition;
                transform.rotation = _syncRootRotation;
            }

            // 애니메이터 속도 적용
            _animator.speed = _syncAnimatorSpeed;
        }
    }
}
