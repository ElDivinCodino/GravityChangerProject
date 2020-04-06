using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("In seconds")]
    public float gravityInversionCooldown;

    GravitySource[] objects;

    bool gravityInverted = false;
    bool canInvertGravity = true;

    void Start() 
    {
        objects = GameObject.FindObjectsOfType(typeof(GravitySource)) as GravitySource[];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2") && canInvertGravity)
        {
            canInvertGravity = false;
            InvertGravity();
            StartCoroutine(EnableGravityInversion(gravityInversionCooldown));
        }
    }

    private void InvertGravity()
    {
        foreach(GravitySource g in objects)
        {
            g.enabled = !g.enabled;
        }
        gravityInverted = !gravityInverted;
    }


    public bool IsGravityInverted()
    {
        return gravityInverted;
    }

    IEnumerator EnableGravityInversion(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        canInvertGravity = true;
    }
}
