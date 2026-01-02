using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("事件监听")]
    public ScenceLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;

    public GameScenceSO firstLoadScene;
    public GameScenceSO menuScene;

    private GameScenceSO currentLoadScene;
    private GameScenceSO sceneToLoad;

    private AsyncOperationHandle<SceneInstance>? currentSceneHandle;

    private bool fadeScreen;

    public float fadeDuration;

    public DayDataSO dayData;

    private void Awake()
    {
        //Addressables.LoadSceneAsync(firstLoadScene.scenceReference, LoadSceneMode.Additive);
        currentLoadScene = menuScene;
        //currentLoadScene.scenceReference.LoadSceneAsync(LoadSceneMode.Additive);
        StartCoroutine(LoadSceneRoutine(currentLoadScene));
        dayData.currentDay = 0;

    }

    private void Start()
    {
        //loadEventSO.RaiseLoadRequestEvent(menuScene, true);
    }

    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
    }

    private void OnLoadRequestEvent(GameScenceSO locationToLoad, bool fadeScreen)
    {
        sceneToLoad = locationToLoad;
        this.fadeScreen = fadeScreen;
        if(currentLoadScene != null)
            StartCoroutine(SwitchSceneRoutine());
    }

    //private IEnumerator UnLoadPreviousScence()
    //{
    //    if (fadeScreen)
    //    {
    //        //TODO:渐入渐出
    //    }

    //    yield return new WaitForSeconds(fadeDuration);


    //    currentLoadScene.scenceReference.UnLoadScene();
    //    LoadNewScene();
    //}

    //private void LoadNewScene()
    //{
    //    sceneToLoad.scenceReference.LoadSceneAsync(LoadSceneMode.Additive, true);
    //}

    private IEnumerator SwitchSceneRoutine()
    {
        if (fadeScreen)
        {
            // TODO: 渐入渐出
        }

        yield return new WaitForSeconds(fadeDuration);

        if (currentSceneHandle.HasValue && currentSceneHandle.Value.IsValid())
        {
            yield return Addressables.UnloadSceneAsync(currentSceneHandle.Value);
            currentSceneHandle = null;
        }

        currentLoadScene = sceneToLoad;
        yield return LoadSceneRoutine(currentLoadScene);
    }

    private IEnumerator LoadSceneRoutine(GameScenceSO sceneSO)
    {
        var handle = sceneSO.scenceReference.LoadSceneAsync(
            LoadSceneMode.Additive,
            true
        );

        yield return handle;

        currentSceneHandle = handle;
    }

    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        loadEventSO.RaiseLoadRequestEvent(sceneToLoad, true);
    }
}
