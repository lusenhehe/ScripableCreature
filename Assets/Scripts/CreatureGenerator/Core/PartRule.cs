/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 部件生成规则: 一个部件的若干种备选长法, 每种长法由若干挂载位组成
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PartRule", menuName = "Creature/PartRule")]
public class PartRule : ScriptableObject {
    [Tooltip("部件语义类型")]                public NodeType type;
    [Tooltip("备选长法, 按权重抽取一条")]    public List<Production> productions = new();
}
[Serializable]
public class Production {
    [Tooltip("抽中权重")][Min(0f)]  public float weight = 1.0f;
    [Tooltip("挂载位")]             public List<Slot> slots = new();
}
[Serializable]
public class Slot {
    [Tooltip("子部件规则")]                              public PartRule child;
    [Tooltip("生成数量范围 (x = 最少, y = 最多)")]       public Vector2Int count = new(1, 1);
    [Tooltip("该挂载位出现的概率")][Range(0f, 1f)]       public float chance = 1.0f;
    [Tooltip("左右成对生成, 数量按对计")]                public bool mirrored;
}