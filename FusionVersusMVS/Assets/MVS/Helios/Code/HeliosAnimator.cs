using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace MVS.Helios
{
    
    [AddComponentMenu("Helios/HeliosAnimator")]
    [RequireComponent(typeof(Animator))]
    public class HeliosAnimator : HeliosMonoBehavior
    {
        [SerializeField] private Animator _animator;

        private Dictionary<string, object> previousParameterValues = new Dictionary<string, object>();

        public Animator Animator
        {
            get => _animator;
            set => _animator = value;
        }
        private AnimatorController _animatorController;
        private AnimatorStateMachine _animatorStateMachine;

        public override void Start()
        {
            _animator = GetComponent<Animator>();

            foreach (var parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    previousParameterValues[parameter.name] = _animator.GetBool(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Float)
                    previousParameterValues[parameter.name] = _animator.GetFloat(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Int)
                    previousParameterValues[parameter.name] = _animator.GetInteger(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Trigger)
                    previousParameterValues[parameter.name] = false;
            }
        }

        private void Update()
        {
            foreach (var parameter in _animator.parameters)
            {
                object previousValue = previousParameterValues[parameter.name];
                object currentValue = null;
                
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    currentValue = _animator.GetBool(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Float)
                    currentValue = _animator.GetFloat(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Int)
                    currentValue = _animator.GetInteger(parameter.name);
                else if (parameter.type == AnimatorControllerParameterType.Trigger)
                    currentValue = _animator.GetBool(parameter.name);

                if (!previousValue.Equals(currentValue))
                {
                    Debug.Log(parameter.name + " changed to " + currentValue);
                }

                previousParameterValues[parameter.name] = currentValue;
            }
        }
    }
}
