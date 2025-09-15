using UnityEngine;
using UnityEngine.SceneManagement;

public class KillZone : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D col) {
        if (col.CompareTag("Player")) {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
            SceneManager.LoadScene("Timing", LoadSceneMode.Additive);

        }
        else {
            Destroy(col.gameObject);
        }
    }
}
