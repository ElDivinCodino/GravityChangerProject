using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveSphere : MonoBehaviour
{
    // Note that setting both max speeds to the same value can produce inconsistent results due to precision limitations. 
    // It's better to make the max snap speed a bit higher or lower than the max speed.

    [Tooltip("In metres/seconds")]
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [Tooltip("Max speed at which the player remains snapped to the ground")]
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [Tooltip("Distance (in metres) used to check whether player must be snapped or not")]
    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [Tooltip("Layers that represent ground")]
    [SerializeField]
    LayerMask probeMask = -1;

    [Tooltip("Layers that represent stairs")]
    [SerializeField]
    LayerMask stairsMask = -1;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [Tooltip("In metres.")]
    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [Tooltip("Maximum angle of an object's slope to be considered as ground.")]
    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f;

    [Tooltip("Maximum angle of a stair's slope to be climbed.")]
    [SerializeField, Range(0, 90)]
    float maxStairsAngle = 50f;

    [Tooltip("Player input space configuration. If not set, player input space configuration is World Space.")]
    [SerializeField]
    Transform playerInputSpace = default;

    Rigidbody body;
    Vector3 velocity, desiredVelocity;
    Vector3 contactNormal, steepNormal; // A steep contact is one that is too steep to count as ground, but isn't a ceiling or overhang. 
    bool desiredJump;
    int groundContactCount, steepContactCount;
    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;
    int jumpPhase;
    float minGroundDotProduct, minStairsDotProduct;
    int stepsSinceLastGrounded, stepsSinceLastJump;

    // Gravity-related stuff
    Vector3 upAxis, rightAxis, forwardAxis;


    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
		body.useGravity = false;
        OnValidate();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)	// If the movement is relative to a transform
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else // Otherwise the movement is relative to World Space
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        // |= because we could have pressed the button during a frame out of fixedUpdate, hence the jump could be not happened but the variable resets to false anyway the next frame
        desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

		velocity += gravity * Time.deltaTime;

        body.velocity = velocity;
        ClearState();
    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;    // WARNING: could cause integer overflow if the player is not grounded for some in-game months :)
        stepsSinceLastJump += 1;    // WARNING: could cause integer overflow if the player doesn't jump for some in-game months
        velocity = body.velocity;

        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;

            // Reset the jump phase only when player is more than one step after a jump was initiated, to avoid false landing
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }

            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        // Check whether player is on the ground. If so, use the contact normal for the jump direction
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        // If not, check whether player is on something steep. If so, use the steep normal instead
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0; // To make possible wall jumping into a new sequence of air jumps
        }
        // If player is n-jumping, use the contact normal
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0) // Skip the first jump phase when air jumping to prevent air jumping one extra time after falling off a surface without jumping
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);

        jumpDirection = (jumpDirection + upAxis).normalized; // To increase vertical speed when jumping off a vertical wall

        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }

        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    // Evaluate multiple collisions and sum their normals to obtain an average collision vector
    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);

            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (upDot > -0.01f) // The dot product of a perfectly vertical wall should be zero, but let's be a bit lenient.
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    // Align our desired velocity with the ground by projecting the velocity on a plane
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    // Keeps player stuck to the ground if needed 
    bool SnapToGround()
    {
        // If player has just jumped or it is on air because of it 
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        // Player just lost contact with the ground but is still above ground, so must snap to it.
        groundContactCount = 1;
        contactNormal = hit.normal;

        // Adjust player's velocity to align with the ground
        float dot = Vector3.Dot(velocity, hit.normal);
        // The velocity might already point somewhat down, in which case realigning it would slow convergence to the ground. Adjust only if dot product is negative
        if (dot > 0f)
            velocity = (velocity - hit.normal * dot).normalized * speed;

        return true;
    }

    // Returns the appropriate minimum for a given layer (to differenciate between normal surfaces/ground and stairs)
    float GetMinDot(int layer)
    {
        // Support a mask for any combination of layers.
        return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    // Try to convert the steep contacts into virtual ground, and return whether the operation succeded or not.
    // Needed to make player being able to move a bit and jump while in a crevasse or in similar places where it previously got stuck.
    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();

            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }
}