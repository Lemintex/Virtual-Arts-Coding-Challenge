using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    float rotateSpeedMultiplier = 75f;
    float rotateSpeedX;
    float rotateSpeedY;
    float rotateSpeedZ;
    Transform shape;

    System.Random rnd;
    // Start is called before the first frame update
    void Start()
    {
        shape = gameObject.transform;
        rnd = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        // rotates the UI shape a random amount on each axis
        rotateSpeedX = (float)rnd.NextDouble() * rotateSpeedMultiplier;
        rotateSpeedY = (float)rnd.NextDouble() * rotateSpeedMultiplier;
        rotateSpeedZ = (float)rnd.NextDouble() * rotateSpeedMultiplier;
        shape.Rotate(Time.deltaTime * new Vector3(rotateSpeedX, rotateSpeedY, rotateSpeedZ));
    }
}
