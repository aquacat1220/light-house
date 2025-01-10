using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterMovement : MonoBehaviour
{
    // Maximum movement speed of this character.
    public float maxSpeed;


    // Reference to the character's Rigidbody2D.
    public Rigidbody2D rigidBody;

    // Reference to InputAction for character movement.
    private InputAction moveAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rigidBody is null) {
            Debug.Log("\"rigidBody\" wasn't set.");
            throw new Exception();
        }
        moveAction = InputSystem.actions.FindAction("Move");
        if (moveAction is null) {
            Debug.Log("\"Move\" action wasn't found.");
            throw new Exception();
        }
    }

    
    void FixedUpdate() {
        Vector2 moveDirection = moveAction.ReadValue<Vector2>().normalized;
        rigidBody.linearVelocity = moveDirection * maxSpeed;

        Camera mainCam = Camera.main;
        Vector3 screenPosition = new Vector3(Pointer.current.position.x.ReadValue(), Pointer.current.position.y.ReadValue(), -mainCam.transform.position.z);
        Vector3 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
        Vector3 viewDirection = worldPosition - transform.position;
        float rotation = Mathf.Atan2(viewDirection.y, viewDirection.x) * Mathf.Rad2Deg - 90;
        rigidBody.rotation = rotation;
    }
}
