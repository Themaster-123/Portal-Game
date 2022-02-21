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

    public float smoothSpeed = 1;

    protected float smoothAmount = 1;
    protected Quaternion orginalRotation = Quaternion.identity;
    protected Quaternion targetRotation = Quaternion.identity;

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
        rotationTransform.localRotation =  Quaternion.Slerp(orginalRotation, targetRotation, smoothAmount) * (Quaternion.Inverse(targetRotation) * GetAxisRotation());
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

    public virtual void StartSmoothRotate(Quaternion rotation)
	{
        orginalRotation = rotation;
        targetRotation = GetAxisRotation();
        smoothAmount = 0;
	}

    public virtual Quaternion GetAxisRotation()
	{
        return Quaternion.Euler(new Vector3(rotateOnYAxis ? rotation.y : 0, rotateOnXAxis ? rotation.x : 0, 0));
    }

    protected virtual void Update()
    {
        StepSmoothCamera();
    }

    protected virtual void StepSmoothCamera()
	{
        smoothAmount += Time.deltaTime / smoothSpeed;
        CalculateRotation();
	}
}
