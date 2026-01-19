using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RopeWrapper
{
    [RequireComponent(typeof(Rigidbody2D))]

    public class PlayerMovementController : MonoBehaviour
    {
        private Rigidbody2D playerRgbd;
        private SpriteRenderer sprite;

        [SerializeField]
        private float movementSpeed;

        private void Start()
        {
            playerRgbd = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            playerRgbd.linearVelocity = Vector2.ClampMagnitude(playerRgbd.linearVelocity, movementSpeed);

            if (Input.GetKey(KeyCode.W))
                playerRgbd.linearVelocity = new Vector2(playerRgbd.linearVelocity.x, movementSpeed);
            if (Input.GetKey(KeyCode.S))
                playerRgbd.linearVelocity = new Vector2(playerRgbd.linearVelocity.x, -movementSpeed);
            if (Input.GetKey(KeyCode.D))
            {
                playerRgbd.linearVelocity = new Vector2(movementSpeed, playerRgbd.linearVelocity.y);
                sprite.flipX = false;
            }
            if (Input.GetKey(KeyCode.A))
            {
                playerRgbd.linearVelocity = new Vector2(-movementSpeed, playerRgbd.linearVelocity.y);
                sprite.flipX = true;
            }
        }
    }
}