using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Puzzle : MonoBehaviour,
        IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 correctPosition;  // 拼图块的正确位置
    private Vector2 originalPosition;  // 初始位置
    public bool IsInCorrectPosition { get; private set; }
    public float puzzleScale = 1;
    private Vector2 offset;
    private RectTransform rectTransform;
    private float snapDistance = 100f;
    private float snapSpeed = 10f;

    public Vector2 minBounds = new Vector2(368, -314);  // 区域的最小点（左下角）
    public Vector2 maxBounds = new Vector2(755, 323);  // 区域的最大点（右上角）

    private Canvas canvas;
    private Coroutine snapCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        float randomX = Random.Range(minBounds.x, maxBounds.x);
        float randomY = Random.Range(minBounds.y, maxBounds.y);

        rectTransform.anchoredPosition = new Vector2(randomX, randomY);
        rectTransform.localScale = Vector3.one * puzzleScale / 2;
        
        originalPosition = rectTransform.anchoredPosition;
        IsInCorrectPosition = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localMousePos
        );

        offset = rectTransform.anchoredPosition - localMousePos;
        rectTransform.localScale = Vector3.one * puzzleScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localMousePos
        );

        rectTransform.anchoredPosition = localMousePos + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Vector2.Distance(rectTransform.anchoredPosition, correctPosition) < snapDistance)
        {
            snapCoroutine = StartCoroutine(SnapToPosition(correctPosition));
            rectTransform.localScale = Vector3.one * puzzleScale;
            IsInCorrectPosition = true;
        }
        else
        {
            snapCoroutine = StartCoroutine(SnapToPosition(originalPosition));
            rectTransform.localScale = Vector3.one * puzzleScale / 2;
            IsInCorrectPosition = false;
        }
        PuzzleGameManager.Instance.CheckPuzzleCompletion();
    }

    // ================= 吸附动画 =================

    private IEnumerator SnapToPosition(Vector2 target)
    {
        while (Vector2.Distance(rectTransform.anchoredPosition, target) > 0.1f)
        {
            rectTransform.anchoredPosition =
                Vector2.Lerp(rectTransform.anchoredPosition, target, snapSpeed * Time.deltaTime);
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }
}

    //private void OnMouseDown()
    //{
    //    // 获取鼠标点击的世界坐标
    //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    rectTransform.localScale = new Vector3(0.25f, 0.25f, 1);

    //    // 计算拼图块与鼠标之间的偏移量
    //    offset = rectTransform.position - mousePos;
    //}

    //private void OnMouseDrag()
    //{
    //    rectTransform.localScale = new Vector3(0.5f, 0.5f, 1);
    //    // 获取当前鼠标在世界坐标系中的位置
    //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //    // 更新拼图块的位置，保持与鼠标的偏移量
    //    rectTransform.position = mousePos + offset;

    //    Debug.Log("Mouse Drag: " + rectTransform.position);
    //}
    //private void OnMouseUp()
    //{
    //    // 检查拼图块是否放置到正确的位置
    //    if (Vector3.Distance(rectTransform.position, correctPosition) < 0.5f)  // tolerance 为允许的误差
    //    {
    //        // 开始平滑过渡到正确位置
    //        StartCoroutine(SmoothMoveToCorrectPosition(correctPosition,5f));
    //        IsInCorrectPosition = true;
    //    }
    //    else
    //    {
    //        // 启动平滑过渡到原始位置
    //        StartCoroutine(SmoothMoveToCorrectPosition(originalPosition,10f));
    //        IsInCorrectPosition = false;
    //        rectTransform.localScale = new Vector3(0.25f, 0.25f, 1);
    //    }

    //    // 检查拼图是否完成
    //    PuzzleGameManager.Instance.CheckPuzzleCompletion();
    //}

    //private IEnumerator SmoothMoveToCorrectPosition(Vector3 targetPosition, float speed)
    //{
    //    float distanceToTarget = Vector3.Distance(rectTransform.position, targetPosition);
    //    while (distanceToTarget > 0.01f)
    //    {
    //        // 使用 MoveTowards 来平滑地移动拼图块
    //        rectTransform.position = Vector3.MoveTowards(rectTransform.position, targetPosition, speed * Time.deltaTime);
    //        distanceToTarget = Vector3.Distance(rectTransform.position, targetPosition);
    //        yield return null;
    //    }

    //    // 确保完全到达目标位置
    //    rectTransform.position = targetPosition;
    //}



