using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
public class GravityBody : MonoBehaviour {
	
	public GravityAttractor influencingPlanet;
	Rigidbody rb;
	
	void Awake () {
		rb = GetComponent<Rigidbody> ();

		// Disable rigidbody gravity and rotation as this is simulated in GravityAttractor script
		rb.useGravity = false;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	void FixedUpdate () {
		// Allow this body to be influenced by planet's gravity
		influencingPlanet.Attract(rb);
	}
}