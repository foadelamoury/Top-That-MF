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
        collision.gameObject.SetActive(false);
        StartCoroutine(DelayedSceneReload());
    }

    IEnumerator DelayedSceneReload() 
    {
        yield return new WaitForSeconds(delayBeforeReload);

        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        if (next < SceneManager.sceneCountInBuildSettings) 
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(next);
            string currentSceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            SceneManager.LoadSceneAsync(next);
            
            if (next != 5) SceneManager.LoadScene("Timing", LoadSceneMode.Additive);
        }
        else
            SceneManager.LoadSceneAsync(0);
    }

  

}
