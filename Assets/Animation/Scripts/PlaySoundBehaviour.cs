using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundBehaviour : StateMachineBehaviour
{
    private AudioSource audioSource;
    public AudioClip audioSound;
    public bool loop = false;

    // Reference to the character controller
    private CharacterController2D characterController;
    private bool wasGrounded = true;
    private bool isInState = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Get the character controller if we haven't already
        if (characterController == null)
        {
            characterController = animator.transform.GetComponent<CharacterController2D>();

            // If we found a character controller, subscribe to its events
            if (characterController != null)
            {
                // Store initial grounded state
                wasGrounded = characterController.IsGrounded();
            }
        }

        isInState = true;

        // Only play the sound if the character is grounded
        if (characterController != null && characterController.IsGrounded())
        {
            PlaySound(animator);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (characterController == null) return;

        bool isGrounded = characterController.IsGrounded();

        // Check if grounded status changed
        if (isGrounded != wasGrounded)
        {
            if (isGrounded)
            {
                // Just landed - play the sound
                PlaySound(animator);
            }
            else
            {
                // Just left ground - stop the sound
                StopSound();
            }

            wasGrounded = isGrounded;
        }

        // Additional check to ensure sound stops if we're no longer grounded
        if (audioSource != null && audioSource.isPlaying && !isGrounded)
        {
            StopSound();
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isInState = false;
        StopSound();
    }

    private void PlaySound(Animator animator)
    {
        if (audioSource == null)
        {
            audioSource = animator.transform.GetComponent<AudioSource>();
        }

        if (audioSource != null && audioSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = audioSound;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }

    private void StopSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
}