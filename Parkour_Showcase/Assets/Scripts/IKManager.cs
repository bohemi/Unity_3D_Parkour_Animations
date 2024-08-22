using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    Animator _anim;
    AnimationController _animController;

    public LayerMask _layerMask; // ray collider mask

    [Range(0, 1f)]
    public float _foorValue; // Distance from where the foot transform

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _animController = GetComponentInParent<AnimationController>();
    }

    private void OnAnimatorIK()
    {
        FootPlacementOnGround(!_animController.IsInAnimation);
    }

    void FootPlacementOnGround(bool hasControl)
    {
        if (!hasControl)
            return;

        // Set the weights of the feets defined by the curve in our animations.
        _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat("IKLeftFootWeight"));
        _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _anim.GetFloat("IKLeftFootWeight"));
        _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, _anim.GetFloat("IKRightFootWeight"));
        _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, _anim.GetFloat("IKRightFootWeight"));

        // Left Foot
        RaycastHit hit;
        Ray ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

        // adding 1 so when the foot goes up ray will have enough length to keep on detecting further down
        if (Physics.Raycast(ray, out hit, _foorValue + 1f, _layerMask))
        {
            if (hit.transform.tag == "IKDetectable")
            {
                Vector3 footPosition = hit.point; // The target foot position is where the raycast hit a walkable object
                // as we increase ray value we will set the foot for desired placement
                footPosition.y += _foorValue; // because we are setting from ankle so add value to move the foot above
                _anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                _anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        }

        // Right Foot
        ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

        if (Physics.Raycast(ray, out hit, _foorValue + 1f, _layerMask))
        {
            if (hit.transform.tag == "IKDetectable")
            {
                Vector3 footPosition = hit.point;
                footPosition.y += _foorValue;
                _anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                _anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        }
    }
}
