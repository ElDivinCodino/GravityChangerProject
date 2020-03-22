using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InvertGravity : MonoBehaviour
{
    public GravityAttractor first, second;
    public float switchForce = 1000f;

    private void OnTriggerEnter(Collider other) {
        GravityBody gb = other.gameObject.GetComponent<GravityBody>();
        
        if (gb != null)
        {
            StartCoroutine(ChangeGravity(1f, gb));
            gb.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            gb.gameObject.GetComponent<Rigidbody>().AddForce(-gb.gameObject.transform.up * switchForce);
        }
    }

    IEnumerator ChangeGravity (float secondsToSwitch, GravityBody gb)
    {
        GravityAttractor newInfluencingPlanet = gb.influencingPlanet == first ? second : first;
        gb.influencingPlanet = newInfluencingPlanet;
        yield return new WaitForSeconds(secondsToSwitch);
        gb.influencingPlanet = newInfluencingPlanet;
    }
}
