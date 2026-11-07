/// <author> lusen </author>
/// <date>2026-11-06</date>
/// <summary>
/// 昆虫部位节点类
/// </summary>
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class PartNode
{
    public int id;          // 节点ID (根节点为-1)
    public InsectPart part; // 部位类型
    public int parentId;    // 父节点ID
    public float length;    // 部位长度
    public float width;     // 部位宽度
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

        void PrintNode(PartNode node, int depth, bool isLast)
        {
            string prefix = "";
            for (int i = 0; i < depth; i++)
            {
                prefix += "    ";
            }
            string branch = isLast ? "└─ " : "├─ ";
            Debug.Log($"{prefix}{branch}[ID:{node.id}] Part:{node.part} | L:{node.length:F2} W:{node.width:F2}");

            if (childrenDict.TryGetValue(node.id, out var childs))
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    PrintNode(childs[i], depth + 1, i == childs.Count - 1);
                }
            }
        }

        for (int i = 0; i < rootChildren.Count; i++)
        {
            PrintNode(rootChildren[i], 0, i == rootChildren.Count - 1);
        }
    }
}