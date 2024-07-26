using System.Collections.Generic;
using UnityEngine;

namespace MVS.Helios
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Helios/HeliosAnimator")]
    public class HeliosAnimator : HeliosMonoBehavior
    {
        private Animator _animator;

        [HNSync, OnChanged(nameof(SetParamChanged))] private Dictionary<int, bool> _syncParametersBools;
        [HNSync, OnChanged(nameof(SetParamChanged))] private Dictionary<int, float> _syncParametersFloats;
        [HNSync, OnChanged(nameof(SetParamChanged))] private Dictionary<int, int> _syncParametersInts;
        [HNSync, OnChanged(nameof(SetParamChanged))] private Dictionary<int, bool> _syncParametersTriggers;
        
        public override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            GetAnimatorParameters();
        }

        private void GetAnimatorParameters()
        {
            int index = 0;
            foreach (var animatorParameter in _animator.parameters)
            {
                switch (animatorParameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        _syncParametersBools[index] = animatorParameter.defaultBool;
                        break;
                    case AnimatorControllerParameterType.Float:
                        _syncParametersFloats[index] = animatorParameter.defaultFloat;
                        break;
                    case AnimatorControllerParameterType.Int:
                        _syncParametersInts[index] = animatorParameter.defaultInt;
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        _syncParametersTriggers[index] = animatorParameter.defaultBool;
                        break;
                }
                index++;
            }
        }

        private void SetParamChanged()
        {
            Debug.Log("Changed");
        }
    }
}