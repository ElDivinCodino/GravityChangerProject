using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GravityChanger[] objects;

    void Start() 
    {
        objects = GameObject.FindObjectsOfType(typeof(GravityChanger)) as GravityChanger[];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            InvertGravity();
        }
    }

    private void InvertGravity()
    {
        Physics.gravity *= -1;

        RotateObjects();
    }

    private void RotateObjects()
    {
        foreach(GravityChanger g in objects)
            g.RotateToGravity();
    }
}
