/*
** WARNING: By applying gravity ourselves the Rigidbody no longer goes to sleep.
** PhysX puts bodies to sleep when it can, reducing the amount of work it has to do. 
** For this reason it is a good idea limit how many bodies are affected by our custom gravity.
*/

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    [SerializeField, Tooltip("Determines if gameobject is allowed to float for a moment before sleeping.")]
    bool floatToSleep = false;

    Rigidbody body;
    float floatDelay;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (floatToSleep)
        {
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f) // Stop the rigidbody if its velocity is tiny (so that body.IsSpleeping() can be called)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)   // Wait for 1 second just to be sure the rigidbody is not momentarily hovering in place for some reasons
                {
                    return;
                }
            }
            else
            {
                floatDelay = 0f;
            }
        }

        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }
}