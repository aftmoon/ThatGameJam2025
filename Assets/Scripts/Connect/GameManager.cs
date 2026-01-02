using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LayerMask nodeLayer;
    public Transform nodesRoot;
    public Node[] allNodes;
    public Node selectedNode;
    public GameObject linePrefab;
    private LineRenderer previewLine;//跟随鼠标走
    private GameObject previewLineObj;

    private bool isDragging;

    // 所有连线（用于检测断线）
    private List<NodeConnection> allConnections = new List<NodeConnection>();
    //交叉检测
    public float lineIntersectThreshold = 0.3f;


    // 鼠标当前指向的连线
    private NodeConnection hoveredConnection;

    // 鼠标距离线多近算点中
    public float disconnectThreshold = 0.15f;

    [Header("场景转换")]
    public GameScenceSO scenceToGo;
    public ScenceLoadEventSO loadEventSO;
    bool hasFinished = false;


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
        allNodes = nodesRoot.GetComponentsInChildren<Node>();
    }

    void Update()
    {
        HandleMouseInput();

        DetectConnectionUnderMouse();

        if (isDragging && previewLine != null && selectedNode != null)
        {
            UpdatePreviewLine();
        }
        CheckConnectCompletion();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            TryEndDrag();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryDisconnect();
        }
    }

    void TryStartDrag()
    {
        Node node = GetNodeUnderMouse();

        if (node == null || !node.CanConnect())
            return;

        selectedNode = node;
        isDragging = true;

        CreatePreviewLine(node);
    }

    void TryEndDrag()
    {
        if (!isDragging)
            return;

        Node endNode = GetNodeUnderMouse();

        if (endNode != null && endNode != selectedNode)
        {
            TryCreateConnection(selectedNode, endNode);
        }


        ClearPreviewLine();
        selectedNode = null;
        isDragging = false;
    }

    void TryDisconnect()
    {
        if (hoveredConnection != null)
        {
            RemoveConnection(hoveredConnection);
            hoveredConnection = null;
        }
    }


    Node GetNodeUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, nodeLayer))
        {
            return hit.collider.GetComponent<Node>();
        }

        return null;
    }


    void CreatePreviewLine(Node node)
    {
        previewLineObj = Instantiate(linePrefab);
        previewLine = previewLineObj.GetComponent<LineRenderer>();


        previewLine.positionCount = 2;
        previewLine.enabled = true;
    }

    void ClearPreviewLine()
    {
        if (previewLineObj != null)
            Destroy(previewLineObj);

        previewLine = null;
        previewLineObj = null;
    }

    void UpdatePreviewLine()
    {
        previewLine.SetPosition(0, selectedNode.transform.position);
        previewLine.SetPosition(1, GetMouseWorldPos());
    }



    void TryCreateConnection(Node a, Node b)
    {
        // 1. 基础校验
        if (!a.CanConnect() || !b.CanConnect()) return;

        // 2. 不能重复连接
        if (IsAlreadyConnected(a, b)) return;

        // 3. 不能交叉
        if (WillIntersectExistingConnections(a, b))
            return;

        // 4. 创建线
        GameObject lineObj = Instantiate(linePrefab);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, a.transform.position);
        lr.SetPosition(1, b.transform.position);

        // 5. 数据绑定
        NodeConnection c = new NodeConnection
        {
            nodeA = a,
            nodeB = b,
            lineRenderer = lr
        };

        a.connections.Add(c);
        b.connections.Add(c);

        a.currentConnections++;
        b.currentConnections++;

        allConnections.Add(c);
    }

    bool IsAlreadyConnected(Node a, Node b)
    {
        foreach (var c in a.connections)
        {
            if (c.nodeA == b || c.nodeB == b)
                return true;
        }
        return false;
    }

    bool WillIntersectExistingConnections(Node a, Node b)
    {
        Vector3 p1 = a.transform.position;
        Vector3 p2 = b.transform.position;

        foreach (var c in allConnections)
        {
            //共享端点的线不算交叉
            if (c.nodeA == a || c.nodeA == b ||
                c.nodeB == a || c.nodeB == b)
                continue;

            Vector3 q1 = c.nodeA.transform.position;
            Vector3 q2 = c.nodeB.transform.position;

            float dist = DistanceBetweenSegments(p1, p2, q1, q2);

            if (dist < lineIntersectThreshold)
            {
                return true; // 发生交叉
            }
        }

        return false;
    }

    float DistanceBetweenSegments(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
    {
        Vector3 d1 = p2 - p1;
        Vector3 d2 = q2 - q1;
        Vector3 r = p1 - q1;

        float a = Vector3.Dot(d1, d1);
        float e = Vector3.Dot(d2, d2);
        float f = Vector3.Dot(d2, r);

        float s, t;

        if (a <= Mathf.Epsilon && e <= Mathf.Epsilon)
        {
            return Vector3.Distance(p1, q1);
        }

        if (a <= Mathf.Epsilon)
        {
            s = 0;
            t = Mathf.Clamp01(f / e);
        }
        else
        {
            float c = Vector3.Dot(d1, r);
            if (e <= Mathf.Epsilon)
            {
                t = 0;
                s = Mathf.Clamp01(-c / a);
            }
            else
            {
                float b = Vector3.Dot(d1, d2);
                float denom = a * e - b * b;

                s = denom != 0 ? Mathf.Clamp01((b * f - c * e) / denom) : 0;
                t = (b * s + f) / e;

                if (t < 0)
                {
                    t = 0;
                    s = Mathf.Clamp01(-c / a);
                }
                else if (t > 1)
                {
                    t = 1;
                    s = Mathf.Clamp01((b - c) / a);
                }
            }
        }

        Vector3 cp1 = p1 + d1 * s;
        Vector3 cp2 = q1 + d2 * t;

        return Vector3.Distance(cp1, cp2);
    }



    Vector3 GetMouseWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }

        // 如果什么都没打到，就给一个射线方向上的点
        return ray.origin + ray.direction * 10f;
    }

    void DetectConnectionUnderMouse()
    {
        hoveredConnection = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        foreach (var c in allConnections)
        {
            Vector3 a = c.nodeA.transform.position;
            Vector3 b = c.nodeB.transform.position;

            if (DistanceRayToSegment(ray, a, b) < disconnectThreshold)
            {
                hoveredConnection = c;
                break;
            }
        }
    }

    float DistanceRayToSegment(Ray ray, Vector3 a, Vector3 b)
    {
        Vector3 rayOrigin = ray.origin;
        Vector3 rayDir = ray.direction;

        Vector3 segDir = b - a;
        Vector3 cross = Vector3.Cross(rayDir, segDir);

        float denom = cross.magnitude;
        if (denom < 0.0001f)
            return float.MaxValue;

        float distance = Mathf.Abs(Vector3.Dot((a - rayOrigin), cross.normalized));
        return distance;
    }

    void RemoveConnection(NodeConnection c)
    {
        c.nodeA.connections.Remove(c);
        c.nodeB.connections.Remove(c);

        c.nodeA.currentConnections--;
        c.nodeB.currentConnections--;

        Destroy(c.lineRenderer.gameObject);

        allConnections.Remove(c);
    }

    public void CheckConnectCompletion()
    {
        bool isComplete = true;
        foreach (var node in allNodes)
        {
            if (node.currentConnections != node.maxConnections)
            {
                isComplete = false;
                break;
            }
        }
        if (isComplete && !hasFinished)
        {
            hasFinished = true;
            Debug.Log("连线已完成");
            ClearAllConnections();
            loadEventSO.RaiseLoadRequestEvent(scenceToGo, true);
        }
    }
    public void ClearAllConnections()
    {
        // 1. 删除所有连线 GameObject
        foreach (var c in allConnections)
        {
            if (c.lineRenderer != null)
            {
                Destroy(c.lineRenderer.gameObject);
            }
        }

        // 2. 清空每个 Node 的数据
        Node[] nodes = FindObjectsOfType<Node>();
        foreach (var node in nodes)
        {
            node.connections.Clear();
            node.currentConnections = 0;
        }

        // 3. 清空总列表
        allConnections.Clear();

        // 4. 清理预览线（如果正在拖）
        ClearPreviewLine();
        selectedNode = null;
        isDragging = false;
    }

}



