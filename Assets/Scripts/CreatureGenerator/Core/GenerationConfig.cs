/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// Spore 式脊柱蓝图: 一个模板描述整只生物的拓扑参数
/// </summary>
using UnityEngine;
[CreateAssetMenu(fileName = "GenerationConfig", menuName = "Creature/GenerationConfig")]
public class GenerationConfig : ScriptableObject {
    [Header("脊柱")]
    [Tooltip("脊柱段数范围")] public Vector2Int spineSegments = new(2, 4);

    [Header("腿")]
    [Tooltip("每一段脊柱长出一对腿的概率")][Range(0f, 1f)] public float legPairChance = 0.8f;
    [Tooltip("每条腿的关节数范围 (之后再接爪子)")] public Vector2Int legJoints = new(1, 3);

    [Header("头")]
    [Tooltip("头上长出一对触角的概率")][Range(0f, 1f)] public float antennaChance = 0.5f;

    [Header("尾")]
    [Tooltip("末段脊柱长出尾巴的概率")][Range(0f, 1f)] public float tailChance = 0.7f;
    [Tooltip("尾巴节数范围")] public Vector2Int tailSegments = new(1, 3);

    [Header("预算")]
    [Tooltip("节点总数硬上限")][Min(1)] public int maxNodes = 64;
}
