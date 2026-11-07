///<author> lusen </author>
/// <date>2026-11-07</date>

/// <summary>
/// 昆虫数据类：根据 InsectGenerationConfig 随机生成身体段与子部位树
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utils;

[CreateAssetMenu(fileName = "InsectData", menuName = "Insect/Data")]
public class InsectData : ScriptableObject
{
    public string seed;             // 种子
    public List<PartNode> parts;    // 部位节点列表

    public static InsectData Generate(string seed, InsectGenerationConfig config)
    {
        var rng = new System.Random(StableHash(seed));
        var data = ScriptableObject.CreateInstance<InsectData>();

        data.seed = seed;
        data.parts = new List<PartNode>();

        int nextId = 0;

        // 1. 生成身体段（线性链：Head → Chest → Abdomen，按 part 排序保证顺序）
        var segmentIds = new Dictionary<Insect.Part, List<int>>();
        int chainParent = -1;
        var segments = config.rules
            .Where(s => s != null && s.isBodySegment)
            .OrderBy(s => (int)s.part)
            .ToList();

        foreach (var seg in segments)
        {
            if ((float)rng.NextDouble() > seg.probability) continue;

            int count = WeightedRandom.Pick(seg.amountOptions, rng);
            var ids = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var node = new PartNode
                {
                    id = nextId++,
                    part = seg.part,
                    parentId = chainParent,
                    length = WeightedRandom.Pick(seg.lengthOptions, rng) * config.bodyLength,
                    radius = WeightedRandom.Pick(seg.radiusOptions, rng) * config.bodyLength,
                };
                data.parts.Add(node);
                ids.Add(node.id);
            }
            segmentIds[seg.part] = ids;
            if (ids.Count > 0) chainParent = ids[ids.Count - 1];
        }

        // 2. 生成子部位（按 ChildToParent 挂到对应身体段，每个父段实例都挂）
        foreach (var rule in config.rules)
        {
            if (rule == null || rule.isBodySegment) continue;
            if (!Insect.ChildToParent.TryGetValue(rule.part, out var parentPart)) continue;
            if (!segmentIds.TryGetValue(parentPart, out var parents) || parents.Count == 0) continue;
            if ((float)rng.NextDouble() > rule.probability) continue;

            int count = WeightedRandom.Pick(rule.amountOptions, rng);
            foreach (var pid in parents)
            {
                for (int i = 0; i < count; i++)
                {
                    data.parts.Add(new PartNode
                    {
                        id = nextId++,
                        part = rule.part,
                        parentId = pid,
                        length = WeightedRandom.Pick(rule.lengthOptions, rng) * config.bodyLength,
                        radius = WeightedRandom.Pick(rule.radiusOptions, rng) * config.bodyLength,
                    });
                }
            }
        }

        PartNode.debugPrintTree(data.parts);
        // 在Data/Insects/目录下保存生成的昆虫数据
#if UNITY_EDITOR
        string path = $"Assets/Data/InsectTests/Insect_{seed}.asset";
        UnityEditor.AssetDatabase.CreateAsset(data, path);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
        
        return data;
    }
    static int StableHash(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        unchecked
        {
            uint hash = 2166136261u;
            foreach (char c in s)
            {
                hash ^= c;
                hash *= 16777619u;
            }
            return (int)hash;
        }
    }
}
