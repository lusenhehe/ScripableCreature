/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 生物实例数据: 扁平部件列表, 可程序生成也可由玩家直接编辑
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Creature/CreatureData")]
public class CreatureData : ScriptableObject
{
    [Tooltip("显示名")] public string displayName;
    [Tooltip("部件列表, parentId = -1 表示根")] public List<CreaturePart> parts = new();
}

[Serializable]
public class CreaturePart
{
    public int id;
    public NodeType type;
    public int side;       // -1 左, 0 中, 1 右
    public int parentId;   // 根为 -1
}
