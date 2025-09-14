using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CharacterController2D : MonoBehaviour {
    [SerializeField] private float m_JumpForce = 400f;
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    [SerializeField] private bool m_AirControl = false;
    [SerializeField] private LayerMask m_WhatIsGround;
    [SerializeField] private Transform m_GroundCheck;
    [SerializeField] private Transform m_WallCheck;

    const float k_GroundedRadius = .2f;
    private bool m_Grounded;
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;
    private Vector3 velocity = Vector3.zero;
    private float limitFallSpeed = 25f;

    public bool canDoubleJump = true;
    [SerializeField] private float m_DashForce = 25f;
    private bool canDash = true;
    private bool isDashing = false;
    private bool m_IsWall = false;
    private bool isWallSliding = false;
    private bool oldWallSlidding = false;
    private bool canCheck = false;

    public float life = 10f;
    public bool invincible = false;
    private bool canMove = true;

    private Animator animator;
    public ParticleSystem particleJumpUp;
    public ParticleSystem particleJumpDown;
    private float jumpWallStartX = 0;
    private float jumpWallDistX = 0;
    private bool limitVelOnWallJump = false;

    private bool prevFallingNoDash = false;

    [Header("Events")]
    public UnityEvent OnFallEvent;
    public UnityEvent OnLandEvent;
    public UnityEvent OnFallingNoDash;

    void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        OnFallEvent ??= new UnityEvent();
        OnLandEvent ??= new UnityEvent();
    }

    void FixedUpdate() {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // Ground check
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++) {
            if (colliders[i].gameObject != gameObject) { m_Grounded = true; break; }
        }

        if (m_Grounded && !wasGrounded) {
            OnLandEvent.Invoke();
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsDoubleJumping", false);
            animator.SetBool("JumpUp", false);

            if (!m_IsWall && !isDashing) particleJumpDown.Play();
            canDoubleJump = true;
            if (m_Rigidbody2D.linearVelocity.y < 0f) limitVelOnWallJump = false;
        }

        if (!m_Grounded && wasGrounded) OnFallEvent.Invoke();

        m_IsWall = false;

        if (!m_Grounded) {
            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(m_WallCheck.position, k_GroundedRadius, m_WhatIsGround);
            for (int i = 0; i < collidersWall.Length; i++) {
                if (collidersWall[i].gameObject != gameObject) { isDashing = false; m_IsWall = true; break; }
            }
        }

        bool currentFallingNoDash = !m_Grounded && !isDashing;
        if (currentFallingNoDash && !prevFallingNoDash) OnFallingNoDash.Invoke();
        prevFallingNoDash = currentFallingNoDash;

        if (limitVelOnWallJump) {
            if (m_Rigidbody2D.linearVelocity.y < -0.5f) limitVelOnWallJump = false;
            jumpWallDistX = (jumpWallStartX - transform.position.x) * transform.localScale.x;

            if (jumpWallDistX < -0.5f && jumpWallDistX > -1f) canMove = true;
            else if (jumpWallDistX < -1f && jumpWallDistX >= -2f) { canMove = true; m_Rigidbody2D.linearVelocity = new Vector2(10f * transform.localScale.x, m_Rigidbody2D.linearVelocity.y); }
            else if (jumpWallDistX < -2f || jumpWallDistX > 0) { limitVelOnWallJump = false; m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y); }
        }
    }

    public void Move(float move, bool jump, bool dash) {
        if (!canMove) return;

        if (dash && canDash && !isWallSliding) StartCoroutine(DashCooldown());

        if (isDashing) {
            m_Rigidbody2D.linearVelocity = new Vector2(transform.localScale.x * m_DashForce, 0);
        }
        else if (m_Grounded || m_AirControl) {
            if (m_Rigidbody2D.linearVelocity.y < -limitFallSpeed)
                m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, -limitFallSpeed);

            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.linearVelocity.y);
            m_Rigidbody2D.linearVelocity = Vector3.SmoothDamp(m_Rigidbody2D.linearVelocity, targetVelocity, ref velocity, m_MovementSmoothing);

            if (move > 0 && !m_FacingRight && !isWallSliding) Flip();
            else if (move < 0 && m_FacingRight && !isWallSliding) Flip();
        }

        if (m_Grounded && jump) {
            animator.SetBool("IsJumping", true);
            animator.SetBool("JumpUp", true);
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            canDoubleJump = true;
            particleJumpDown.Play();
            particleJumpUp.Play();
        }
        else if (!m_Grounded && jump && canDoubleJump && !isWallSliding) {
            canDoubleJump = false;
            m_Rigidbody2D.linearVelocity = new Vector2(m_Rigidbody2D.linearVelocity.x, 0);
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce / 1.2f));
            animator.SetBool("IsDoubleJumping", true);
            animator.SetBool("JumpUp", false);
        }
        else if (m_IsWall && !m_Grounded) {
            if ((!oldWallSlidding && m_Rigidbody2D.linearVelocity.y < 0) || isDashing) {
                isWallSliding = true;
                m_WallCheck.localPosition = new Vector3(-m_WallCheck.localPosition.x, m_WallCheck.localPosition.y, 0);
                Flip();
                StartCoroutine(WaitToCheck(0.1f));
                canDoubleJump = true;
                animator.SetBool("IsWallSliding", true);
                ResetJumpAnimations();
            }
            isDashing = false;

            if (isWallSliding) {
                if (move * transform.localScale.x > 0.1f) StartCoroutine(WaitToEndSliding());
                else { oldWallSlidding = true; m_Rigidbody2D.linearVelocity = new Vector2(-transform.localScale.x * 2, -5); }
            }

            if (jump && isWallSliding) {
                animator.SetBool("IsJumping", true);
                animator.SetBool("JumpUp", true);
                m_Rigidbody2D.linearVelocity = Vector2.zero;
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
            else if (dash && canDash) {
                isWallSliding = false;
                animator.SetBool("IsWallSliding", false);
                oldWallSlidding = false;
                m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
                canDoubleJump = true;
                ResetJumpAnimations();
                StartCoroutine(DashCooldown());
            }
        }
        else if (isWallSliding && !m_IsWall && canCheck) {
            isWallSliding = false;
            animator.SetBool("IsWallSliding", false);
            oldWallSlidding = false;
            m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
            canDoubleJump = true;
        }
    }

    void Flip() {
        m_FacingRight = !m_FacingRight;
        var s = transform.localScale; s.x *= -1; transform.localScale = s;
    }

    public void ApplyDamage(float damage, Vector3 position) {
        if (invincible) return;

        animator.SetBool("Hit", true);
        life -= damage;

        Vector2 damageDir = (transform.position - position).normalized * 40f;
        m_Rigidbody2D.linearVelocity = Vector2.zero;
        m_Rigidbody2D.AddForce(damageDir * 10);

        if (life <= 0) StartCoroutine(WaitToDead());
        else { StartCoroutine(Stun(0.25f)); StartCoroutine(MakeInvincible(1f)); }
    }

    IEnumerator DashCooldown() {
        animator.SetBool("IsDashing", true);
        isDashing = true; canDash = false;
        yield return new WaitForSeconds(0.1f);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }

    IEnumerator Stun(float time) { canMove = false; yield return new WaitForSeconds(time); canMove = true; }
    IEnumerator MakeInvincible(float t) { invincible = true; yield return new WaitForSeconds(t); invincible = false; }

    IEnumerator WaitToCheck(float t) { canCheck = false; yield return new WaitForSeconds(t); canCheck = true; }

    IEnumerator WaitToEndSliding() {
        yield return new WaitForSeconds(0.1f);
        canDoubleJump = true;
        isWallSliding = false;
        animator.SetBool("IsWallSliding", false);
        oldWallSlidding = false;
        m_WallCheck.localPosition = new Vector3(Mathf.Abs(m_WallCheck.localPosition.x), m_WallCheck.localPosition.y, 0);
    }

    IEnumerator WaitToDead() {
        animator.SetBool("IsDead", true);
        canMove = false; invincible = true;
        GetComponent<Attack>().enabled = false;
        yield return new WaitForSeconds(0.4f);
        m_Rigidbody2D.linearVelocity = new Vector2(0, m_Rigidbody2D.linearVelocity.y);
        yield return new WaitForSeconds(1.1f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    void ResetJumpAnimations() {
        animator.SetBool("IsJumping", false);
        animator.SetBool("IsDoubleJumping", false);
        animator.SetBool("JumpUp", false);
    }

    public bool IsGrounded() => m_Grounded;
    public bool IsDashing() => isDashing;

    // Run-sound control hooks
    public void AddFallingNoDashListener(UnityAction a) => OnFallingNoDash.AddListener(a);
    public void RemoveFallingNoDashListener(UnityAction a) => OnFallingNoDash.RemoveListener(a);
}
