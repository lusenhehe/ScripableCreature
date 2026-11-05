# 生物生成规则设计（拓扑层）

## 目标
用加权产生式文法生成生物的节点拓扑树，规则全部以 ScriptableObject 资产配置，Inspector 原生可编辑。本轮只做拓扑，不涉及空间位置与外观。

## 数据模型
- `GenerationConfig`：`PartRule root`、`int maxDepth`、`int maxNodes`
- `PartRule`（ScriptableObject）：`NodeType type`（语义标签）、`List<Production> productions`
- `Production`：`float weight`、`bool terminal`、`List<Slot> slots`
- `Slot`：`PartRule child`、`Vector2Int count`、`float chance`、`bool mirrored`
- `CreatureNode`：`id`、`PartRule rule`、`side`(-1/0/1)、`depth`、`parent`、`children`

## 生成流程
1. `new System.Random(seed.GetHashCode())`（稳定哈希暂不考虑）。
2. 从 `root` 开始广度优先展开（队列）。
3. 每个节点：若 `depth >= maxDepth` 或节点数 `>= maxNodes`，只在 `terminal` 长法中按权重抽；否则在全部长法中抽。无可选长法则为叶子。
4. 每个插槽：按 `chance` 判定出现，在 `count` 范围内抽数量；`mirrored` 时每个生成左右一对。
5. 镜像对共用一个子种子，各自用独立随机数展开子树，保证左右一致。
6. 节点预算为硬上限，超出时截断并 `Debug.LogWarning`。

## 删除
`GenerationRule.cs`、`SerializableDictionary.cs`、`UtilDebug.cs`。

## 验证
示例资产：躯干 → 头、尾、2~3 对腿；腿 → 关节链 → 爪。检查：换种子结构变化；同种子结果一致；每条腿以爪结尾；左右对称。
