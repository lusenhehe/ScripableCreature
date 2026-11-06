/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 程序入口: 用蓝图生成并写入 CreatureData, 或直接打印已有生物数据
/// </summary>
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteAlways]
public class Program : MonoBehaviour
{
    [Tooltip("生成蓝图")] public GenerationConfig config;
    [Tooltip("生物数据, 生成结果写入此处, 也可手改")] public CreatureData creature;
    public string seed;

    [ContextMenu("Generate Into CreatureData")]
    private void Generate()
    {
        CreatureGenerator.Generate(config, seed, creature);
#if UNITY_EDITOR
        EditorUtility.SetDirty(creature);
#endif
        Debug.Log(CreaturePrinter.Print(creature));
    }

    [ContextMenu("Print CreatureData")]
    private void PrintCreature() => Debug.Log(CreaturePrinter.Print(creature));
}
