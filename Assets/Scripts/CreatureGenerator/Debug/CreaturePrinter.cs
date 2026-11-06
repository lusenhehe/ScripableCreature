/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 按 parentId 打印生物部件列表
/// </summary>
using System.Collections.Generic;
using System.Text;

public static class CreaturePrinter
{
    public static string Print(CreatureData data)
    {
        StringBuilder builder = new();
        int root = data.parts.FindIndex(part => part.parentId < 0);
        if (root >= 0) Append(builder, data.parts, root, 0);
        return builder.ToString();
    }

    private static void Append(StringBuilder builder, List<CreaturePart> parts, int index, int depth)
    {
        CreaturePart node = parts[index];
        string side = node.side switch { -1 => " [L]", 1 => " [R]", _ => "" };
        builder.Append(' ', depth * 4).Append($"{node.type} #{node.id}{side}").AppendLine();
        for (int i = 0; i < parts.Count; i++)
            if (parts[i].parentId == node.id) Append(builder, parts, i, depth + 1);
    }
}
