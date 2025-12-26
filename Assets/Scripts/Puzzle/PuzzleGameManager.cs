using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGameManager : MonoBehaviour
{
    public static PuzzleGameManager Instance;
    public Puzzle[] puzzlePieces;

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
            // 触发游戏完成逻辑
            Debug.Log("Puzzle Complete!");
        }
    }


}
