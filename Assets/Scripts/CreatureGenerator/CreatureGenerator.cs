/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 负责生成生物节点树的生成器: 从根部件开始广度优先展开, 按权重抽取长法并填充挂载位
/// </summary>
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class CreatureGenerator
{
    /// <summary>
    /// 生成生物节点树
    /// </summary>
    /// <param name="config">生成配置</param>
    /// <param name="seed">随机种子</param>
    /// <returns>生成的生物节点树根节点</returns>
    public static CreatureNode Generate(GenerationConfig config, string seed)
    {
        int nextId = 0;
        bool truncated = false;
        CreatureNode root = new CreatureNode { id = nextId++, rule = config.root };

        // 每个节点携带独立种子, 镜像的一对节点共用同一种子, 保证左右子树一致
        Queue<(CreatureNode node, int seed)> pending = new();
        pending.Enqueue((root, seed.GetHashCode()));

        while (pending.Count > 0) {
            // 从队列中取出待处理节点及其种子
            var (node, nodeSeed) = pending.Dequeue();
            System.Random random = new System.Random(nodeSeed);
            // 判断是否超过生成预算 (深度或节点数)
            bool overBudget = node.depth >= config.maxDepth || nextId >= config.maxNodes;
            Production production = PickProduction(node.rule.productions, overBudget, random);
            if (production == null) continue;
            // 遍历生成规则的挂载位
            foreach (Slot slot in production.slots) {
                // 如果该挂载位没有子节点或未通过概率检查, 则跳过
                if (!slot.child || random.NextDouble() >= slot.chance) continue;
                // 计算该挂载位需要生成的子节点数量
                int count = random.Next(slot.count.x, Mathf.Max(slot.count.x, slot.count.y) + 1);
                int[] sides = slot.mirrored ? new[] { -1, 1 } : new[] { node.side };
                for (int i = 0; i < count; i++) {
                    int childSeed = random.Next();
                    if (nextId + sides.Length > config.maxNodes) {
                        truncated = true;
                        break;
                    }
                    foreach (int side in sides) {
                        CreatureNode child = new CreatureNode {
                            id = nextId++,
                            rule = slot.child,
                            side = side,
                            depth = node.depth + 1,
                            parent = node
                        };
                        node.children.Add(child);
                        pending.Enqueue((child, childSeed));
                    }
                }
            }
        }

        if (truncated) Debug.LogWarning($"节点数达到上限 {config.maxNodes}, 部分挂载位被截断, 请检查规则是否过大");
        return root;
    }
    /// <summary>
    /// 从给定的生成规则列表中选择一个生成规则
    /// </summary>
    /// <param name="productions">生成规则列表</param>
    /// <param name="terminalOnly">是否只选择终端规则</param>
    /// <param name="random">随机数生成器</param>
    /// <returns>选中的生成规则, 若无可选规则则返回 null</returns>
    private static Production PickProduction(List<Production> productions, bool terminalOnly, System.Random random)
    {
        List<Production> candidates = productions.Where(p => !terminalOnly || p.terminal).ToList();
        float roll = (float)random.NextDouble() * candidates.Sum(p => p.weight);
        foreach (Production production in candidates) {
            roll -= production.weight;
            if (roll < 0f) return production;
        }
        return null;
    }
}
