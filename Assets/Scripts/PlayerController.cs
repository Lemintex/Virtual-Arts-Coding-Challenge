using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody body;
    Camera viewCamera;

    Vector3 move;
    Vector2 turn;
    float moveSpeed = 5f;
    float xRotation = 0f;
    float sensitivity = 200f;
    float rotationSpeed = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        body = GetComponent<Rigidbody>();
        viewCamera = GetComponentInChildren<Camera>();
        viewCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }


    void Update()
    {
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        move = ((movementInput.x * transform.right) + (transform.forward * movementInput.y)).normalized;
            
        turn = new Vector2(Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime, Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime);
        if (turn.x != 0)transform.Rotate(turn.x * rotationSpeed * Vector3.up);

        xRotation -= turn.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        viewCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }


    private void FixedUpdate()
    {
        body.MovePosition(body.position + (move * moveSpeed * Time.fixedDeltaTime));
    }
}
