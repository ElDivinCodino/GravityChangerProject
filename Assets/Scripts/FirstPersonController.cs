using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GravityBody))]
public class FirstPersonController : MonoBehaviour
{

    // public vars
    public float mouseSensitivityX = 1;
    public float mouseSensitivityY = 1;
    public float walkSpeed = 6;
    public float jumpForce = 220;
    public LayerMask groundedMask;

    // System vars
    bool grounded;
    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;
    float verticalLookRotation;
    Transform cameraTransform;
    Rigidbody rb;
    
	Ray ray;
	RaycastHit hit;
	Vector3 pos;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        // Look rotation:
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
        cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;

        // Calculate movement:
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = new Vector3(inputX, 0, inputY).normalized;
        Vector3 targetMoveAmount = moveDir * walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

        // Jump
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                rb.AddForce(transform.up * jumpForce);
            }
        }

        // Grounded check
		ray = new Ray(transform.position, -transform.up);
        
		if (Physics.Raycast(ray, out hit, 10, groundedMask))
        {
			pos = hit.point;
            grounded = true;
        }
        else
        {
            grounded = false;
        }

    }

	public RaycastHit GetRaycastHit() { return hit; }

    void FixedUpdate()
    {
        // Apply movement to rb
        Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + localMove);
    }
}