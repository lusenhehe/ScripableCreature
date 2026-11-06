/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 按种子与蓝图生成生物拓扑, 结果写入 CreatureData.parts
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public static class CreatureGenerator
{
    public static void Generate(GenerationConfig config, string seed, CreatureData target)
    {
        System.Random random = new(Animator.StringToHash(seed));
        List<CreaturePart> parts = target.parts;
        parts.Clear();
        int nextId = 0;

        int segmentCount = Range(random, config.spineSegments);
        int[] spine = new int[segmentCount];
        spine[0] = Add(parts, ref nextId, NodeType.Body, 0, -1);

        int head = Add(parts, ref nextId, NodeType.Head, 0, spine[0]);
        if (random.NextDouble() < config.antennaChance)
            for (int side = -1; side <= 1; side += 2) {
                int cursor = head;
                for (int n = 0, count = Range(random, config.antennaSegments); n < count; n++)
                    cursor = Add(parts, ref nextId, NodeType.Antenna, side, cursor);
            }

        for (int i = 1; i < segmentCount; i++)
            spine[i] = Add(parts, ref nextId, NodeType.Body, 0, spine[i - 1]);

        foreach (int segment in spine)
            if (random.NextDouble() < config.legPairChance)
                AddLegs(parts, ref nextId, segment);

        if (random.NextDouble() < config.tailChance) {
            int cursor = spine[^1];
            for (int n = 0, count = Range(random, config.tailSegments); n < count; n++)
                cursor = Add(parts, ref nextId, NodeType.Tail, 0, cursor);
        }
    }

    private static void AddLegs(List<CreaturePart> parts, ref int nextId, int segment)
    {
        for (int side = -1; side <= 1; side += 2) {
            int limb = Add(parts, ref nextId, NodeType.Limb, side, segment);
            Add(parts, ref nextId, NodeType.End, side, limb);
        }
    }

    private static int Add(List<CreaturePart> parts, ref int nextId, NodeType type, int side, int parentId)
    {
        parts.Add(new CreaturePart { id = nextId, type = type, side = side, parentId = parentId });
        return nextId++;
    }

    private static int Range(System.Random random, Vector2Int range) => random.Next(range.x, range.y + 1);
}
