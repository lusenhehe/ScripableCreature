///<author> lusen </author>
/// <date>2026-11-06</date>

/// <summary>
/// 昆虫生成配置：体长 B 与各身体部位的生成规则
/// </summary>

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InsectGenerationConfig", menuName = "Insect/GenerationConfig")]
public class InsectGenerationConfig : ScriptableObject
{
    [Tooltip("体长 B (米)")] [Min(0.0001f)] public float bodyLength = 0.1f;
    public List<GenerationRule> rules;           // 子部位规则（口器/触角/眼/足/翅/附肢）
}
