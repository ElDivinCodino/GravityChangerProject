using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityChanger : MonoBehaviour
{
    public float rotationSpeedMultiplier = 1;
    public GameManager gameManager;
    public Transform rotationReference;

    float rotationSpeed;
    Ray ray;
    RaycastHit hit;

    Quaternion targetRot;

    public void RotateToGravity()
    {
        rotationSpeed = GetRequiredSpeed();
        targetRot = transform.rotation * Quaternion.Euler(transform.right * 180);
        StartCoroutine(Rotate());
    }

    IEnumerator Rotate()
    {
        float speed = rotationSpeed * rotationSpeedMultiplier;
        Quaternion previousRot = transform.rotation;

        for (float t = 0f; t < 1f; t += speed * Time.deltaTime)
        {
            transform.rotation = Quaternion.Lerp(previousRot, targetRot, t);
            yield return null;
        }

        GetComponent<Rigidbody>().velocity = Vector3.zero;

        ray = new Ray(transform.position, -transform.up);

        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(hit.normal.x, currentRotation.y, currentRotation.z);
        }
    }

    float GetRequiredSpeed()
    {
        ray = new Ray(transform.position, transform.up);

        if (Physics.Raycast(ray, out hit, 100))
        {
            return Vector3.Distance(transform.position, hit.point) * 0.05f;
        }
        else
        {
            return 0.5f;
        }
    }
}
