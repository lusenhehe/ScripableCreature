/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 用于打印生物节点树的调试工具
/// </summary>
using System.Text;
public static class CreaturePrinter
{
    public static string Print(CreatureData data) => Print(data.BuildTree());

    public static string Print(CreatureNode root)
    {
        StringBuilder builder = new();
        Append(builder, root);
        return builder.ToString();
    }

    private static void Append(StringBuilder builder, CreatureNode node)
    {
        string side = node.side switch { -1 => " [L]", 1 => " [R]", _ => "" };
        builder.Append(' ', node.depth * 4)
               .Append($"{node.type} #{node.id}{side}")
               .AppendLine();
        foreach (CreatureNode child in node.children) Append(builder, child);
    }
}
