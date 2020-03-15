using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InvertGravity : MonoBehaviour
{
    public GravityAttractor first, second;
    public float switchForce = 300f;

    private void OnTriggerEnter(Collider other) {
        GravityBody gb = other.gameObject.GetComponent<GravityBody>();
        
        if (gb != null)
        {
            gb.influencingPlanet = gb.influencingPlanet == first ? second : first;
            gb.gameObject.GetComponent<Rigidbody>().AddForce(gb.gameObject.transform.up * switchForce);
        }
    }
}
