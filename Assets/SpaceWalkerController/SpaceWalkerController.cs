
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    [RequireComponent(typeof(PlayerInput))]
    public class SpaceWalkerController : MonoBehaviour
    {
        private PlayerControls _playerControls = null;
        private Rigidbody _rigidbody = null;

        private const float _terminalVelocity = 53f;

        [SerializeField]
        private float RotationSmoothTime = 0.12f;

        [SerializeField] private float _movementSpeed = 5f;

        //movement inputs
        private Vector2 _movement = Vector2.zero;
        private float _targetRotation = 0f;

        private float _rotationVelocity;

        //jumping
        public float Gravity = -15.0f;
        public float maxJumpHeight = 2f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        private float _verticalVelocity;
        public bool isJumpPressed = false;

        private bool isGrounded = false;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;


        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _playerControls = new PlayerControls();

            _playerControls.Player.Jump.started += OnJumpPressed;
            _playerControls.Player.Jump.canceled += OnJumpPressed;

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void OnDestroy()
        {
            _playerControls.Player.Jump.started -= OnJumpPressed;
            _playerControls.Player.Jump.canceled -= OnJumpPressed;
        }

        private void OnEnable()
        {
            _playerControls.Enable();
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }
        private void OnJumpPressed(InputAction.CallbackContext context)
        {
            isJumpPressed = context.ReadValueAsButton();
        }

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            GroundCheck();
            JumpAndGravity();

            UpdateMovement();
        }

        private void ReadInput()
        {
            _movement = _playerControls.Player.Move.ReadValue<Vector2>();
        }

        private void UpdateMovement()
        {
            Vector3 inputDirection = new Vector3(_movement.x, 0f, _movement.y).normalized;

            //cheaper than magnitude check and uses floating point approximation
            if (_movement != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.rotation.eulerAngles.y;
                float rotationAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);
            }
            else
            {
                //auto align if no input happens. (difficulty setting maybe?)

                _targetRotation = 90f; //try to face him right if no input happens
                float rotationAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);
            }
            //remove velocity from turning
            _rigidbody.angularVelocity = Vector3.zero;

            //apply vertical velocity
            _rigidbody.MovePosition(transform.position + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

            Vector3 targetDirection = Quaternion.Euler(0f, _targetRotation, 0f) * Vector3.forward;

            //move
            if (_movement != Vector2.zero)
            {
                _rigidbody.AddForce(targetDirection, ForceMode.Impulse);
            }
            else
            {
                if (isGrounded)
                    _rigidbody.AddForce(targetDirection.normalized * _movementSpeed * Time.deltaTime, ForceMode.VelocityChange);
            }
        }

        private bool isFlying = false;
        public bool hasFuel = true;

        private void JumpAndGravity()
        {
            if (isGrounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                hasFuel = true; //regenerate while grounded

                isFlying = false;
                if (isJumpPressed)
                {
                    _verticalVelocity = 5f;//Mathf.Sqrt(maxJumpHeight * -2f * Gravity);
                    isFlying = true;
                }

                // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * Time.deltaTime;
                }
            }
            else
            {
                if (isJumpPressed && hasFuel)
                {
                    isFlying = true;
                    _verticalVelocity = 5f;
                }
                else
                {
                    isFlying = false;
                    // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
                    if (_verticalVelocity < _terminalVelocity)
                    {
                        _verticalVelocity += Gravity * Time.deltaTime;
                    }
                }
            }
        }

        private void JumpAndGravityObsolete()
        {
            if (isGrounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;
/*
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }
*/
                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (isJumpPressed && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(maxJumpHeight * -2f * Gravity);
/*
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
*/
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
/*
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
*/
                }

                // if we are not grounded, do not jump
                isJumpPressed = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void GroundCheck()
        {
            isGrounded = IsGrounded();

            //animator
        }

        private bool IsGrounded()
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.25f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
                return true;
            else
                return false;
        }

    }
}

/*

public class SpaceWalkerController : MonoBehaviour
{
    //input fields
    private ThirdPersonInputActions playerActionsAsset;
    private InputAction move;

    //movement fields
    private Rigidbody rb;
    [SerializeField]
    private float movementForce = 1f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    [SerializeField]
    private Camera playerCamera;
    private Animator animator;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        playerActionsAsset = new ThirdPersonInputActions();
        animator = this.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerActionsAsset.Player.Jump.started += DoJump;
        move = playerActionsAsset.Player.Move;
        playerActionsAsset.Player.Enable();
    }

    private void OnDisable()
    {
        playerActionsAsset.Player.Jump.started -= DoJump;
        playerActionsAsset.Player.Disable();
    }

    private void FixedUpdate()
    {

        forceDirection += move.ReadValue<Vector2>().x * GetCameraForward(playerCamera) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraRight(playerCamera) * movementForce;

        if (forceDirection == Vector3.zero)
        {
            forceDirection = Vector3.right * maxSpeed * Time.fixedDeltaTime;
        }
        else
            forceDirection = forceDirection;



        rb.AddForce(forceDirection, ForceMode.VelocityChange);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;

        //look
        LookAt();
    }


    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
            //rb.rotation = Quaternion.RotateTowards(rb.rotation, Quaternion.LookRotation(direction), 0.12f);
        else
            rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f))
            return true;
        else
            return false;
    }

    private void DoAttack(InputAction.CallbackContext obj)
    {
        animator.SetTrigger("attack");
    }
}*/