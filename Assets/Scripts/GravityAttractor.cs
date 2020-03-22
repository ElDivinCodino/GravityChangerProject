﻿using UnityEngine;
using System.Collections;

public class GravityAttractor : MonoBehaviour
{

    public float gravity = -9.81f;
    public bool isCentrifugal = false;

    Vector3 gravityUp;

    public void Attract(Rigidbody body)
    {
        //gravityUp = body.transform.GetComponent<FirstPersonController>().GetRaycastHit().normal;

        // Apply downwards gravity to body
        body.AddForce(gravityUp * gravity);

		Debug.DrawLine(body.transform.position, gravityUp * gravity);

        // Align bodies up axis with the centre of planet
        body.rotation = Quaternion.FromToRotation(body.transform.up, gravityUp) * body.rotation;
    }
}