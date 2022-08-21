using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour
{
    string prefabName = "Cube";
    public LayerMask mask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("cLICK");
            Camera cameraView = GetComponentInChildren<Camera>();
            Ray ray = new Ray(cameraView.transform.position, cameraView.transform.forward);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, mask))
            {
                Transform shape = Resources.Load<Transform>(prefabName);
                Vector3 spawnPosition = new Vector3(hitInfo.point.x, hitInfo.point.y + (shape.localPosition.y / 2), hitInfo.point.z);                Transform newCube = Instantiate(shape, spawnPosition, Quaternion.identity) as Transform;
            }
        }
    }
}
