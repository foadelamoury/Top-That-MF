using UnityEngine;

public class Grass : MonoBehaviour {
    public ParticleSystem leafParticle;

    void OnTriggerEnter2D(Collider2D col) {
        var anim = GetComponent<Animator>();
        if (!anim) return;

        if (transform.position.x - col.transform.position.x > 0)
            anim.Play("MovingGrassL");
        else
            anim.Play("MovingGrassR");
    }

    public void ApplyDamage(float damage) {
        Instantiate(leafParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
