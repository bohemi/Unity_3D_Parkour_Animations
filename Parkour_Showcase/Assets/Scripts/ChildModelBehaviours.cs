using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildModelBehaviours : MonoBehaviour
{
    Animator _animator;
    Rigidbody _parentRigidBody;
    AnimationController _animationController;
    [SerializeField] CapsuleCollider _hipCollider;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _parentRigidBody = GetComponentInParent<Rigidbody>();
        _animationController = GetComponentInParent<AnimationController>();
    }

    private void Update()
    {
        if (_animationController.ShouldEnableChildCollider)
        {
            _hipCollider.enabled = true;
        }
        else
            _hipCollider.enabled = false;
    }

    private void OnAnimatorMove()
    {
        // it means only update when performing root animations
        if (_animationController.IsInAnimation)
        {
            _parentRigidBody.position += _animator.deltaPosition;
            _parentRigidBody.rotation = _animator.deltaRotation * _parentRigidBody.rotation;
        }
    }
}
