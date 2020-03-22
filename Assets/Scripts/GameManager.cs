using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("In seconds")]
    public float gravityInversionCooldown;

    GravityChanger[] objects;

    bool gravityInverted = false;
    bool canInvertGravity = true;

    void Start() 
    {
        objects = GameObject.FindObjectsOfType(typeof(GravityChanger)) as GravityChanger[];
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
        Physics.gravity *= -1;
        gravityInverted = !gravityInverted;

        RotateObjects();
    }

    private void RotateObjects()
    {
        foreach(GravityChanger g in objects)
            g.RotateToGravity();
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
