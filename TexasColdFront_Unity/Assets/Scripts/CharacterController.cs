using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tcf.controller
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed;
        private Animator anim;
        private SpriteRenderer sprite;
        private bool leftUp = true;
        private bool rightUp = true;
        private bool faceRight = true;

        public bool LeftUp { get => leftUp; }
        public bool RightUp { get => rightUp; }
        public float MovingSpeed { get => moveSpeed; }

        private void Start()
        {
            anim = GetComponent<Animator>();
            sprite = GetComponent<SpriteRenderer>();
        }

        private void FixedUpdate()
        {
            if(leftUp && rightUp)
            {
                anim.SetBool("isWalking", false);
            }
        }

        private void OnEnable()
        {
            GameInputSystem.KeyHeld_MoveLeft += OnKeyHeld_MoveLeft;
            GameInputSystem.KeyHeld_MoveRight += OnKeyHeld_MoveRight;
            GameInputSystem.KeyUp_MoveLeft += OnKeyUp_MoveLeft;
            GameInputSystem.KeyUp_MoveRight += OnKeyUp_MoveRight;
        }

        private void OnDisable()
        {
            GameInputSystem.KeyHeld_MoveLeft -= OnKeyHeld_MoveLeft;
            GameInputSystem.KeyHeld_MoveRight -= OnKeyHeld_MoveRight;
            GameInputSystem.KeyUp_MoveLeft -= OnKeyUp_MoveLeft;
            GameInputSystem.KeyUp_MoveRight -= OnKeyUp_MoveRight;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IPlayerInteractable other))
                GameStateMachine.Instance.CurrentInteractableInRange = other;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out IPlayerInteractable other) && GameStateMachine.Instance.CurrentInteractableInRange == other)
                GameStateMachine.Instance.CurrentInteractableInRange = null;
        }

        #region Movement
        private void OnKeyHeld_MoveLeft()
        {
            leftUp = false;
            if (faceRight)
            {
                faceRight = !faceRight;
                sprite.flipX = true;
            }
            anim.SetBool("isWalking", true);
            PerformMovement(new Vector2(-1, 0));
        }

        private void OnKeyHeld_MoveRight()
        {
            rightUp = false;
            if (!faceRight)
            {
                faceRight = !faceRight;
                sprite.flipX = false;
            }
            anim.SetBool("isWalking", true);
            PerformMovement(new Vector2(1, 0));
        }

        private void PerformMovement(Vector2 direction)
        {
            if (GameStateMachine.Instance.CurrentState == GameState.GAME)
                gameObject.transform.Translate(direction.x * moveSpeed * Time.deltaTime, 0, 0);
        }

        private void OnKeyUp_MoveLeft()
        {
            leftUp = true;
        }

        private void OnKeyUp_MoveRight()
        {
            rightUp = true;
        }
        #endregion

    }
}
