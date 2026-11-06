/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 脊柱蓝图: 描述整只生物的拓扑随机参数
/// </summary>
using UnityEngine;

[CreateAssetMenu(fileName = "GenerationConfig", menuName = "Creature/GenerationConfig")]
public class GenerationConfig : ScriptableObject
{
    [Header("脊柱")]
    [Tooltip("脊柱段数范围")] public Vector2Int spineSegments = new(2, 4);

    [Header("腿")]
    [Tooltip("每一段脊柱长出一对腿的概率")][Range(0f, 1f)] public float legPairChance = 0.8f;

    [Header("尾")]
    [Tooltip("末段脊柱长出尾巴的概率")][Range(0f, 1f)] public float tailChance = 0.7f;
    [Tooltip("尾巴节数范围")] public Vector2Int tailSegments = new(1, 3);

    [Header("头")]
    [Tooltip("长出一对触角的概率")][Range(0f, 1f)] public float antennaChance = 0.65f;
    [Tooltip("每根触角的节数")] public Vector2Int antennaSegments = new(2, 3);

    private void OnValidate()
    {
        spineSegments = Ordered(spineSegments, 1);
        tailSegments = Ordered(tailSegments, 1);
        antennaSegments = Ordered(antennaSegments, 1);
    }

    private static Vector2Int Ordered(Vector2Int range, int min)
    {
        int lo = Mathf.Max(min, Mathf.Min(range.x, range.y));
        int hi = Mathf.Max(lo, Mathf.Max(range.x, range.y));
        return new Vector2Int(lo, hi);
    }
}
