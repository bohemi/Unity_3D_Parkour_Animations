using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float _braceToJumpUpForce = 8.0f;
    [SerializeField] float _braceToJumpDirectionForce;
    [SerializeField] float _braceToJumpBackForce;
    // will snatch player controls and allows jump animation to play while is in the air
    [SerializeField] float _braceToJumpTimer;
    [SerializeField] float _rotationSpeed = 500;
    [SerializeField] float _damper = 0.3f;
    [SerializeField] float _fixedDuration = 0.15f;
    [SerializeField] bool _isInAnimation = false;
    // collider for rolling action to prevent from capsule collider to interfere
    [SerializeField] bool _shouldEnableChildCollider = false;

    [Header("refrences")]
    [SerializeField] BraceAnimationSettings _braceIdleHangSetting;
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
        _animator = GetComponentInChildren<Animator>();
        _animator.applyRootMotion = true;
    }

    private void Update()
    {
        CheckPossibleGroundActions();
        CheckBraceActions();
    }

    private void FixedUpdate()
    {
        PlayJumpWalkAnimations(_damper, _isInAnimation);
    }

    void PlayJumpWalkAnimations(float walkDampValue, bool isAlreadyinAction)
    {
        if (isAlreadyinAction)
            return;

        if (!_playerMovement.Prop_IsGrounded)
            _animator.CrossFadeInFixedTime(IDjump, 0);

        if (_playerMovement.Prop_IsGrounded)
            _animator.SetFloat(IDidleToWalk, _playerMovement.Prop_PlayerVelocity, walkDampValue, Time.fixedDeltaTime);
    }

    void CheckBraceActions()
    {
        if (_playerMovement.Prop_IsGrounded || _isInAnimation)
            return;

        if (_braceIdleHangSetting.CanPerformBraceAction(_detectObjects.BraceRayData, _detectObjects.BraceRayDownData))
            StartCoroutine(PlayBraceAnimations(_braceIdleHangSetting));
    }

    IEnumerator PlayBraceAnimations(BraceAnimationSettings HangAction)
    {
        _playerMovement.ShouldPlayerMove(false, false);
        _isInAnimation = true;

        RotatePlayerDuringAnimation(null, HangAction);
        ResetAnimationValues();

        // we dont woant to keep matching the idleHang in the loop. since hangMove will match after moving
        if (HangAction.EnableMatching)
            TargetMatching(null, HangAction);

        // we turned off the gravity above but rigidBody could be in motion making it move in that direction forever
        // so kinematic will stop it where we found the brace
        _playerMovement.Prop_PlayerRigidBody.isKinematic = true;
        yield return null;
        _playerMovement.Prop_PlayerRigidBody.isKinematic = false;

        while (_isInAnimation)
        {
            _animator.CrossFadeInFixedTime(HangAction.IDHangIdle, _fixedDuration, 0);
            yield return null;

            AnimatorStateInfo idleHangState = _animator.GetNextAnimatorStateInfo(0);
            float tempIdleHangTimer = 0.0f;
            while (tempIdleHangTimer <= idleHangState.length)
            {
                tempIdleHangTimer += Time.fixedDeltaTime;
                // test jumpUp
                if (CanBraceToDirectionJump())
                {
                    while (_detectObjects.BraceRayData.collider)
                        yield return null;
                    
                    while (!_detectObjects.BraceRayData.collider && !_playerMovement.Prop_IsGrounded)
                    {
                        _animator.CrossFadeInFixedTime(IDjump, 0);
                        yield return null;
                    }
                    
                    if (_playerMovement.Prop_IsGrounded)
                    {
                        _isInAnimation = false;
                        _playerMovement.ShouldPlayerMove(true, false);
                        yield break;
                    }

                    _playerMovement.ShouldPlayerMove(false, false);

                    _playerMovement.Prop_PlayerRigidBody.isKinematic = true;
                    yield return null;
                    _playerMovement.Prop_PlayerRigidBody.isKinematic = false;

                    break;
                }
                
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
                    break;

                if (ShouldBraceToDrop())
                    yield break;

                yield return null;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.A))
                    _animator.SetBool(HangAction.IDIsHangMoveMirror, true);
                else
                    _animator.SetBool(HangAction.IDIsHangMoveMirror, false);

                _animator.CrossFadeInFixedTime(HangAction.IDHangMove, _fixedDuration, 0);
                yield return null;

                AnimatorStateInfo state = _animator.GetNextAnimatorStateInfo(0);
                float tempHangMoveTime = 0.0f;
                while (tempHangMoveTime <= state.length)
                {
                    tempHangMoveTime += Time.deltaTime;
                    // test jump
                    if (CanBraceToDirectionJump())
                    {
                        while (_detectObjects.BraceRayData.collider)
                            yield return null;

                        while (!_detectObjects.BraceRayData.collider && !_playerMovement.Prop_IsGrounded)
                        {
                            _animator.CrossFadeInFixedTime(IDjump, 0);
                            yield return null;
                        }

                        if (_playerMovement.Prop_IsGrounded)
                        {
                            _isInAnimation = false;
                            _playerMovement.ShouldPlayerMove(true, false);
                            yield break;
                        }

                        _playerMovement.ShouldPlayerMove(false, false);

                        _playerMovement.Prop_PlayerRigidBody.isKinematic = true;
                        yield return null;
                        _playerMovement.Prop_PlayerRigidBody.isKinematic = false;

                        break;
                    }

                    if (ShouldBraceToDrop())
                        yield break;

                    yield return null;
                }
            }
        }
        _isInAnimation = false;
        _playerMovement.ShouldPlayerMove(true, false);
    }

    bool ShouldBraceToDrop()
    {
        if (Input.GetKeyDown(KeyCode.C) || !_detectObjects.BraceRayData.collider)
        {
            _isInAnimation = false;
            _playerMovement.ShouldPlayerMove(true, false);
            _detectObjects.TurnOffBraceRay();

            return true;
        }
        return false;
    }

    bool CanBraceToDirectionJump()
    {
        if (!Input.GetKey(KeyCode.W) && !Input.GetKeyDown(KeyCode.Space) ||
            !Input.GetKey(KeyCode.A) && !Input.GetKeyDown(KeyCode.Space) ||
            !Input.GetKey(KeyCode.S) && !Input.GetKeyDown(KeyCode.Space) ||
            !Input.GetKey(KeyCode.D) && !Input.GetKeyDown(KeyCode.Space))
            return false;

        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            direction = _braceToJumpUpForce * Vector3.up;

        else if (Input.GetKey(KeyCode.A))
            direction = new Vector3(-_braceToJumpDirectionForce, _braceToJumpUpForce / 2.0f, 0);

        else if (Input.GetKey(KeyCode.S))
        {
            Quaternion rot = Quaternion.LookRotation(_detectObjects.BraceRayData.normal);
            _playerMovement.Prop_PlayerRigidBody.rotation = Quaternion.RotateTowards(_playerMovement.Prop_PlayerRigidBody.rotation,
                rot, _rotationSpeed);

            direction = new Vector3(0, _braceToJumpUpForce / 2.0f, _braceToJumpBackForce);
        }

        else if (Input.GetKey(KeyCode.D))
            direction = new Vector3(_braceToJumpDirectionForce, _braceToJumpUpForce / 1.5f, 0);

        _playerMovement.Prop_PlayerRigidBody.AddRelativeForce(direction, ForceMode.Impulse);
        _playerMovement.Prop_PlayerRigidBody.useGravity = true;

        return true;
    }

    void CheckPossibleGroundActions()
    {
        if (!_playerMovement.Prop_IsGrounded || _isInAnimation)
            return;

        foreach (RootAnimationSettings rootAction in _rootAnimSetting)
        {
            if (Input.GetKeyDown(KeyCode.Space) &&
                rootAction.CanPerformRootAction(_detectObjects.DownRayData, _detectObjects.ForwardRayData))
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
        _playerMovement.ShouldPlayerMove(false, true);
        _isInAnimation = true;
        ResetAnimationValues();

        _animator.CrossFadeInFixedTime(rootActions.AnimationName, _fixedDuration, 0);
        yield return null;

        AnimatorStateInfo animState = _animator.GetNextAnimatorStateInfo(0);

        if (!animState.IsName(rootActions.AnimationName))
            Debug.LogError("Wrong Anim Name");

        if (animState.IsName("ForwardJumpAnim"))
            _shouldEnableChildCollider = true;

        float waitTimer = 0;
        while (waitTimer <= animState.length)
        {
            waitTimer += Time.fixedDeltaTime;

            if (rootActions.EnableMatching)
                TargetMatching(rootActions, null);

            if (rootActions.ShouldPlayerRotate)
                RotatePlayerDuringAnimation(rootActions, null);

            yield return null;
        }

        // reset the values back
        _shouldEnableChildCollider = false;
        _isInAnimation = false;
        _playerMovement.ShouldPlayerMove(true, false);
    }

    void RotatePlayerDuringAnimation(RootAnimationSettings whichRootAction, BraceAnimationSettings whichBraceAction)
    {
        if (!whichRootAction)
        {
            _playerMovement.Prop_PlayerRigidBody.rotation = Quaternion.RotateTowards(_playerMovement.Prop_PlayerRigidBody.rotation,
                whichBraceAction.RotationDirection, _rotationSpeed);
            return;
        }

        _playerMovement.Prop_PlayerRigidBody.rotation = Quaternion.RotateTowards(_playerMovement.Prop_PlayerRigidBody.rotation,
                whichRootAction.RotationDirection, _rotationSpeed);
    }

    void TargetMatching(RootAnimationSettings actionName, BraceAnimationSettings braceAction)
    {
        if (_animator.isMatchingTarget)
            return;

        if (!actionName)
        {
            Vector3 correction = new Vector3(braceAction.MatchingPosition.x + braceAction.OffSetX,
            braceAction.MatchingPosition.y + braceAction.OffSetY, braceAction.MatchingPosition.z);

            _animator.MatchTarget(correction, transform.rotation, braceAction.BodyPart,
            new MatchTargetWeightMask(braceAction.MatchAxes, 0), braceAction.StartTime, braceAction.EndTime);
            return;
        }

        // ex: foot`s origin starts at the ankle level and targetmatching will match that position
        // to prevent that we added correction pos at the Y axes to move it up accordingly.
        Vector3 correctionPos = new Vector3(actionName.MatchingPosition.x,
            actionName.MatchingPosition.y + actionName.OffSet, actionName.MatchingPosition.z);

        _animator.MatchTarget(correctionPos, transform.rotation, actionName.BodyPart,
            new MatchTargetWeightMask(actionName.MatchAxes, 0), actionName.StartTime, actionName.EndTime);
    }

    //Properties
    public bool IsInAnimation => _isInAnimation;
    public bool ShouldEnableChildCollider => _shouldEnableChildCollider;
}