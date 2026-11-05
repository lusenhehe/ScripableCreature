///<author>lusen</author>
///<date>2026-11-05</date>
/// <summary>
/// 表示生物的一个节点, 包含节点的规则、左右侧、深度、父节点和子节点信息
/// </summary>
using System.Collections.Generic;
public class CreatureNode
{
    public int id;                              // 节点的唯一标识
    public PartRule rule;                       // 生成该节点所用的部件规则
    public int side;                            // -1 左, 0 中, 1 右
    public int depth;                           // 根节点为 0
    public CreatureNode parent;                 // 父节点, 如果是根节点则为 null
    public List<CreatureNode> children = new(); // 子节点列表, 如果没有子节点则为空列表
}
