using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBehavior : Behavior
{
    public InputMaster inputMaster;
    public float mouseSensitivity = .1f;

    public virtual Vector2 MousePosition
	{
        get
		{
            return inputMaster.Player.UIMousePosition.ReadValue<Vector2>();
		}
	}

    public virtual Vector2 PlayerMovement
    {
        get
		{
            return inputMaster.Player.Movement.ReadValue<Vector2>();

        }
    }

    public virtual Vector2 MouseMovement
    {
        get
		{
         return inputMaster.Player.MouseMovement.ReadValue<Vector2>() * mouseSensitivity;
		}
    }

    public virtual void LockMouse(bool lockMouse)
	{
        Cursor.lockState = lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockMouse;
	}

    protected override void Awake()
    {
        base.Awake();
        inputMaster = new InputMaster();
    }

    protected virtual void OnEnable()
    {
        EnableInputMaster();
    }

    protected virtual void OnDisable()
    {
        DisableInputMaster();
    }

    protected virtual void InitializeInputMaster()
	{
        inputMaster = new InputMaster();
    }

    protected virtual void EnableInputMaster()
	{
        inputMaster.Enable();
    }

    protected virtual void DisableInputMaster()
    {
        inputMaster.Enable();
    }
}
