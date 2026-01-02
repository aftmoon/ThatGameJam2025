using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleGameManager : MonoBehaviour
{
    public int currentLevel = 1;
    public int totalLevels = 2;

    public static PuzzleGameManager Instance;
    public Puzzle[] puzzlePieces;

    public GameScenceSO scenceToGo;
    public GameScenceSO daytimeScene;
    public ScenceLoadEventSO loadEventSO;

    //public int sumDay = 3;
    //private int currentDay = 0;

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

    public void CheckPuzzleCompletion() 
    {
        bool isComplete = true;
        foreach (var piece in puzzlePieces)
        {
            if (!piece.IsInCorrectPosition)
            {
                isComplete = false;
                break;
            }
        }

        if (isComplete)
        {
            if (currentLevel < totalLevels)
            {
                // 加载下一关
                currentLevel++;
                Debug.Log("过关");
                loadEventSO.RaiseLoadRequestEvent(scenceToGo, true);
            }
            else
            {
                // 游戏完成，显示结算页面等
                Debug.Log("游戏已完成，恭喜！");
                loadEventSO.RaiseLoadRequestEvent(daytimeScene, true);
                // 这里可以显示结算界面等
                //currentDay++;
                //if (currentDay <= sumDay)
                //    loadEventSO.RaiseLoadRequestEvent(scenceToGo, true);
                //else
                //    loadEventSO.RaiseLoadRequestEvent(backToDayWorkScene);
            }
            
        }
    }

    // 加载指定的关卡场景
    public void LoadLevel(int level)
    {
        string sceneName = "Level" + level.ToString();  // 假设场景名称为 Level1, Level2 等
        SceneManager.LoadScene(sceneName);
    }
}
