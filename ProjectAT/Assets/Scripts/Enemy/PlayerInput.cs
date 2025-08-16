using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MomDra.Input
{
    public class PlayerInput : MonoBehaviour
    {
        private Vector2 move;
        public Vector2 Move => move;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }
#endif

        private void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }
    }
}
