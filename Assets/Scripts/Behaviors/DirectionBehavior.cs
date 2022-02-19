using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionBehavior : Behavior
{
    public bool rotateOnXAxis = true;
    public bool rotateOnYAxis = false;
    public Transform rotationTransform;

    [HideInInspector]
    public Vector2 rotation;

    // rotates the entity using rotation parameter
    public virtual void Rotate(Vector2 rotation)
    {
        this.rotation.x += rotation.x;
        this.rotation.y = Mathf.Clamp(this.rotation.y - rotation.y, -90, 90);
        CalculateRotation();
    }

    // calculates rotations off of rotation variable
    public virtual void CalculateRotation()
    {
        rotationTransform.localRotation = Quaternion.Euler(new Vector3(rotateOnYAxis ? rotation.y : 0, rotateOnXAxis ? rotation.x : 0, 0));
    }

    // gets the Horzontal Direction based off of the rotation variable
    public virtual Vector3 HorizontalDirection()
    {
        return transform.TransformDirection(Quaternion.AngleAxis(rotation.x, Vector3.up) * Vector3.forward);
    }

    public virtual Vector3 GetDirection()
    {
        return transform.TransformDirection(GetEntityRotation() * Vector3.forward);
    }

    // gets the rotation based off of the rotation variable
    public virtual Quaternion GetHorizontalEntityRotation()
    {
        return transform.rotation * Quaternion.Euler(0, rotation.x, 0);
    }

    // gets the rotation based off of the rotation variable
    public virtual Quaternion GetEntityRotation()
    {
        return transform.rotation * Quaternion.Euler(rotation.y, rotation.x, 0);
    }
}
