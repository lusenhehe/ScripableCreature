/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 生物生成配置
/// </summary>
using UnityEngine;
[CreateAssetMenu(fileName = "GenerationConfig", menuName = "Creature/GenerationConfig")]
public class GenerationConfig : ScriptableObject {
    [Tooltip("根部件, 建议为躯干")]                          public PartRule root;
    [Tooltip("深度达到该值后只抽取 terminal 长法")][Min(0)]  public int maxDepth = 5;
    [Tooltip("节点总数硬上限")][Min(1)]                      public int maxNodes = 64;
}
