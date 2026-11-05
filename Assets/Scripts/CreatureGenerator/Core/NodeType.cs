/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 表示生物节点的类型
/// </summary>
public enum NodeType
{
    Head,       // 头部
    Body,       // 躯干、身体主干
    Joint,      // 关节、连接节点
    Limb,       // 肢体：腿、手臂、触手、翅膀、鳍
    End,        // 末端：手、脚、爪、触角等
    Tail        // 尾巴
}