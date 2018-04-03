//Spagettikoodi a'la Veni
//Use at your own risk. :-D

using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public Camera Kamera;

	public float lammisenkiki;

	public bool lockCursor;
	public float mouseSensitivity = 5f;
    public Transform target;
    public float dstFromTarget = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 85);

    public float rotationSmoothTime = .12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    float yaw;
    float pitch;

    void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;  //Lukitaan kursori näyttöön ja piilotetaan se
            Cursor.visible = false;
        }

		
    }

    void LateUpdate()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target.position - transform.forward * dstFromTarget;

		

    }

	public void AdjustSensitivity (float newSpeed)
	{
		mouseSensitivity = newSpeed;
	}

	

}