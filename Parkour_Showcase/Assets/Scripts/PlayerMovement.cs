using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] float _speed = 1000;
    [SerializeField] float _jumpForce = 6;
    [SerializeField] float _jumpCoolDown = 0.7f;
    [SerializeField] float _airControl = 0.1f;
    [SerializeField] float _dragValue = 5;
    [SerializeField] float _turnSmoothTime = 0.1f;
    [SerializeField] bool readyToJump = true; // for invoking function call

    bool shouldJump = false; // for Actual jump calling
    float _turnSmoothVelocity = 0.0f;

    [Header("Ray Settings")]
    [SerializeField] float _rayDistance = 0.55f;
    [SerializeField] float _rayOffsetY = 0.53f;
    [SerializeField] bool controller = true;
    [SerializeField] LayerMask _groundLayer;

    Vector3 _inputValues = Vector3.zero;
    Vector3 _playerForward = Vector3.zero;
    Rigidbody _rigidbBody = null;
    [SerializeField] CapsuleCollider _capsuleCollider;

    private void Start()
    {
        _rigidbBody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _capsuleCollider.enabled = true;
    }

    private void Update()
    {
        IsGrounded();
        if (!controller)
            return;

        // will get the forward of player before controller is off for the forwardJump Animation
        _playerForward = transform.forward;

        _inputValues = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        OnAndOffGroundBehaviours(IsGrounded());
        PlayerJumpInput();
    }

    private void FixedUpdate()
    {
        if (!controller)
            return;

        CameraBasedMovement();
        Jump(shouldJump);
    }

    void PlayerJumpInput()
    {
        if (Input.GetKey(KeyCode.F) && IsGrounded() && readyToJump)
        {
            shouldJump = true;
            readyToJump = false;
            Invoke(nameof(JumpReset), _jumpCoolDown);
        }
    }

    void Jump(bool shouldPlayerJump)
    {
        if (shouldPlayerJump)
            _rigidbBody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }

    void JumpReset()
    {
        readyToJump = true;
    }

    void CameraBasedMovement()
    {
        if (_inputValues.magnitude >= 0.1f)
        {
            float cameraY = Camera.main.transform.eulerAngles.y;

            float targetAngle = Mathf.Atan2(_inputValues.x, _inputValues.z) * Mathf.Rad2Deg + cameraY;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);

            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            moveDirection.Normalize();

            if (IsGrounded())
                _rigidbBody.AddForce(_speed * Time.fixedDeltaTime * moveDirection, ForceMode.Force);
            else
                _rigidbBody.AddForce(_speed * _airControl * Time.fixedDeltaTime * moveDirection, ForceMode.Force);
        }
    }

    public void ShouldPlayerMove(bool controller, bool shouldColliderTrigger)
    {
        this.controller = controller;
        if (!controller)
        {
            _inputValues = Vector3.zero;
            _capsuleCollider.isTrigger = shouldColliderTrigger;
            _rigidbBody.useGravity = false;
        }
        if (controller)
        {
            _rigidbBody.useGravity = true;
            _capsuleCollider.isTrigger = false;
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + _rayOffsetY, transform.position.z),
            Vector3.down, _rayDistance, _groundLayer);
    }

    void OnAndOffGroundBehaviours(bool isGround)
    {
        if (isGround)
            _rigidbBody.drag = _dragValue;
        else
        {
            _rigidbBody.drag = 0;
            shouldJump = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (IsGrounded())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + _rayOffsetY, transform.position.z),
                    Vector3.down * _rayDistance);
        }
    }

    // Properties Section
    public bool Prop_IsGrounded => IsGrounded();
    public float Prop_PlayerVelocity => Mathf.Clamp01(_inputValues.sqrMagnitude);
    public Vector3 Prop_PlayerForward => _playerForward;
    public Rigidbody Prop_PlayerRigidBody => _rigidbBody;
    public CapsuleCollider Prop_PlayerCollider => _capsuleCollider;
}