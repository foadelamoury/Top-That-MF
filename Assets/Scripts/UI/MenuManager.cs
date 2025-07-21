using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    [Header("Menu Objects")]
    [SerializeField] GameObject mainMenuCanvasGO;
    [SerializeField] GameObject settingsMenuCanvasGO;
    [SerializeField] GameObject controlsMenuCanvasGO;


    [Header("First Selected Options")]
    [SerializeField] GameObject mainMenuFirst;
    [SerializeField] GameObject settingsMenuFirst;


    private bool isPaused;
    private void Start()
    {
        mainMenuCanvasGO.SetActive(true);
        settingsMenuCanvasGO.SetActive(false);
        controlsMenuCanvasGO.SetActive(false);

    }
    private void Update()
    {

    }

    private void StartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    private void OpenSettingsMenuHandle()
    {
        mainMenuCanvasGO.SetActive(false);
        controlsMenuCanvasGO.SetActive(false);
        settingsMenuCanvasGO.SetActive(true);

    }
    private void OpenMainMenuHandle()
    {
        mainMenuCanvasGO.SetActive(true);
        controlsMenuCanvasGO.SetActive(false);

        settingsMenuCanvasGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(settingsMenuFirst);
    }

    private void OpenControlsMenuHandle()
    {
        mainMenuCanvasGO.SetActive(false);
        controlsMenuCanvasGO.SetActive(true);

        settingsMenuCanvasGO.SetActive(false);
    }


    public void OnStartGame()
    {
        StartGame();
    }

    public void OnSettingsPress()
    {
        OpenSettingsMenuHandle();
    }

    public void OnSettingsBackPress()
    {
        OpenMainMenuHandle();
    }

    public void OnControlsPress()
    {
        OpenControlsMenuHandle();
    }

 
}