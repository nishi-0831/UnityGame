using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool prevJump;
		public bool sprint;
		public bool releaseJumpBtn;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

		public Action onReleaseJumpBtn;
        private void Update()
        {
			
        }
#if ENABLE_INPUT_SYSTEM
        //public void OnMove(InputValue value)
        public void OnMove(InputAction.CallbackContext context)
		{
			MoveInput(context.ReadValue<Vector2>());
		}

		//public void OnLook(InputValue value)
		public void OnLook(InputAction.CallbackContext context)
        {
			if(cursorInputForLook)
			{
				LookInput(context.ReadValue<Vector2>());
			}
		}

		public void OnJump(InputAction.CallbackContext context)
		{
			//É{É^ÉìÇ™âüÇ≥ÇÍÇΩèuä‘
			if(context.started)
			{
				JumpInput(true);
				releaseJumpBtn = false;
            }

            //ó£Ç≥ÇÍÇΩèuä‘
            if (context.canceled)
			{
				onReleaseJumpBtn?.Invoke();
            }

        }

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif
		

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;

        }
		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}