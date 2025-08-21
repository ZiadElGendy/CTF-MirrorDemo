using Mirror;
using UnityEngine;

namespace CTF
{


    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Settings")] [SerializeField] public float speed = 5f;

        [Header("References")] [SerializeField]
        private CharacterController _controller;

        private float _gravity = -9.81f;
        private Vector3 _velocity;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            if (!isLocalPlayer) return;

            // Get input for move   ment
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            // Calculate movement direction
            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            // Move the character
            _controller.Move(move * speed * Time.deltaTime);

            // Apply gravity
            _velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);

            // Reset velocity when grounded
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
        }
    }
}