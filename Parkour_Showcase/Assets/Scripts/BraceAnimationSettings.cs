using UnityEngine;

[CreateAssetMenu(menuName = "My Parkours/Brace Anim Settings")]
public class BraceAnimationSettings : ScriptableObject
{
    [SerializeField] string _animatioName;
    [SerializeField] bool _shouldPlayerRotate;

    [Header("Target Matching")]
    [SerializeField] bool _shouldTargetMatch;
    [SerializeField] float _startTime;
    [SerializeField] float _endTime;
    // offSets will be in global axes and mathcAxes will receive those in the local space
    [SerializeField] float _offsetY = -0.55f;
    [SerializeField] float _offsetX = 0.18f;
    [SerializeField] AvatarTarget _bodyPartToTarget;
    [SerializeField] Vector3 _matchAxes;

    readonly int idHangIdle = Animator.StringToHash("HangIdle");
    readonly int idHangMove = Animator.StringToHash("HangMovement");
    readonly int idIsHangMoveMirror = Animator.StringToHash("isHangMoveMirror");

    Quaternion _rotationDirection;
    Vector3 _targetMatchPosition = Vector3.zero;

    public bool CanPerformBraceAction(RaycastHit forwardHitData,RaycastHit downHitData)
    {
        if (forwardHitData.collider == null)
            return false;

        if(_shouldTargetMatch)
            _targetMatchPosition = downHitData.point;

        if (_shouldPlayerRotate)
            _rotationDirection = Quaternion.LookRotation(-forwardHitData.normal);

        return true;
    }

    // Properties
    public bool ShouldPlayerRotate => _shouldPlayerRotate;
    public int IDHangIdle => idHangIdle;
    public int IDHangMove => idHangMove;
    public int IDIsHangMoveMirror => idIsHangMoveMirror;
    public Quaternion RotationDirection => _rotationDirection;

    // Target Matching
    public Vector3 MatchingPosition => _targetMatchPosition;
    public bool EnableMatching => _shouldTargetMatch;
    public float StartTime => _startTime;
    public float EndTime => _endTime;
    public float OffSetY => _offsetY;
    public float OffSetX => _offsetX;
    public AvatarTarget BodyPart => _bodyPartToTarget;
    public Vector3 MatchAxes => _matchAxes;
}
