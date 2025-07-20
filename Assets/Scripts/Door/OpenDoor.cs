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
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate the index of the next scene
        int nextSceneIndex = currentSceneIndex + 1;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Load the next scene by index
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available in build settings.");
            // Optionally, load a default scene or loop back to the first scene
            SceneManager.LoadScene(0);
        }
    }
}