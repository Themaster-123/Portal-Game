using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputBehavior))]
[RequireComponent(typeof(MovementBehavior))]
[RequireComponent(typeof(DirectionBehavior))]
[RequireComponent(typeof(DirectionPortalableBehavior))]
[RequireComponent(typeof(InteractableBehavior))]
[RequireComponent(typeof(PortalPlacerBehavior))]
public class Player : MonoBehaviour
{
	protected InputBehavior inputBehavior;
	protected MovementBehavior movementBehavior;
	protected DirectionBehavior directionBehavior;

	protected virtual void Awake()
	{
		GetComponents();
	}

	protected virtual void Start()
	{
		RegisterInput();
	}

	protected virtual void Update()
	{
		SetMovement();
	}

	protected virtual void OnEnable()
	{
		inputBehavior.LockMouse(true);
	}

	protected virtual void OnDisable()
	{
		inputBehavior.LockMouse(false);
	}

	protected virtual void RegisterInput()
	{
		inputBehavior.inputMaster.Player.Jump.performed += context => movementBehavior.Jump();
	}

	// Sets the movement fields in movementBehavior
	protected virtual void SetMovement()
	{
		movementBehavior.Move(inputBehavior.PlayerMovement * Time.deltaTime);
		directionBehavior.Rotate(inputBehavior.MouseMovement);
	}

	protected virtual void GetComponents()
	{
		inputBehavior = GetComponent<InputBehavior>();
		movementBehavior = GetComponent<MovementBehavior>();
		directionBehavior = GetComponent<DirectionBehavior>();
	}
}
