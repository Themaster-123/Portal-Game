using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(DirectionBehavior))]
public class MovementBehavior : Behavior
{
    [Header("Movement Settings")]
    [Tooltip("Clamps the velocity")]
    public float maxVelocity = 1024;
    public float maxSpeed = 8;
    public float acceleration = 80;
    public float maxAirSpeed = 8;
    public float airAcceleration = 10;
    public float maxSlopeAngle = 65;
    public float snapPredictionTime = .1f;

    [Header("Jump Settings")]
    public float jumpStrength = 3;
    public float groundedCheckDistance = 2.0f;

    // The velocity not accounting for speed or vertical velocity
    [HideInInspector]
    public Vector3 MovementVelocity 
    { 
        get
		{
            Vector3 velocity = rigidBody.velocity;
            velocity.y = 0;
            return velocity.normalized;
		} 
    }

    protected Rigidbody rigidBody;
    // tracks if entity is on ground
    protected bool onGround = false;
    protected uint stepsSinceLastGrounded = 0;
    protected uint stepsSinceLastJump = 0;
    protected Vector3 contactNormal;
    protected Vector3 combinedNormals;
    // used to move in FixedUpdate
    protected Vector2 physicsMovement;
    protected float jumpTime;
    protected bool jumped;
    protected DirectionBehavior directionBehavior;

    public bool IsGrounded()
    {
        /*return IsGrounded(out _);*/
        return onGround;
    }

    // changes physicsMovement to the sum of all movement
    public virtual void Move(Vector2 movement)
    {
        physicsMovement += movement;
    }

    public virtual void Move(Vector3 movement)
	{
        Move(new Vector2(movement.x, movement.z));
	}

    // makes the rigidbody jump with the force of jumpStrength
    public virtual void Jump()
    {
        if (IsGrounded())
        {
            jumped = true;
        }
    }

    // changes the rigidbody velocity to the target velocity(movement) then resets physicsMovement
    protected virtual void PhysicsMove()
    {
        physicsMovement = Vector2.ClampMagnitude(physicsMovement / Time.fixedDeltaTime, 1.0f);

        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        Vector3 direction = directionBehavior.GetHorizontalEntityRotation() * new Vector3(physicsMovement.x, 0, physicsMovement.y);

        Vector3 horizontalVel = new Vector3(Vector3.Dot(rigidBody.velocity, xAxis), 0, Vector3.Dot(rigidBody.velocity, zAxis));
        Vector3 targetVelocity = direction * (IsGrounded() ? maxSpeed : maxAirSpeed);
        Vector3 velocityDifference = Vector3.zero;
        if (IsGrounded())
		{
            velocityDifference = targetVelocity - horizontalVel;
        } else
		{
            velocityDifference = Vector3.ClampMagnitude((horizontalVel + targetVelocity), maxAirSpeed) - Vector3.ClampMagnitude(horizontalVel, maxAirSpeed);
		}

        Vector3 force = Vector3.ClampMagnitude(ProjectOnContactPlane(velocityDifference), (IsGrounded() ? acceleration : airAcceleration) * Time.fixedDeltaTime);

        rigidBody.AddForce(force, ForceMode.VelocityChange);
        physicsMovement = Vector2.zero;


    }

    protected virtual void OnEnable()
	{
        SetJumpTime();
    }

    protected override void Awake()
	{
        base.Awake();
	}

    protected virtual void FixedUpdate()
    {
        UpdateState();
        IncrementStepsSinceLastGrounded();
        SnapToGround();
        PhysicsMove();
        PhysicsJump();
        PreventSlipping();
        ClampVelocity();

        ClearState();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        CheckIfOnGround(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        CheckIfOnGround(collision);
    }

    protected virtual void SetGrounded(bool grounded, Vector3 normal)
    {
        onGround = grounded;
        contactNormal = normal.normalized;
    }

    protected virtual void SetGrounded(bool grounded)
    {
        SetGrounded(grounded, Vector3.up);
    }

    // checks if the ground slope is below or equal maxSlopeAngle and other stuff
    protected virtual void CheckIfOnGround(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (IsNormalBelowSlopeAngle(collision.contacts[i].normal))
            {
                combinedNormals += collision.contacts[i].normal;
            }
        }

        if (combinedNormals != Vector3.zero)
        {
            SetGrounded(true);
            return;
        }

        SetGrounded(false);
    }

    protected virtual void IncrementStepsSinceLastGrounded()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
    }

    protected bool PredictIfGroundUnderRigidbody(out RaycastHit hitPoint)
    {
        Vector3 p1 = transform.position + rigidBody.velocity * snapPredictionTime;

        return RaycastAtRigidbody(p1, -transform.up, groundedCheckDistance, out hitPoint);
    }
    
    protected bool RaycastAtRigidbody(Vector3 position, Vector3 direction, float distance, out RaycastHit hitPoint)
	{
        RaycastHit[] hits = Physics.RaycastAll(position, direction, distance, ~0, QueryTriggerInteraction.Ignore);

/*        hits = new List<RaycastHit>(hits).Where(delegate (RaycastHit hit)
        {
            return ((mask.value >> hit.transform.gameObject.layer) & 1) == 1 && hit.transform.gameObject != gameObject;
        }).ToArray();*/

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject != gameObject)
            {
                hitPoint = hit;
                return true;
            }
        }

        RaycastHit nullHitPoint = new RaycastHit();
        hitPoint = nullHitPoint;
        return false;
    }

    protected virtual void SnapToGround()
    {
        if (IsGrounded())
        {
            stepsSinceLastGrounded = 0;
            return;
        }

        if (stepsSinceLastGrounded == 1 && stepsSinceLastJump > 2)
        {
            bool groundUnder = PredictIfGroundUnderRigidbody(out RaycastHit hit);

            if (groundUnder && IsNormalBelowSlopeAngle(hit.normal))
            {
                SetGrounded(true, hit.normal);
                float speed = rigidBody.velocity.magnitude;
                float dot = Vector3.Dot(rigidBody.velocity, hit.normal);
                rigidBody.velocity = (rigidBody.velocity - hit.normal * dot).normalized * speed;
            }
        }
    }

    protected virtual bool IsNormalBelowSlopeAngle(Vector3 normal)
    {
        return Vector3.Angle(Vector3.up, normal) <= maxSlopeAngle;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return Vector3.ProjectOnPlane(vector, contactNormal);
    }

    protected void ClearState()
    {
        SetGrounded(false);
        combinedNormals = Vector3.zero;
    }

    protected void UpdateState()
    {
        SetGrounded(onGround, combinedNormals);
    }

    protected virtual void PhysicsJump()
    {
        if (jumped)
        {
            jumped = false;
            stepsSinceLastJump = 0;
            rigidBody.AddForce(transform.up * (jumpStrength - rigidBody.velocity.y), ForceMode.VelocityChange);
        }
    }

    protected virtual void SetJumpTime()
	{
        jumpTime = Time.time;
    }

    // Stops the rigidbody from slipping on a slope
    protected virtual void PreventSlipping()
	{
        if (!onGround) return;

        Vector3 slipDirection = (contactNormal - -Physics.gravity.normalized);
        Vector3 slipVelocity = slipDirection * Physics.gravity.magnitude;

        rigidBody.AddForce(-slipVelocity, ForceMode.Acceleration);
	}

    // Unused for now
    // If the rigidbody is out of bounds launch it back to the World.
    protected virtual void HandleOutOfBounds()
	{
		/*bool worldBelow = RaycastAtRigidbody(transform.position, -transform.up, Mathf.Infinity, groundLayers, out _);
		if (!worldBelow)
		{
			bool worldAbove = RaycastAtRigidbody(transform.position, transform.up, Mathf.Infinity, groundLayers, out RaycastHit aboveHit);
			if (worldAbove)
			{
				rigidBody.velocity = new Vector3(rigidBody.velocity.x, aboveHit.distance, rigidBody.velocity.z);
			}
		}*/
/*		bool worldBelow = RaycastAtRigidbody(transform.position, -transform.up, groundedCheckDistance, groundLayers, out _);
		if (!worldBelow && entity.world.GetVoxel(VoxelUtilities.ToVoxelPosition(transform.position + , entity.world)).value >= 0)
		{
			rigidBody.velocity = new Vector3(rigidBody.velocity.x, 10, rigidBody.velocity.z);
		}*/
	}

    protected virtual void ClampVelocity()
	{
        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxVelocity);
	}

    protected override void GetComponents()
    {
        rigidBody = GetComponent<Rigidbody>();
        directionBehavior = GetComponent<DirectionBehavior>();
    }
}
