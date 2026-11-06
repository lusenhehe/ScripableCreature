# Spore 式脊柱蓝图配置

## 目标
一个生物模板 = 一个 ScriptableObject。Inspector 直接调「身段、腿、头尾」参数，不再维护互相引用的 PartRule 资产网。

## 数据模型（GenerationConfig）
- `spineSegments`：脊柱段数范围
- `legPairChance`：每一段长出一对腿的概率
- `legJoints`：每条腿的关节数范围（之后再接爪子）
- `antennaChance`：头上成对触角的概率
- `tailChance` / `tailSegments`：尾巴出现概率与节数范围
- `maxNodes`：节点硬上限

## 生成流程
1. 生成脊柱串行链（Body），段数在范围内随机。
2. 第一段挂 Head；按概率挂一对 Antenna（End）。
3. 每一段按概率挂一对 Leg（Limb）；每条腿后接 `legJoints` 个 Joint，最后接 Claw（End）。
4. 最后一段按概率挂 Tail 链（Tail）。
5. 节点数触及 `maxNodes` 时截断并警告。

## 节点与数据分离
- `GenerationConfig`：蓝图参数，只负责「怎么随机长」
- `CreatureData`（ScriptableObject）：生物实例，扁平 `parts` 列表（id / type / side / parentId），程序生成写入，玩家也可直接改
- `CreatureNode`：运行时树，由 `CreatureData.BuildTree()` 重建

## 删除
`PartRule.cs` 及 `Assets/Creature/Rules/` 下旧规则资产。
