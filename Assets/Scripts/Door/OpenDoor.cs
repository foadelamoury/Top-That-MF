using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OpenDoor : MonoBehaviour {
    [Header("UI Animation")]
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] UIAutoAnimation uiAnimation;

    [Header("Scene Reload Settings")]
    [SerializeField] float delayBeforeReload = 2f;

    Animator animator;

    void Start() => animator = GetComponent<Animator>();

    void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (uiAnimation != null) {
            winText.gameObject.SetActive(true);
            uiAnimation.EntranceAnimation();
        }
        animator.SetTrigger("IsOpen");
        StartCoroutine(DelayedSceneReload());
    }

    IEnumerator DelayedSceneReload() {
        yield return new WaitForSeconds(delayBeforeReload);

        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(0);
    }
}
