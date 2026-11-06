/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 表示生物的一个节点, 包含类型、左右侧、深度、父节点和子节点信息
/// </summary>
using System.Collections.Generic;
public class CreatureNode
{
    public int id;
    public NodeType type;
    public int side;                            // -1 左, 0 中, 1 右
    public int depth;
    public CreatureNode parent;
    public List<CreatureNode> children = new();
}
