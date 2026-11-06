/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 生物部件类型. 触角与爪子都是 End, 由父节点区分
/// </summary>
public enum NodeType
{
    Head,   // 头
    Body,   // 脊柱段
    Joint,  // 腿关节
    Limb,   // 腿
    End,    // 爪或触角
    Tail    // 尾巴
}
