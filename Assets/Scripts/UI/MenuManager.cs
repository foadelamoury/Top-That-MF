using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {
    [Header("Menu Objects")]
    [SerializeField] GameObject mainMenuCanvasGO;
    [SerializeField] GameObject settingsMenuCanvasGO;
    [SerializeField] GameObject controlsMenuCanvasGO;

    [Header("First Selected Options")]
    [SerializeField] GameObject mainMenuFirst;
    [SerializeField] GameObject settingsMenuFirst;

    void Start() {
        if (mainMenuCanvasGO) mainMenuCanvasGO.SetActive(true);
        if (settingsMenuCanvasGO) settingsMenuCanvasGO.SetActive(false);
        if (controlsMenuCanvasGO) controlsMenuCanvasGO.SetActive(false);

        if (mainMenuFirst) EventSystem.current?.SetSelectedGameObject(mainMenuFirst);
    }

    // ---- internal handlers ----
    void StartGame() => SceneManager.LoadSceneAsync(1);

    void OpenSettingsMenuHandle() {
        if (mainMenuCanvasGO) mainMenuCanvasGO.SetActive(false);
        if (controlsMenuCanvasGO) controlsMenuCanvasGO.SetActive(false);
        if (settingsMenuCanvasGO) settingsMenuCanvasGO.SetActive(true);

        if (settingsMenuFirst) EventSystem.current?.SetSelectedGameObject(settingsMenuFirst);
    }

    void OpenMainMenuHandle() {
        if (mainMenuCanvasGO) mainMenuCanvasGO.SetActive(true);
        if (controlsMenuCanvasGO) controlsMenuCanvasGO.SetActive(false);
        if (settingsMenuCanvasGO) settingsMenuCanvasGO.SetActive(false);

        if (mainMenuFirst) EventSystem.current?.SetSelectedGameObject(mainMenuFirst);
    }

    void OpenControlsMenuHandle() {
        if (mainMenuCanvasGO) mainMenuCanvasGO.SetActive(false);
        if (controlsMenuCanvasGO) controlsMenuCanvasGO.SetActive(true);
        if (settingsMenuCanvasGO) settingsMenuCanvasGO.SetActive(false);
    }

    // ---- UI events ----
    public void OnStartGame() => StartGame();
    public void OnSettingsPress() => OpenSettingsMenuHandle();
    public void OnSettingsBackPress() => OpenMainMenuHandle();
    public void OnControlsPress() => OpenControlsMenuHandle();
}
