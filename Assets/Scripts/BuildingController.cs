using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    enum State { 
        BUILDING,
        EDITING
    }
    State state = State.BUILDING;
    string[] allPrefabNames = { "Cube", "Sphere", "Cylinder", "Wall" };
    int selectedIndex;
    float scrollCooldown = 0.05f;
    float lastScrollTime;

    float rotateSpeed = 25f;
    float scaleSpeed = 0.8f;
    float flashesPerSecond = 2f;

    LayerMask buildMask;
    LayerMask editMask;

    List<Transform> shapesList;
    Transform editingShape;
    Color originalColour;
    Color flashingColour = Color.magenta;
    // Start is called before the first frame update
    void Start()
    {
        shapesList = new List<Transform>();

        buildMask = LayerMask.GetMask("Ground", "Obstacle");
        editMask = LayerMask.GetMask("Obstacle");
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.BUILDING:
                if (HasScrolled())
                {
                    OnScrollWheel();
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    SpawnShape();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    DeleteShape();
                }
                else if (Input.GetMouseButtonDown(2))
                {
                    SelectShape();
                }
                break;

            case State.EDITING:
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
                if (Input.GetKey(KeyCode.Equals))
                {
                    ModifyScale(1);
                }
                else if (Input.GetKey(KeyCode.Minus))
                {
                    ModifyScale(-1);
                }
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    state = State.BUILDING;
                }
                break;
            default:
                break;
        }
    }


    bool HasScrolled()
    {
        return Input.mouseScrollDelta.y != 0 && lastScrollTime + scrollCooldown < Time.time;
    }


    void OnScrollWheel()
    {
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
    }
    void SpawnShape()
    {
        Camera cameraView = GetComponentInChildren<Camera>();
        Ray ray = new Ray(cameraView.transform.position, cameraView.transform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 20f, buildMask))
        {
            Transform shape = Resources.Load<Transform>(allPrefabNames[selectedIndex]);
            Vector3 spawnPosition = new Vector3(hitInfo.point.x, hitInfo.point.y + (shape.localScale.y / 2), hitInfo.point.z);
            if (shape.CompareTag("Cylinder"))
            {
                spawnPosition.y += shape.localScale.y / 2;
            }
            Vector3 direction = spawnPosition - new Vector3(cameraView.transform.position.x, spawnPosition.y, cameraView.transform.position.z);
            Transform newCube = Instantiate(shape, spawnPosition, Quaternion.LookRotation(direction));
            shapesList.Add(newCube);
        }
    }


    void DeleteShape()
    {
        Camera cameraView = GetComponentInChildren<Camera>();
        Ray ray = new Ray(cameraView.transform.position, cameraView.transform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 20f, editMask))
        {
            Transform shapeToDelete = hitInfo.transform;
            shapesList.Remove(shapeToDelete);
            Destroy(shapeToDelete.gameObject);
        }
    }


    void SelectShape()
    {
        Camera cameraView = GetComponentInChildren<Camera>();
        Ray ray = new Ray(cameraView.transform.position, cameraView.transform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 20f, editMask))
        {
            editingShape = hitInfo.transform;
            originalColour = editingShape.GetComponent<Renderer>().material.color;
            state = State.EDITING;
            StartCoroutine(FlashSelectedShape());
        }
    }


    IEnumerator FlashSelectedShape()
    {
        Renderer renderer = editingShape.GetComponent<Renderer>();
        Material shapeMaterial = renderer.material;
        float timer = 0;
        while (state == State.EDITING)
        {
            shapeMaterial.color = Color.Lerp(originalColour, flashingColour, Mathf.PingPong(timer * flashesPerSecond * 2, 1));
            timer += Time.deltaTime;
            yield return null;
        }
        shapeMaterial.color = originalColour;
    }

    void RotateSelectedShape(Vector3 rotation)
    {
        editingShape.Rotate(rotateSpeed * Time.deltaTime * rotation);
    }


    void ModifyScale(int scale)
    {
        editingShape.localScale += scale * scaleSpeed * Time.deltaTime * Vector3.one;
    }
}