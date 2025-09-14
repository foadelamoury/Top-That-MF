using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

   [SerializeField] CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool dash = false;

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
            Debug.Log("Jump pressed");
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            dash = true;
        }
    }

    public void OnFall()
    {
        // This is called when the player starts falling (transitions from grounded to not grounded)
        // Don't set IsJumping here - it should only be set when actually jumping
        Debug.Log("Player is falling");
    }

    public void OnLanding()
    {
        // This is called when the player lands (transitions from not grounded to grounded)
        // Animation resets are handled in CharacterController2D
        Debug.Log("Player landed");
    }

    void FixedUpdate()
    {
        // Move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        jump = false;
        dash = false;
    }
}