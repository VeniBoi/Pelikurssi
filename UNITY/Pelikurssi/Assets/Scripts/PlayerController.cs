//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using UnityEngine;
using System.Collections;
using UnityEngine.UI;



public class PlayerController : MonoBehaviour


{
	

	public float walkSpeed = 2;
	public float runSpeed = 6;
	public float gravity = -12;
	public float jumpHeight = 1;
	[Range(0, 1)]
	public float airControlPercent;

	public float turnSmoothTime = 0.2f;
	float turnSmoothVelocity;

	public float speedSmoothTime = 0.1f;
	float speedSmoothVelocity;
	float currentSpeed;
	float velocityY;



	public Animator animator;
	Transform cameraT;
	CharacterController controller;

	void Start()
	{
		animator = GetComponent<Animator>();
		cameraT = Camera.main.transform;
		controller = GetComponent<CharacterController>();

		

	}

	void Update()
	{
		// input
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		Vector2 inputDir = input.normalized;
		bool running = Input.GetKey(KeyCode.LeftShift);

		Move(inputDir, running);


		// animator
		float animationSpeedPercent = ((running) ? 1 : .5f) * inputDir.magnitude;
		animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);

	}

	void Move(Vector2 inputDir, bool running)
	{
		if (inputDir != Vector2.zero)
		{
			float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
		}

		float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
		currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

		velocityY += Time.deltaTime * gravity;
		Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;

		controller.Move(velocity * Time.deltaTime);
		currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

		if (controller.isGrounded)
		{
			velocityY = 0;
		}

	}

	float GetModifiedSmoothTime(float smoothTime)
	{
		if (controller.isGrounded)
		{
			return smoothTime;
		}

		if (airControlPercent == 0)
		{
			return float.MaxValue;
		}
		return smoothTime / airControlPercent;
	}

	

	

}






