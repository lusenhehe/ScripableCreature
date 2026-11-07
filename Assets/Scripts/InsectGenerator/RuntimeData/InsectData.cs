///<author> lusen </author>
/// <date>2026-11-06</date>

/// <summary>
/// 昆虫数据类
/// </summary>
using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "InsectData", menuName = "Insect/Data")]
public class InsectData : ScriptableObject
{
    public string seed;             // 种子
    public List<PartNode> parts;    // 部位节点列表

    
}