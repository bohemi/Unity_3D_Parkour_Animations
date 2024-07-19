using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GridBrushBase;

[CreateAssetMenu(menuName = "My Parkours/Root Action Settings")]
public class RootAnimationSettings : ScriptableObject
{
    [Header("Set Target Matching")]
    [SerializeField] bool _enableTargetMatching = false;
    [SerializeField] bool _shouldPlayerRotate;
    [SerializeField] float _startTime;
    [SerializeField] float _endTime;
    [SerializeField] AvatarTarget _bodyPartToTarget;
    [SerializeField] Vector3 _matchAxes;

    [Header("Avatar Placement Correction")]
    [SerializeField] float _offset;

    Quaternion _rotationDirection;
    Vector3 _matchingPosition = Vector3.zero;

    [Header("Set Object Height")]
    [SerializeField] string _animatioName;
    [SerializeField] float _minHeight;
    [SerializeField] float _maxHeight;
    [SerializeField] bool _isItVaultAnim = false;

    public bool CanPerformRootAction(RaycastHit downRayData, RaycastHit forwardRayData)
    {
        if (forwardRayData.collider == null)
            return false;

        if (forwardRayData.transform.tag == "Vault" && !_isItVaultAnim)
            return false;

        if (forwardRayData.transform.localScale.y > _maxHeight || forwardRayData.transform.localScale.y < _minHeight)
            return false;

        if (_shouldPlayerRotate)
            _rotationDirection = Quaternion.LookRotation(-forwardRayData.normal);

        if (EnableMatching)
            _matchingPosition = downRayData.point;

        return true;
    }

    // Properties
    public string AnimationName => _animatioName;
    public bool ShouldPlayerRotate => _shouldPlayerRotate;
    public Quaternion RotationDirection => _rotationDirection;

    // Target Matching Proprities
    public bool EnableMatching => _enableTargetMatching;
    public float StartTime => _startTime;
    public float EndTime => _endTime;
    public float OffSet => _offset;
    public AvatarTarget BodyPart => _bodyPartToTarget;
    public Vector3 MatchingPosition => _matchingPosition;
    public Vector3 MatchAxes => _matchAxes;
}
