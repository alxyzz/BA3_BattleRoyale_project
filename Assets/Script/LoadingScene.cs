using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image loadingBar;

    private void Start()
    {
        StartCoroutine(LoadSceneAsync(SteamLobby.SceneToLoad));
    }


    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        // operation.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.fillAmount = progressValue;
            yield return null;
        }

    }
}
