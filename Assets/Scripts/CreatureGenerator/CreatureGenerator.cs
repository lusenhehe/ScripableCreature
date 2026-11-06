/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 按脊柱蓝图生成生物, 结果写入 CreatureData
/// </summary>
using System.Collections.Generic;
using UnityEngine;
public static class CreatureGenerator
{
    /// <summary>
    /// 按蓝图与种子生成拓扑, 写入目标 CreatureData
    /// </summary>
    public static void Generate(GenerationConfig config, string seed, CreatureData target)
    {
        CreatureNode root = BuildTree(config, seed);
        target.WriteFromTree(root);
    }

    private static CreatureNode BuildTree(GenerationConfig config, string seed)
    {
        System.Random random = new System.Random(seed.GetHashCode());
        int nextId = 0;
        bool truncated = false;

        int segmentCount = Range(random, config.spineSegments, 1);
        CreatureNode root = null;
        CreatureNode previous = null;
        List<CreatureNode> spine = new();

        for (int i = 0; i < segmentCount; i++) {
            CreatureNode segment = Alloc(ref nextId, NodeType.Body, 0, previous, config.maxNodes, ref truncated);
            if (segment == null) break;
            if (root == null) root = segment;
            else previous.children.Add(segment);
            previous = segment;
            spine.Add(segment);
        }

        if (root == null) return new CreatureNode { id = 0, type = NodeType.Body };

        CreatureNode head = Alloc(ref nextId, NodeType.Head, 0, spine[0], config.maxNodes, ref truncated);
        if (head != null) {
            spine[0].children.Insert(0, head);
            if (random.NextDouble() < config.antennaChance)
                TryAddPair(head, NodeType.End, ref nextId, config.maxNodes, ref truncated);
        }

        foreach (CreatureNode segment in spine) {
            if (random.NextDouble() >= config.legPairChance) continue;
            int joints = Range(random, config.legJoints, 0);
            TryAddMirroredLegs(segment, joints, ref nextId, config.maxNodes, ref truncated);
        }

        if (random.NextDouble() < config.tailChance) {
            int tails = Range(random, config.tailSegments, 1);
            CreatureNode cursor = spine[spine.Count - 1];
            for (int i = 0; i < tails; i++) {
                CreatureNode tail = Alloc(ref nextId, NodeType.Tail, 0, cursor, config.maxNodes, ref truncated);
                if (tail == null) break;
                cursor.children.Add(tail);
                cursor = tail;
            }
        }

        if (truncated) Debug.LogWarning($"节点数达到上限 {config.maxNodes}, 部分挂载被截断");
        return root;
    }

    private static void TryAddMirroredLegs(CreatureNode segment, int joints, ref int nextId, int maxNodes, ref bool truncated)
    {
        foreach (int side in new[] { -1, 1 }) {
            CreatureNode limb = Alloc(ref nextId, NodeType.Limb, side, segment, maxNodes, ref truncated);
            if (limb == null) return;
            segment.children.Add(limb);

            CreatureNode cursor = limb;
            for (int i = 0; i < joints; i++) {
                CreatureNode joint = Alloc(ref nextId, NodeType.Joint, side, cursor, maxNodes, ref truncated);
                if (joint == null) break;
                cursor.children.Add(joint);
                cursor = joint;
            }

            CreatureNode claw = Alloc(ref nextId, NodeType.End, side, cursor, maxNodes, ref truncated);
            if (claw != null) cursor.children.Add(claw);
        }
    }

    private static void TryAddPair(CreatureNode parent, NodeType type, ref int nextId, int maxNodes, ref bool truncated)
    {
        foreach (int side in new[] { -1, 1 }) {
            CreatureNode child = Alloc(ref nextId, type, side, parent, maxNodes, ref truncated);
            if (child == null) return;
            parent.children.Add(child);
        }
    }

    private static CreatureNode Alloc(ref int nextId, NodeType type, int side, CreatureNode parent, int maxNodes, ref bool truncated)
    {
        if (nextId >= maxNodes) {
            truncated = true;
            return null;
        }
        return new CreatureNode {
            id = nextId++,
            type = type,
            side = side,
            depth = parent == null ? 0 : parent.depth + 1,
            parent = parent
        };
    }

    private static int Range(System.Random random, Vector2Int range, int fallbackMin)
    {
        int min = Mathf.Max(fallbackMin, Mathf.Min(range.x, range.y));
        int max = Mathf.Max(range.x, range.y);
        return random.Next(min, max + 1);
    }
}
