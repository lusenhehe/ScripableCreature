/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 生物部件类型
/// </summary>
public enum NodeType
{
    Head,     // 头
    Body,     // 脊柱段
    Limb,     // 上腿
    End,      // 下腿 / 爪
    Tail,     // 尾巴
    Antenna,  // 触角
    Hip,      // 髋
    // 之后可能会合并 Tail , Limb, Body, Antenna 等类型
}

public enum InsectType
{
    Head, // 头部
    Mouse, // 口器
    Antenna, // 触角
    Eye, // 复眼

    Chest, // 胸部
    Leg, // 腿部
    Wing, // 翅膀

    Abdomen, // 腹部
    Appendage, // 附肢
}
