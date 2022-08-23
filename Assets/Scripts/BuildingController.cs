using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildingController : MonoBehaviour
{
    // enum for storing the player state
    enum State { 
        BUILDING,
        EDITING
    }
    State state = State.BUILDING;

    // names of the building prefabs and index
    string[] allPrefabNames = { "Cube", "Sphere", "Cylinder", "Wall" };
    int selectedIndex;

    float maxPlaceDistance = 20f;
    float scrollCooldown = 0.05f;
    float lastScrollTime;
    public UnityEvent<Transform> OnShapeChanged;

    float moveSpeed = 4f;
    float rotateSpeed = 75f;
    float scaleSpeed = 0.8f;
    float flashesPerSecond = 2f;

    // layer masks for raycasting in different modes (we don't want to delete the ground!)
    LayerMask buildMask;
    LayerMask editMask;

    // the shape to place
    Transform buildShape;

    // the transparent object in build mode
    Transform ghost;

    // the shape being edited
    Transform editingShape;

    // colors to store ogiginal and flashing colour
    Color originalColour;
    Color flashingColour = Color.magenta;
    // Start is called before the first frame update
    void Start()
    {
        //initialises the layer masks
        buildMask = LayerMask.GetMask("Ground", "Obstacle");
        editMask = LayerMask.GetMask("Obstacle");

        // puts the ghost at the origin but makes it invisible
        InitialiseGhost();
        EditGhostVisibility(false);

        // invokes OnShapeChanged to ensure the shape is initially displayed on the UI panel
        if (OnShapeChanged != null)
        {
            Transform shape = Resources.Load<Transform>(allPrefabNames[selectedIndex]);
            OnShapeChanged.Invoke(shape);
        }
    }

    // initialises the ghost so it is never null
    void InitialiseGhost()
    {
        Transform shape = Resources.Load<Transform>(allPrefabNames[selectedIndex]);

        //sneaky line copies the shape for later placing
        buildShape = shape;

        // ahh! ghosts!
        ghost = Instantiate(shape);

        // removes the collider of the ghost so it's more ghosty
        Destroy(ghost.GetComponent<Collider>());

        // makes ghost semi-transparent
        Renderer renderer = ghost.GetComponent<Renderer>();
        Material cubeMaterial = renderer.material;
        Color cubeColor = cubeMaterial.color;
        cubeColor.a = 0.5f;
        cubeMaterial.color = cubeColor;

    }

    void Update()
    {
        // places the ghost where the player is looking
        GhostFollowLook();

        // state machine for the builder
        switch (state)
        {
            case State.BUILDING:
                if (HasScrolled())
                {
                    OnScrollWheel();

                    // updates the ghost shape to the newly selected shape
                    UpdateGhostShape();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    // spawns the selected shape
                    SpawnShape();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    // deletes the shape the player is looking at (or tries to)
                    DeleteShape();
                }
                else if (Input.GetMouseButtonDown(2))
                {
                    // selects the shape the player is looking at (or tries to)
                    SelectShape();
                }
                break;

            case State.EDITING:

                // the following four statements control the rotation of the selected shape, and are able to be combined on different axes
                if (Input.GetKey(KeyCode.Keypad4))
                {
                    RotateSelectedShape(new Vector3(0, 1, 0));
                }
                else if (Input.GetKey(KeyCode.Keypad6))
                {
                    RotateSelectedShape(new Vector3(0, -1, 0));
                }
                if (Input.GetKey(KeyCode.Keypad8))
                {
                    RotateSelectedShape(new Vector3(1, 0, 0));
                }
                else if (Input.GetKey(KeyCode.Keypad2))
                {
                    RotateSelectedShape(new Vector3(-1, 0, 0));
                }

                // the follow two statements control the scale of the selected shape, both don't work at the same time
                if (Input.GetKey(KeyCode.Equals))
                {
                    ScaleSelectedShape(1);
                }
                else if (Input.GetKey(KeyCode.Minus))
                {
                    ScaleSelectedShape(-1);
                }

                // the following four statements control the position of the selected shape, and are able to be combined on different axes
                // it is worth noting the direction is local (AKA dependent on the shapes rotation), meaning any position can be reached given enough fiddling
                if (Input.GetKey(KeyCode.L))
                {
                    MoveShape(new Vector3(1, 0, 0));
                }
                else if (Input.GetKey(KeyCode.J))
                {
                    MoveShape(new Vector3(-1, 0, 0));
                }
                if (Input.GetKey(KeyCode.K))
                {
                    MoveShape(new Vector3(0, -1, 0));
                }
                else if (Input.GetKey(KeyCode.I)) 
                {
                    MoveShape(new Vector3(0, 1, 0));
                }

                // pressing the backspace key returns to build mode
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    state = State.BUILDING;
                    EditGhostVisibility(true);
                }
                break;
            default:
                break;
        }
    }

    // returns if the user used the scroll wheel
    // this could easily be excluded but provides insight regarding what the condition is checking
    bool HasScrolled()
    {
        return Input.mouseScrollDelta.y != 0 && lastScrollTime + scrollCooldown < Time.time;
    }

    // called when the user used the scroll wheel
    // changes the seleted shape if the previous scroll was long enough ago
    void OnScrollWheel()
    {
        // configures the index to next/previous, wrapping around if -1 or out of bounds
        lastScrollTime = Time.time;
        if (Input.mouseScrollDelta.y == -1 && selectedIndex == 0)
        {
            selectedIndex = allPrefabNames.Length - 1;
        }
        else if (Input.mouseScrollDelta.y == 1 && selectedIndex == allPrefabNames.Length - 1)
        {
            selectedIndex = 0;
        }
        else
        {
            selectedIndex += (int)Input.mouseScrollDelta.y;
        }

        // invokea the OnShapeChanged event if any listeners are attached
        if (OnShapeChanged != null)
        {
            buildShape = Resources.Load<Transform>(allPrefabNames[selectedIndex]);
            OnShapeChanged.Invoke(buildShape);
        }
    }

    // ensures the ghost follows where the player is looking
    void GhostFollowLook()
    {
        RaycastHit hitInfo;
        if (RaycastWhereLooking(buildMask, out hitInfo))
        {
            EditGhostVisibility(true);// if something is hit, the ghost should be visible

            // calculates and sets the new position and rotation of the ghost
            Vector3 spawnPosition = new Vector3(hitInfo.point.x, hitInfo.point.y + (ghost.localScale.y / 2), hitInfo.point.z);
            if (ghost.CompareTag("Cylinder"))// I have no idea why cylinders 'centre' is the bottom of the shape? or am I just a moron?
            {
                spawnPosition.y += ghost.localScale.y / 2;
            }
            Camera cameraView = Camera.main;
            Vector3 direction = spawnPosition - new Vector3(cameraView.transform.position.x, spawnPosition.y, cameraView.transform.position.z);
            ghost.position = spawnPosition;
            ghost.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            // if the ray doesn't hit anything, don't show the ghost
            EditGhostVisibility(false);
        }
    }

    // called when the user scrolls and the shape changes
    // the ghost must be identical to the shape to maintain the effect
    void UpdateGhostShape()
    {
        Vector3 position = ghost.position;
        Quaternion rotation = ghost.rotation;

        // removes the now deselected ghost as it needs changing
        RemoveGhost();

        //puts the new ghost bin the same position the oold ghost was
        ghost = Instantiate(buildShape, position, rotation);

        // removes the ghosts collider to make it a true ghost (spooky!)
        Collider collider = ghost.GetComponent<Collider>();
        Destroy(collider);

        // makes the ghost transparent-ish
        Renderer renderer = ghost.GetComponent<Renderer>();
        Material cubeMaterial = renderer.material;            
        Color cubeColor = cubeMaterial.color;
        cubeColor.a = 0.5f;
        cubeMaterial.color = cubeColor;
    }

    // destroys the ghost (R.I.P)
    void RemoveGhost()
    {
        Destroy(ghost.gameObject);
    }

    // changes whether the renderer of the ghost is enabled
    void EditGhostVisibility(bool visible)
    {
        // we don't the ghost to become visible if not building
        if (state == State.BUILDING)
        {
            Renderer renderer = ghost.GetComponent<Renderer>();
            renderer.enabled = visible;
        }
    }

    // spawns the selected shape
    void SpawnShape()
    {
        // if the player can see the ghost
        if (ghost.GetComponent<Renderer>().enabled)
        {
            // we can put put it where the ghost is, no need to do all that raycasting again
            Instantiate(buildShape, ghost.position, ghost.rotation);
        }
        // NOTE FOR READER: I raycasted here too before realising I could just use the ghost position/rotation
    }

    // deletes the shape the player is looking at
    void DeleteShape()
    {
        RaycastHit hitInfo;
        if (RaycastWhereLooking(editMask, out hitInfo))
        {
            Transform shapeToDelete = hitInfo.transform;
            Destroy(shapeToDelete.gameObject);
        }
    }

    // selects the shape the player is looking at
    void SelectShape()
    {
        RaycastHit hitInfo;
        if (RaycastWhereLooking(editMask, out hitInfo))
        {
            // stores shape editing
            editingShape = hitInfo.transform;

            // remove ghost and start flashing
            EditGhostVisibility(false);
            state = State.EDITING;
            StartCoroutine(FlashSelectedShape());
        }
    }

    // returns whether ray hit something and hitInfo stores hit details
    // this function exists because I used this snippet multiple times
    bool RaycastWhereLooking(LayerMask mask, out RaycastHit hitInfo)
    {
        Camera cameraView = GetComponentInChildren<Camera>();
        Ray ray = new Ray(cameraView.transform.position, cameraView.transform.forward);

        return Physics.Raycast(ray, out hitInfo, maxPlaceDistance, mask);
    }

    // coroutine for flashing effect on selected shape
    IEnumerator FlashSelectedShape()
    {
        Renderer renderer = editingShape.GetComponent<Renderer>();
        Material shapeMaterial = renderer.material;
        originalColour = shapeMaterial.color;
        float timer = 0;
        while (state == State.EDITING)
        {
            //lerp back and forward between the original color and flash color
            shapeMaterial.color = Color.Lerp(originalColour, flashingColour, Mathf.PingPong(timer * flashesPerSecond * 2, 1));// first argument is doubled as it has to go from original-flash-original which counts as 2 flashes
            timer += Time.deltaTime;
            yield return null;
        }
        // when deselected, return to original colour
        shapeMaterial.color = originalColour;
    }

    // moves the selected shape depending on the argment
    void MoveShape(Vector3 movement)
    {
        editingShape.transform.Translate(moveSpeed * Time.deltaTime * movement);
    }

    // rotates the selected shape depending on the argment
    void RotateSelectedShape(Vector3 rotation)
    {
        editingShape.Rotate(rotateSpeed * Time.deltaTime * rotation);
    }

    // scales the selected shape depending on the argment
    void ScaleSelectedShape(int scale)
    {
        float minScale = Mathf.Min(editingShape.localScale.x, editingShape.localScale.y, editingShape.localScale.z);
        if (minScale > 0.1f || scale > 0)
        {
            editingShape.localScale += scale * scaleSpeed * Time.deltaTime * Vector3.one;
        }
    }
}