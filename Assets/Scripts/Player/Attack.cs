using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour {
    public float dmgValue = 4;
    public GameObject throwableObject;
    public Transform attackCheck;
    public Animator animator;
    public bool canAttack = true;

    public GameObject cam;

    Rigidbody2D m_Rigidbody2D;

    void Awake() => m_Rigidbody2D = GetComponent<Rigidbody2D>();

    void Update() {
        if (Input.GetKeyDown(KeyCode.X) && canAttack) {
            canAttack = false;
            animator.SetBool("IsAttacking", true);
            StartCoroutine(AttackCooldown());
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            var pos = transform.position + new Vector3(transform.localScale.x * 0.5f, -0.2f);
            GameObject throwableWeapon = Instantiate(throwableObject, pos, Quaternion.identity);
            Vector2 direction = new Vector2(transform.localScale.x, 0);
            throwableWeapon.GetComponent<ThrowableWeapon>().direction = direction;
            throwableWeapon.name = "ThrowableWeapon";
        }
    }

    IEnumerator AttackCooldown() {
        yield return new WaitForSeconds(0.25f);
        canAttack = true;
    }

    public void DoDashDamage() {
        float val = Mathf.Abs(dmgValue);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCheck.position, 0.9f);
        foreach (var c in hits) {
            if (c.CompareTag("Enemy")) {
                float final = (c.transform.position.x - transform.position.x < 0) ? -val : val;
                c.gameObject.SendMessage("ApplyDamage", final);
                if (cam) cam.GetComponent<CameraFollow>().ShakeCamera();
            }
        }
    }
}
