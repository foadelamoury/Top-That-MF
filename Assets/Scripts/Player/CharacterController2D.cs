using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_WallCheck;                             //Posicion que controla si el personaje toca una pared

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 velocity = Vector3.zero;
    private float limitFallSpeed = 25f; // Limit fall speed

    public bool canDoubleJump = true; //If player can double jump
    [SerializeField] private float m_DashForce = 25f;
    private bool canDash = true;
    private bool isDashing = false; //If player is dashing
    private bool m_IsWall = false; //If there is a wall in front of the player
    private bool isWallSliding = false; //If player is sliding in a wall
    private bool oldWallSlidding = false; //If player is sliding in a wall in the previous frame
    private float prevVelocityX = 0f;
    private bool canCheck = false; //For check if player is wallsliding

    public float life = 10f; //Life of the player
    public bool invincible = false; //If player can die
    private bool canMove = true; //If player can move

    private Animator animator;
    public ParticleSystem particleJumpUp; //Trail particles
    public ParticleSystem particleJumpDown; //Explosion particles
    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0; //Distance between player and wall
    private bool limitVelOnWallJump = false; //For limit wall jump distance with low fps





    private bool prevFallingNoDash = false;

    [Header("Events")]
    [Space]

    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;
    public UnityEvent OnFallingNoDash; // Add this with your other events



    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (OnFallEvent == null)
            OnFallEvent = new UnityEvent();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
    }


    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;


        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        Debug.Log("colliders: " + colliders.Length); //Debug colliders
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                break; // Exit loop once we find ground
            }
        }


        // Handle landing - only when transitioning from not grounded to grounded
        if (m_Grounded && !wasGrounded)
        {
            OnLandEvent.Invoke();
            // Reset animation bools when landing
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsDoubleJumping", false);
            animator.SetBool("JumpUp", false);

            if (!m_IsWall && !isDashing)
                particleJumpDown.Play();
            canDoubleJump = true;
            if (m_Rigidbody2D.linearVelocity.y < 0f)
                limitVelOnWallJump = false;
        }

        // Handle falling - only when transitioning from grounded to not grounded
        if (!m_Grounded && wasGrounded)
        {
            OnFallEvent.Invoke();
        }


        m_IsWall = false;

        // Only check for walls when not grounded
        if (!m_Grounded)
        {
            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++)
            {
                if (collidersWall[i].gameObject != gameObject)
                {
                    isDashing = false;
                    m_IsWall = true;
                    break;
                }
            }
            prevVelocityX = m_Rigidbody2D.linearVelocity.x;
        }
        #region Pausing Movement sound when falling
        // Check for falling without dashing
        bool currentFallingNoDash = !m_Grounded && !isDashing;

        if (currentFallingNoDash && !prevFallingNoDash)
        {
            OnFallingNoDash.Invoke();
        }

        prevFallingNoDash = currentFallingNoDash; 
        #endregion

        if (limitVelOnWallJump)
        {
            if (m_Rigidbody2D.linearVelocity.y < -0.5f)
                limitVelOnWallJump = false;
            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;
            if (jumpWallDistX < -0.5f && jumpWallDistX > -1f)
            {
                canMove = true;
            }
            else if (jumpWallDistX < -1f && jumpWallDistX >= -2f)
            {
                canMove = true;
                m_Rigidbody2D.linearVelocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.linearVelocity.y);
            }
            else if (jumpWallDistX < -2f)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            }
            else if (jumpWallDistX > 0)
            {
                limitVelOnWallJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
            }
        }
    }



    public void Move(float move, bool jump, bool dash)
    {
        if (canMove)
        {
            if (dash && canDash && !isWallSliding)
            {
                StartCoroutine(DashCooldown());
            }
            // If crouching, check to see if the character can stand up
            if (isDashing)
            {
                m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
            }
            // If the player is on the ground or air control is turned on
            //only control the player if grounded or airControl is turned on
            else if (m_Grounded || m_AirControl)
            {
                if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                    m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);
                // Move the character by finding the target velocity
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
                // And then smoothing it out and applying it to the character
                m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

                // If the input is moving the player right and the player is facing left...
                if (move > 0 && !m_FacingRight && !isWallSliding)
                {
                    // ... flip the player.
                    Flip();
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (move < 0 && m_FacingRight && !isWallSliding)
                {
                    // ... flip the player.
                    Flip();
                }
            }
            // If the player should jump...
            if (m_Grounded && jump)
            {
                // Add a vertical force to the player.
                animator.SetBool("IsJumping", true);
                animator.SetBool("JumpUp", true);
                m_Grounded = false;
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
                canDoubleJump = true;
                particleJumpDown.Play();
                particleJumpUp.Play();
            }
            else if (!m_Grounded && jump && canDoubleJump && !isWallSliding)
            {
                canDoubleJump = false;
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
                m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));
                animator.SetBool("IsDoubleJumping", true);
                animator.SetBool("JumpUp", false); // Reset JumpUp when double jumping
            }

            else if (m_IsWall && !m_Grounded)
            {
                if (!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0 || isDashing)
                {
                    isWallSliding = true;

                    m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                    Flip();
                    StartCoroutine(WaitToCheck(0.1f));
                    canDoubleJump = true;
                    animator.SetBool("IsWallSliding", true);
                    ResetJumpAnimations();
                }
                isDashing = false;

                if (isWallSliding)
                {
                    if (move * transform.localScale.x > 0.1f)
                    {
                        StartCoroutine(WaitToEndSliding());
                    }
                    else
                    {
                        oldWallSlidding = true;
                        m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5);
                    }
                }

                if (jump && isWallSliding)
                {
                    animator.SetBool("IsJumping", true);
                    animator.SetBool("JumpUp", true);
                    m_Rigidbody2D.linearVelocity = new Vector2(0f, 0f);
                    m_Rigidbody2D.AddForce(new Vector2(transform.localScale.x * m_JumpForce * 1.2f, m_JumpForce));
                    jumpWallStartX = transform.position.x;
                    limitVelOnWallJump = true;
                    canDoubleJump = true;
                    isWallSliding = false;
                    animator.SetBool("IsWallSliding", false);
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canMove = false;
                }
                else if (dash && canDash)
                {
                    isWallSliding = false;
                    animator.SetBool("IsWallSliding", false);
                    oldWallSlidding = false;
                    m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                    canDoubleJump = true;
                    ResetJumpAnimations();
                    StartCoroutine(DashCooldown());
                }
            }
            else if (isWallSliding && !m_IsWall && canCheck)
            {
                isWallSliding = false;
                animator.SetBool("IsWallSliding", false);
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
            }
        }
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void ApplyDamage(float damage, Vector3 position)
    {
        if (!invincible)
        {
            animator.SetBool("Hit", true);
            life -= damage;
            Vector2 damageDir = Vector3.Normalize(transform.position - position) * 40f;
            m_Rigidbody2D.linearVelocity = Vector2.zero;
            m_Rigidbody2D.AddForce(damageDir * 10);
            if (life <= 0)
            {
                StartCoroutine(WaitToDead());
            }
            else
            {
                StartCoroutine(Stun(0.25f));
                StartCoroutine(MakeInvincible(1f));
            }
        }
    }

    IEnumerator DashCooldown()
    {
        animator.SetBool("IsDashing", true);
        isDashing = true;
        canDash = false;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }

    IEnumerator Stun(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    IEnumerator MakeInvincible(float time)
    {
        invincible = true;
        yield return new WaitForSeconds(time);
        invincible = false;
    }
    IEnumerator WaitToMove(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    IEnumerator WaitToCheck(float time)
    {
        canCheck = false;
        yield return new WaitForSeconds(time);
        canCheck = true;
    }

    IEnumerator WaitToEndSliding()
    {
        yield return new WaitForSeconds(0.1f);
        canDoubleJump = true;
        isWallSliding = false;
        animator.SetBool("IsWallSliding", false);
        oldWallSlidding = false;
        m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
    }

    IEnumerator WaitToDead()
    {
        animator.SetBool("IsDead", true);
        canMove = false;
        invincible = true;
        GetComponent<Attack>().enabled = false;
        yield return new WaitForSeconds(0.4f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetJumpAnimations()
    {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsDoubleJumping", false);
        animator.SetBool("JumpUp", false);
    }
    public bool IsGrounded() { return m_Grounded; }
    public bool IsDashing() { return isDashing; }

    #region To disable run sound

    public void AddFallingNoDashListener(UnityAction action)
    {
        OnFallingNoDash.AddListener(action);
    }

    public void RemoveFallingNoDashListener(UnityAction action)
    {
        OnFallingNoDash.RemoveListener(action);
    } 
    #endregion
}

   
