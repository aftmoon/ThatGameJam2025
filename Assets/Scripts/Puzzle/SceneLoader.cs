using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("事件监听")]
    public ScenceLoadEventSO loadEventSO;
    public GameScenceSO firstLoadScene;

    private GameScenceSO currentLoadScene;
    private GameScenceSO sceneToLoad;
    private bool fadeScreen;

    public float fadeDuration;

    private void Awake()
    {
        //Addressables.LoadSceneAsync(firstLoadScene.scenceReference, LoadSceneMode.Additive);
        currentLoadScene = firstLoadScene;
        currentLoadScene.scenceReference.LoadSceneAsync(LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
    }

    private void OnLoadRequestEvent(GameScenceSO locationToLoad, bool fadeScreen)
    {
        sceneToLoad = locationToLoad;
        this.fadeScreen = fadeScreen;
        if(currentLoadScene != null)
            StartCoroutine(UnLoadPreviousScence());
    }

    private IEnumerator UnLoadPreviousScence()
    {
        if (fadeScreen)
        {
            //TODO:渐入渐出
        }

        yield return new WaitForSeconds(fadeDuration);


        currentLoadScene.scenceReference.UnLoadScene();
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        sceneToLoad.scenceReference.LoadSceneAsync(LoadSceneMode.Additive, true);
    }
}
