/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 程序入口: 生成生物数据, 或按数据生成骨骼网格
/// </summary>
using System.Collections.Generic;
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

    [ContextMenu("Build Creature Meshes")]
    private void BuildCreatureMeshes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            if (child.name != "Mesh") continue;
            var renderers = child.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length > 0) DestroyImmediate(renderers[0].sharedMaterial);
            foreach (MeshFilter filter in child.GetComponentsInChildren<MeshFilter>(true))
                DestroyImmediate(filter.sharedMesh);
            DestroyImmediate(child.gameObject);
        }

        Transform holder = new GameObject("Mesh").transform;
        holder.SetParent(transform, false);
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.76f, 0.63f, 0.42f) };
        var nodes = new Dictionary<int, Transform>();
        var partById = new Dictionary<int, CreaturePart>();
        List<CreaturePartMesh> bones = CreatureMeshGenerater.Build(creature);
        foreach (CreaturePart part in creature.parts) partById[part.id] = part;
        foreach (CreaturePartMesh bone in bones) {
            var go = new GameObject($"{bone.type} #{bone.id}");
            go.transform.SetParent(holder, false);
            go.transform.localPosition = bone.position;
            go.transform.localRotation = bone.rotation;
            go.AddComponent<MeshFilter>().sharedMesh = bone.mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            nodes[bone.id] = go.transform;
        }
        foreach (CreaturePart part in creature.parts)
            if (part.parentId >= 0 && nodes.ContainsKey(part.id) && nodes.ContainsKey(part.parentId))
                nodes[part.id].SetParent(nodes[part.parentId], true);
        foreach (CreaturePartMesh bone in bones) {
            if (bone.type != NodeType.End || !partById.TryGetValue(bone.id, out CreaturePart part)) continue;
            if (!partById.TryGetValue(part.parentId, out CreaturePart parent) || parent.type == NodeType.Head) continue;
            var tip = new GameObject("Tip");
            tip.transform.SetParent(nodes[bone.id], false);
            tip.transform.localPosition = Vector3.up * bone.length;
        }
    }
}
