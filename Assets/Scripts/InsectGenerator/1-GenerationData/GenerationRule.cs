///<author> lusen </author>
/// <date>2026-11-06</date>
/// <summary>
/// 表示昆虫身体部位的生成规则
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using Utils;

[System.Serializable]
public class GenerationRule
{
    public Insect.Part part;                // 部位类型

    [Header("生成参数")]
    [Range(0f, 1f)] public float probability = 1f;          // 出现概率
    public List<WeightedInt> amountOptions = new();        // 数量加权选项

    [Header("结构")]
    public bool isBodySegment;              // 是否为主体结构
    public bool isMirrored;                 // 是否左右镜像生成

    [Header("尺寸")]
    public List<WeightedFloat> lengthOptions = new();      // 长度加权选项（单位 B）
    public List<WeightedFloat> radiusOptions = new();      // 半径加权选项（单位 B）
}
