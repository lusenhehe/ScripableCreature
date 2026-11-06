/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 生物实例数据: 扁平部件列表, 可程序生成也可由玩家直接编辑
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CreatureData", menuName = "Creature/CreatureData")]
public class CreatureData : ScriptableObject {
    [Tooltip("显示名")] public string displayName;
    [Tooltip("部件列表, parentId = -1 表示根")] public List<CreaturePart> parts = new();

    /// <summary>
    /// 用扁平列表重建运行时节点树
    /// </summary>
    public CreatureNode BuildTree()
    {
        Dictionary<int, CreatureNode> map = new();
        CreatureNode root = null;
        foreach (CreaturePart part in parts) {
            map[part.id] = new CreatureNode {
                id = part.id,
                type = part.type,
                side = part.side
            };
        }
        foreach (CreaturePart part in parts) {
            CreatureNode node = map[part.id];
            if (part.parentId < 0) {
                root = node;
                continue;
            }
            CreatureNode parent = map[part.parentId];
            node.parent = parent;
            node.depth = parent.depth + 1;
            parent.children.Add(node);
        }
        return root;
    }

    /// <summary>
    /// 用运行时节点树覆盖本数据
    /// </summary>
    public void WriteFromTree(CreatureNode root)
    {
        parts.Clear();
        Stack<CreatureNode> stack = new();
        stack.Push(root);
        while (stack.Count > 0) {
            CreatureNode node = stack.Pop();
            if (node is null) continue;
            parts.Add(new CreaturePart {
                id = node.id,
                type = node.type,
                side = node.side,
                parentId = node.parent is null ? -1 : node.parent.id
            });
            for (int i = node.children.Count - 1; i >= 0; i--) stack.Push(node.children[i]);
        }
    }
}

[Serializable]
public class CreaturePart {
    public int id;
    public NodeType type;
    public int side;       // -1 左, 0 中, 1 右
    public int parentId;   // 根为 -1
}
