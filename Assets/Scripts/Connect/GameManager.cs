using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public LayerMask nodeLayer;
    public Node selectedNode;
    public GameObject linePrefab;
    private LineRenderer previewLine;//跟随鼠标走
    private GameObject previewLineObj;

    private bool isDragging;

    // 所有连线（用于检测断线）
    private List<NodeConnection> allConnections = new List<NodeConnection>();

    // 鼠标当前指向的连线
    private NodeConnection hoveredConnection;

    // 鼠标距离线多近算点中
    public float disconnectThreshold = 0.15f;


    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandleMouseInput();

        DetectConnectionUnderMouse();

        if (isDragging && previewLine != null && selectedNode != null)
        {
            UpdatePreviewLine();
        }
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

        // 3. 可选：距离 / 方向校验
        // if (!IsValidDirection(a, b)) return;

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


}
