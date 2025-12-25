using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int maxConnections;      // 球上显示的数字
    public int currentConnections;  // 当前已连接数量
    //public GameObject node;
    public TextMeshProUGUI countText;

    public List<NodeConnection> connections = new List<NodeConnection>();

    private void Start()
    {
        countText = GetComponentInChildren<TextMeshProUGUI>();
        countText.text = maxConnections.ToString();

    }

    public bool CanConnect()
    {
        return currentConnections < maxConnections;
    }

    //void OnMouseDown()
    //{
    //    GameManager.Instance.OnNodeClicked(this);
    //}
}
