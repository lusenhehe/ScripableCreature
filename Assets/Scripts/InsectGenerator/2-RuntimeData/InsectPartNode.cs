/// <author> lusen </author>
/// <date>2026-11-06</date>
/// <summary>
/// 昆虫部位节点类
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using System.Text;
[System.Serializable]
public class PartNode
{
    public int id;          // 节点ID (根节点为-1)
    public Insect.Part part; // 部位类型
    public int parentId;    // 父节点ID
    public float length;    // 部位长度
    public float radius;     // 部位半径
    public static void debugPrintTree(List<PartNode> parts)
    {
        var nodeDict = new Dictionary<int, PartNode>();
        var childrenDict = new Dictionary<int, List<PartNode>>();
        foreach (var n in parts)
        {
            nodeDict[n.id] = n;
            if (!childrenDict.ContainsKey(n.parentId))
                childrenDict[n.parentId] = new List<PartNode>();
            childrenDict[n.parentId].Add(n);
        }
        if (!childrenDict.TryGetValue(-1, out var rootChildren))
        {
            Debug.Log("Tree has no root (parentId = -1)");
            return;
        }
        StringBuilder sb = new StringBuilder();

        void PrintNode(PartNode node, int depth, bool isLast, string prefixStr)
        {
            string branch = isLast ? "└─ " : "├─ ";
            sb.AppendLine($"{prefixStr}{branch}[ID:{node.id}] Part:{node.part} | L:{node.length:F3} R:{node.radius:F4}");

            if (childrenDict.TryGetValue(node.id, out var childs))
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    bool childLast = i == childs.Count - 1;
                    string newPrefix = prefixStr + (isLast ? "    " : "│   ");
                    PrintNode(childs[i], depth + 1, childLast, newPrefix);
                }
            }
        }

        for (int i = 0; i < rootChildren.Count; i++)
        {
            PrintNode(rootChildren[i], 0, i == rootChildren.Count - 1, "");
        }

        Debug.Log(sb.ToString());
    }

}