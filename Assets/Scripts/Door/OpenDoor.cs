using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OpenDoor : MonoBehaviour
{
    [Header("UI Animation")]
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] UIAutoAnimation uiAnimation; // Reference to UIAutoAnimation component

    Animator animator;

    [Header("Scene Reload Settings")]
    [SerializeField] float delayBeforeReload = 2f; // Time to wait before reloading scene


    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Trigger UI animation before destroying player
            if (uiAnimation != null)
            {
                winText.gameObject.SetActive(true); // Show the win text
                uiAnimation.EntranceAnimation(); // This will trigger the entrance animation
                animator.SetTrigger("IsOpen");
                // Start the delayed reload coroutine
                StartCoroutine(DelayedSceneReload());
            }
        }
    }

    private IEnumerator DelayedSceneReload()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delayBeforeReload);

        // Reload the scene
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}