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

    float spaceLastPressed;
    float spaceSpamCooldown = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        // locks the cursor to the window
        Cursor.lockState = CursorLockMode.Locked;


        body = GetComponent<Rigidbody>();
        viewCamera = GetComponentInChildren<Camera>();
        viewCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }


    void Update()
    {
        // get player walking movement
        Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        move = (movementInput.x * transform.right) + (transform.forward * movementInput.y); 

        //rotate player based on horizontal mouse input
        turn = new Vector2(Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime, Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime);
        transform.Rotate(turn.x * rotationSpeed * Vector3.up);

        // pitch camera based on verticalmouse movement
        xRotation -= turn.y;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);
        viewCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        //get input for gravity toggle
        if (Input.GetKey(KeyCode.Space))
        {
            // prevents spam/button pressing for multiple frames
            if (spaceLastPressed + spaceSpamCooldown < Time.time)
            {
                spaceLastPressed = Time.time;
                body.useGravity = !body.useGravity;
            }
        }

        // the next two statements control the vertical component of movement
        if (Input.GetKey(KeyCode.LeftShift))
        {
            move += transform.up;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            move -= transform.up;
        }
        // normalise vector to ensure magnitude is 1 (and therefore velocity is constant)
        move = move.normalized;
    }

    
    private void FixedUpdate()
    {
        // the body is moved in FixedUpdate() so physics behaves itself
        body.MovePosition(body.position + (move * moveSpeed * Time.fixedDeltaTime));
    }
}
