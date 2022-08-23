using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIShapeEquipManager : MonoBehaviour
{
    Transform shapeDisplayed;
    float cubeSizeModifier = 1.1f;
    float sphereSizeModifier = 1.5f;
    float cylinderSizeModifier = 0.9f;
    float wallSizeModifier = 0.6f;
    // Start is called before the first frame update
    void Start()
    {
        // listens to the OnShapeChange object to ensure the ChangeShapeEquipped function is called when the event is invoked
        FindObjectOfType<BuildingController>().OnShapeChanged.AddListener(ChangeShapeEquipped);
    }

    // changes the shape displayed in the UI panel
    void ChangeShapeEquipped(Transform newShape)
    {
        Quaternion rotation;
        if (shapeDisplayed != null)
        {
            // rotation of the old shape saved before destroying it
            rotation = shapeDisplayed.localRotation;
            Destroy(shapeDisplayed.gameObject);
        }
        else
        {
            // if there is no previous object start with no rotation
            rotation = Quaternion.identity;
        }
        shapeDisplayed = Instantiate(newShape);

        // configuring shape so it is placed well and not visible to the player
        shapeDisplayed.localRotation = rotation;
        shapeDisplayed.parent = gameObject.transform;
        shapeDisplayed.localPosition = Vector3.forward;
        shapeDisplayed.gameObject.layer = LayerMask.NameToLayer("UI");

        // attach the rotate script so it spins
        shapeDisplayed.gameObject.AddComponent<RotateScript>();

        // remove (Clone) from the name for clearer switch-case
        shapeDisplayed.gameObject.name = shapeDisplayed.gameObject.name.Replace("(Clone)", "").Trim();

        // this big switch-case just sets the shape to a decent size in relation to the UI panel
        switch (shapeDisplayed.gameObject.name)
        {
            case "Cube":
                shapeDisplayed.localScale *= cubeSizeModifier;
                break;

            case "Sphere":
                shapeDisplayed.localScale *= sphereSizeModifier;
                break;

            case "Cylinder":
                shapeDisplayed.localScale *= cylinderSizeModifier;
                break;

            case "Wall":
                shapeDisplayed.localScale *= wallSizeModifier;
                break;

            default:
                break;
        }
    }
}
