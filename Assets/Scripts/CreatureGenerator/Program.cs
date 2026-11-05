/// <author>lusen</author>
/// <date>2026-11-05</date>
/// <summary>
/// 程序入口: 修改参数或通过右键菜单 Generate 生成生物并打印节点树
/// </summary>
using UnityEngine;
[ExecuteAlways]
public class Program : MonoBehaviour
{
    public GenerationConfig config;
    public string seed;

    [ContextMenu("Generate")]
    private void Generate() => Debug.Log(CreaturePrinter.Print(CreatureGenerator.Generate(config, seed)));
}
