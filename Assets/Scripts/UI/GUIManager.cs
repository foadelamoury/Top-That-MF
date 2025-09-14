using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour {
    public void RestartLevel() {
        var current = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync(current);
    }
}
