using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _rotationSpeed = 500;
    [SerializeField] float _damper = 0.3f;
    [SerializeField] float _fixedDuration;
    [SerializeField] bool _isInAnimation = false;

    [Header("refrences")]
    [SerializeField] AnimationSettings[] _animSetting;
    [SerializeField] RootAnimationSettings[] _rootAnimSetting;

    Animator _animator;
    DetectObjects _detectObjects;
    PlayerMovement _playerMovement;

    // Anim Hash
    readonly int IDidleToWalk = Animator.StringToHash("idleToWalkSpeed");
    readonly int IDjump = Animator.StringToHash("Jump");

    private void Start()
    {
        _detectObjects = GetComponent<DetectObjects>();
        _playerMovement = GetComponent<PlayerMovement>();
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = false;
    }

    private void Update()
    {
        CheckPossibleActions();
    }

    private void FixedUpdate()
    {
        PlayFixTimeAnimation(_damper, _isInAnimation);
    }

    void PlayFixTimeAnimation(float walkDampValue, bool isAlreadyinAction)
    {
        if (isAlreadyinAction)
            return;

        if (!_playerMovement.Prop_IsGrounded)
            _animator.CrossFadeInFixedTime(IDjump, 0);

        if (_playerMovement.Prop_IsGrounded)
            _animator.SetFloat(IDidleToWalk, _playerMovement.Prop_PlayerVelocity, walkDampValue, Time.fixedDeltaTime);
    }

    void CheckPossibleActions()
    {
        foreach (RootAnimationSettings rootAction in _rootAnimSetting)
        {
            if (rootAction.CanPerformRootAction(_detectObjects.DownRayData, _detectObjects.ForwardRayData)
                && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(PlayRootActions(rootAction));
                return;
            }
        }
    }

    void ResetAnimationValues()
    {
        // Player could continue its previous animations
        // if the last values were greater than 0 before snatching away the controls
        _animator.SetFloat(IDidleToWalk, 0.0f);
    }

    IEnumerator PlayRootActions(RootAnimationSettings rootActions)
    {
        _playerMovement.ShouldPlayerMove(false);
        _isInAnimation = true;
        ResetAnimationValues();
        _animator.applyRootMotion = true;

        _animator.CrossFadeInFixedTime(rootActions.AnimationName, _fixedDuration, 0);
        yield return null;

        AnimatorStateInfo animState = _animator.GetNextAnimatorStateInfo(0);

        if (!animState.IsName(rootActions.AnimationName))
            Debug.LogError("Wrong Anim Name");

        float waitTimer = 0;
        while (waitTimer <= animState.length)
        {
            waitTimer += Time.deltaTime;

            if (rootActions.EnableMatching)
                TargetMatching(rootActions);

            if (rootActions.ShouldPlayerRotate)
                RotatePlayerDuringAnimation(rootActions);

            yield return null;
        }

        // reset the values back
        _animator.applyRootMotion = false;
        _isInAnimation = false;
        _playerMovement.ShouldPlayerMove(true);
    }

    void RotatePlayerDuringAnimation(RootAnimationSettings whichAction)
    {
        _playerMovement.Prop_PlayerRigidBody.rotation = Quaternion.RotateTowards(_playerMovement.Prop_PlayerRigidBody.rotation,
                whichAction.RotationDirection, _rotationSpeed * Time.deltaTime);
    }

    void TargetMatching(RootAnimationSettings actionName)
    {
        if (_animator.isMatchingTarget)
            return;

        // ex: foot`s origin starts at the ankle level and targetmatching will match that position
        // to prevent that we added correction pos at the Y axes to move it up accordingly.
        Vector3 correctionPos = new Vector3(actionName.MatchingPosition.x,
            actionName.MatchingPosition.y + actionName.OffSet, actionName.MatchingPosition.z);

        _animator.MatchTarget(correctionPos, transform.rotation, actionName.BodyPart,
            new MatchTargetWeightMask(actionName.MatchAxes, 0), actionName.StartTime, actionName.EndTime);
    }

    //Properties
    public bool IsInAnimation => _isInAnimation;
}