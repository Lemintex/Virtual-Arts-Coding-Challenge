using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BuildingController))]
[RequireComponent (typeof(PlayerController))]
public class Player : MonoBehaviour
{
    float movementSpeed = 5;
    Camera viewCamera;
    PlayerController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }
}
