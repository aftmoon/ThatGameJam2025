using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    public Flowchart flowchart; // 你的Fungus Flowchart

    private Coroutine waitForClickCoroutine; // 用于存储协程引用

    public Shine[] shineGoods;

    public GameScenceSO scenceToGo;
    public ScenceLoadEventSO loadEventSO;

    public static DialogController Instance;

    public bool dialogEnd = false;

    public DayDataSO dayData;
    public int sumDay = 3;
    //public int currentDay = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // 初始化单例实例
        }
        else
        {
            Destroy(gameObject);  // 确保场景中只有一个实例
        }
    }

    //// 在对话框开始时调用
    //public void OnDialogueStarted()
    //{
    //    currentDialogueBox = GameObject.Find("Dialog"); // 默认命名为"Dialog"
    //    isDialogueActive = true;

    //    // 启动等待点击的协程
    //    if (waitForClickCoroutine != null)
    //    {
    //        StopCoroutine(waitForClickCoroutine);
    //    }
    //    waitForClickCoroutine = StartCoroutine(WaitForClick());
    //}

    //// 在对话框结束时调用
    //public void OnDialogueEnded()
    //{
    //    isDialogueActive = false;
    //    currentDialogueBox = null;

    //    // 停止等待点击
    //    if (waitForClickCoroutine != null)
    //    {
    //        StopCoroutine(waitForClickCoroutine);
    //    }
    //}

    //// 协程：等待点击
    //private IEnumerator WaitForClick()
    //{
    //    while (isDialogueActive)
    //    {
    //        if (Input.GetMouseButtonDown(0)) // 检查是否点击了鼠标左键
    //        {
    //            Vector2 mousePos = Input.mousePosition;

    //            if (currentDialogueBox != null)
    //            {
    //                RectTransform rectTransform = currentDialogueBox.GetComponent<RectTransform>();

    //                // 检查点击是否发生在对话框区域内
    //                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, null))
    //                {
    //                    // 如果点击在对话框区域内，执行下一步
    //                    flowchart.ExecuteBlock("NextStep"); // 执行Fungus中的下一步命令
    //                    yield break; // 点击后结束协程
    //                }
    //            }
    //        }

    //        yield return null; // 等待下一帧
    //    }
    //}

    public void CheckLevelCompletion()
    {
        bool isComplete = true;
        foreach (var isSelected in shineGoods)
        {
            if(!isSelected.HasBeenClicked)
            {
                isComplete = false;
                //Debug.Log(1);
                //break;
            }
        }
        if (flowchart.GetBooleanVariable("isEnd") == false)
        {
            isComplete = false;
        }
        if (isComplete)
        {
            Debug.Log("完成，下一个场景");
            dayData.currentDay++;
            if (dayData.currentDay <= sumDay)
                loadEventSO.RaiseLoadRequestEvent(scenceToGo, true);
            else
            {
                Debug.Log("循环结束");
            }
        }
    }
}
