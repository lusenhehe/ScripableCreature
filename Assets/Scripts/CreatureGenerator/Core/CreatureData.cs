/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 生物实例数据
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Creature/CreatureData")]
public class CreatureData : ScriptableObject
{
    [Tooltip("显示名")] public string displayName;
    [Tooltip("部件列表")] public List<CreaturePart> parts = new();

#if UNITY_EDITOR
    public static event System.Action<CreatureData> Changed;

    public void NotifyChanged() => Changed?.Invoke(this);

    void OnValidate() => NotifyChanged();
#endif
}
[Serializable]
public class CreaturePart
{
    public int id;
    public NodeType type;
    public int side;       // -1 左, 0 中, 1 右
    public int parentId;   // 根为 -1
    [Tooltip("骨骼长度")] public float length;
    [Tooltip("起始半径")] public float radius;
}