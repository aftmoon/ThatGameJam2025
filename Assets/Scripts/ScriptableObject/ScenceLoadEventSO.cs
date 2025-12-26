using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/ScenceLoadEventSO")]
public class ScenceLoadEventSO : ScriptableObject
{
    public UnityAction<GameScenceSO, bool> LoadRequestEvent;

    /// <summary>
    /// 场景加载请求
    /// </summary>
    /// <param name="locationToLoad">要加载的场景</param>
    /// <param name="fadeScreen">是否渐入渐出</param>
    public void RaiseLoadRequestEvent(GameScenceSO locationToLoad, bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(locationToLoad, fadeScreen);
    }
}
