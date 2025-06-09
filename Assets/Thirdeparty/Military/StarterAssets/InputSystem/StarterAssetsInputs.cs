using UnityEngine;
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
        public bool sprint;
        public bool interact; // Действие: подбор/отпускание
        public bool use; // Действие: использовать (крышечка, дверь)

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;
        public float mouseScroll; // Ввод колеса мыши
        public bool push;

#if ENABLE_INPUT_SYSTEM
        
        public void OnScroll(InputValue value)
        {
            mouseScroll = value.Get<Vector2>().y; // Получаем значение прокрутки (y — вертикальная ось)
        }
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
        
        public void OnPush(InputValue value)
        {
            push = value.isPressed;
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnUse(InputValue value)
        {
            UseInput(value.isPressed);
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

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void UseInput(bool newUseState)
        {
            use = newUseState;
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