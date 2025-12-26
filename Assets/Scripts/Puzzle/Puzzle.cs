using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Puzzle : MonoBehaviour
{
    public Vector3 correctPosition;  // 拼图块的正确位置
    private Vector3 originalPosition;  // 初始位置
    public bool IsInCorrectPosition { get; private set; }
    private Vector3 offset;

    public Vector2 minBounds = new Vector2(2.54f, -2.24f);  // 区域的最小点（左下角）
    public Vector2 maxBounds = new Vector2(5.29f, 2.23f);  // 区域的最大点（右上角）

    private void Start()
    {
        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);

        transform.localScale = new Vector3(0.2f, 0.2f, 1);

        transform.position = new Vector3(randomX, randomY, transform.position.z);
        originalPosition = transform.position;
        IsInCorrectPosition = false;
    }


    private void OnMouseDown()
    {
        // 获取鼠标点击的世界坐标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.localScale = new Vector3(0.2f, 0.2f, 1);

        // 计算拼图块与鼠标之间的偏移量
        offset = transform.position - mousePos;
    }

    private void OnMouseDrag()
    {
        transform.localScale = new Vector3(0.4f, 0.4f, 1);
        // 获取当前鼠标在世界坐标系中的位置
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 更新拼图块的位置，保持与鼠标的偏移量
        transform.position = mousePos + offset;

        //Debug.Log("Mouse Drag: " + transform.position);
    }
    private void OnMouseUp()
    {
        // 检查拼图块是否放置到正确的位置
        if (Vector3.Distance(transform.position, correctPosition) < 0.5f)  // tolerance 为允许的误差
        {
            // 开始平滑过渡到正确位置
            StartCoroutine(SmoothMoveToCorrectPosition(correctPosition,5f));
            IsInCorrectPosition = true;
        }
        else
        {
            // 启动平滑过渡到原始位置
            StartCoroutine(SmoothMoveToCorrectPosition(originalPosition,10f));
            IsInCorrectPosition = false;
            transform.localScale = new Vector3(0.2f, 0.2f, 1);
        }

        // 检查拼图是否完成
        PuzzleGameManager.Instance.CheckPuzzleCompletion();
    }

    private IEnumerator SmoothMoveToCorrectPosition(Vector3 targetPosition, float speed)
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        while (distanceToTarget > 0.01f)
        {
            // 使用 MoveTowards 来平滑地移动拼图块
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            yield return null;
        }

        // 确保完全到达目标位置
        transform.position = targetPosition;
    }


}
