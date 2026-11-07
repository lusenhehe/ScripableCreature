///<author> lusen </author>
/// <date>2026-11-06</date>

/// <summary>
/// 枚举表示昆虫的各个身体部位
/// </summary>
using System.Collections.Generic;
public static class Insect
{
    public enum Part {
        Head,       // 头部
        Mouth,      // 口器
        Antenna,    // 触角
        Eye,        // 复眼

        Chest,      // 胸部
        Leg,        // 腿部
        Wing,       // 翅膀

        Abdomen,    // 腹部
        Appendage,  // 附肢

    }
    public static readonly Dictionary<Part, Part> ChildToParent = new Dictionary<Part, Part>
    {
        { Part.Mouth,     Part.Head },
        { Part.Antenna,   Part.Head },
        { Part.Eye,       Part.Head },
        { Part.Leg,       Part.Chest },
        { Part.Wing,      Part.Chest },
        { Part.Appendage, Part.Abdomen },
    };
}