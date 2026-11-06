/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 按脊柱蓝图生成生物, 结果直接写入 CreatureData.parts
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
            for (int n = 0, pairs = Range(random, config.antennaPairs); n < pairs; n++)
                for (int side = -1; side <= 1; side += 2)
                    Add(parts, ref nextId, NodeType.End, side, head);

        for (int i = 1; i < segmentCount; i++)
            spine[i] = Add(parts, ref nextId, NodeType.Body, 0, spine[i - 1]);

        foreach (int segment in spine)
            if (random.NextDouble() < config.legPairChance)
                AddLegs(parts, ref nextId, segment, Range(random, config.legJoints));

        if (random.NextDouble() < config.tailChance) {
            int cursor = spine[^1];
            for (int n = 0, count = Range(random, config.tailSegments); n < count; n++)
                cursor = Add(parts, ref nextId, NodeType.Tail, 0, cursor);
        }
    }

    private static void AddLegs(List<CreaturePart> parts, ref int nextId, int segment, int joints)
    {
        for (int side = -1; side <= 1; side += 2) {
            int cursor = Add(parts, ref nextId, NodeType.Limb, side, segment);
            for (int i = 0; i < joints; i++)
                cursor = Add(parts, ref nextId, NodeType.Joint, side, cursor);
            Add(parts, ref nextId, NodeType.End, side, cursor);
        }
    }

    private static int Add(List<CreaturePart> parts, ref int nextId, NodeType type, int side, int parentId)
    {
        parts.Add(new CreaturePart { id = nextId, type = type, side = side, parentId = parentId });
        return nextId++;
    }

    private static int Range(System.Random random, Vector2Int range) => random.Next(range.x, range.y + 1);
}
