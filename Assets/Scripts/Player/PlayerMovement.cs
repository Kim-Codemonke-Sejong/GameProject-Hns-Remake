using UnityEngine;

public class PlayerMovement : MonoBehaviour, IPlayerFixedBehaviour, IPlayerUpdateBehaviour
{
    [Header("Referance")]
    public Transform orientation;
    public Transform groundCheck;
    Camera _mainCamera;
    Rigidbody _rb;
    PlayerInputManager _input;

    [Header("GroundCheck")]
    public LayerMask groundLayer;
	public float groundDistance = 0.4f;
    bool _grounded;

    [Header("Speed")]
    public float moveSpeed = 5f;
	public float rotateInterpolation = 0.15f;
	public float groundDrag = 5f;

    //For GetCameraRotation()
    Vector3 _camForward;
	Vector3 _camRight;

    //For CharactorMovement()
    private Vector3 _moveDirection;
	private Vector3 _rotateDirection;
	private float _rotateRate;

    public Vector3 MoveDirection => _moveDirection;

    private void Start()
	{
		_rb = GetComponent<Rigidbody>();
		_mainCamera = Camera.main;
        _input = GetComponent<PlayerInputManager>();   
    }
    public void OnPlayerUpdate()
	{
		GetCameraRotation();
		_grounded = GroundCheck();
		SpeedControl();
	}
    public void OnPlayerFixedUpdate()
    {
        CharacterMovement();
    }

    void GetCameraRotation()
    {
        _camForward = _mainCamera.transform.forward;
		_camRight = _mainCamera.transform.right;

		_camForward.y = 0;
		_camRight.y = 0;

		_camForward.Normalize();
		_camRight.Normalize();
    }

    private bool GroundCheck()
	{
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
	}

    private void SpeedControl()
	{
		Vector3 _flatVel = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);

		//limit Velocity if need
		if( _flatVel.magnitude > moveSpeed )
		{
			Vector3 _limitVel = _flatVel.normalized * moveSpeed;
			_rb.linearVelocity = new Vector3(_limitVel.x, _rb.linearVelocity.y, _limitVel.z);
		}
	}

    //Make Character Move

	void CharacterMovement()
	{
		//Drag On Ground
		if( _grounded )
			_rb.linearDamping = groundDrag;
		else
			_rb.linearDamping = 0;

		//Calculate Final Vector
		_moveDirection = _camForward * _input.move.y + 
			_camRight * _input.move.x;
		/*	Rotate Player. Cut into PlayerNormalRotate
		//reset rotate
		_rotateDirection = _moveDirection;

        		//Rotate Head in 0 < moveDirection only to stop spamming debug message.
		if( _moveDirection != Vector3.zero )
			transform.rotation = Quaternion.Slerp
				(transform.rotation, 
				 Quaternion.LookRotation(_rotateDirection.normalized), 
				 rotateInterpolation);
		*/
		//Move Character
		_rb.AddForce(_moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}
