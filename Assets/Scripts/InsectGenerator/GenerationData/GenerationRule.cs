///<author> lusen </author>
/// <date>2026-11-06</date>
/// <summary>
/// 表示昆虫身体部位的生成规则
/// </summary>
using UnityEngine;
[System.Serializable]
public class GenerationRule
{
    public InsectPart part;             // 部位类型
    public float probability;           // 出现概率
    public Vector2Int amountRange;      // 数量范围
    public bool isMirrored;             // 是否左右镜像成对生成 
    public Vector2 lengthRange;         // 长度范围
    public Vector2 radiusRange;         // 半径范围
}
