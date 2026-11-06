/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 按种子生成 CreatureData, 再生成骨骼网格与 IK. liveRebuild 开启后数据变化会自动重建
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Program : MonoBehaviour
{
    [Tooltip("生成蓝图")] public GenerationConfig config;
    [Tooltip("生物数据")] public CreatureData creature;
    public string seed;
    [Tooltip("CreatureData 变化时自动重建网格与 IK")] public bool liveRebuild = true;

    [Header("默认尺寸")]
    [Tooltip("整体基准半径")] public float basis = 0.2f;
    [Tooltip("上腿默认长度")] public float limbLength = 1f;
    [Tooltip("下腿默认长度")] public float endLength = 1.2f;
    [Tooltip("上腿默认半径")] public float limbRadius = 0.055f;
    [Tooltip("下腿默认半径")] public float endRadius = 0.036f;

    int fingerprint;
    float rebuildAt = -1f;
    bool building;

    void OnEnable()
    {
        fingerprint = Fingerprint(creature) ^ SizeFingerprint();
#if UNITY_EDITOR
        CreatureData.Changed -= OnCreatureChanged;
        CreatureData.Changed += OnCreatureChanged;
#endif
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        CreatureData.Changed -= OnCreatureChanged;
#endif
    }

    void Update()
    {
        if (!liveRebuild || building) return;
        int next = Fingerprint(creature) ^ SizeFingerprint();
        if (next != fingerprint) {
            fingerprint = next;
            rebuildAt = Time.realtimeSinceStartup + 0.08f;
        }
        if (rebuildAt < 0f || Time.realtimeSinceStartup < rebuildAt) return;
        rebuildAt = -1f;
        Rebuild();
    }

#if UNITY_EDITOR
    void OnCreatureChanged(CreatureData data)
    {
        if (!liveRebuild || data != creature || building) return;
        fingerprint = Fingerprint(creature) ^ SizeFingerprint();
        rebuildAt = Time.realtimeSinceStartup + 0.08f;
    }
#endif

    [ContextMenu("Generate Into CreatureData")]
    void Generate()
    {
        CreatureGenerator.Generate(config, seed, creature);
#if UNITY_EDITOR
        EditorUtility.SetDirty(creature);
#endif
        fingerprint = Fingerprint(creature) ^ SizeFingerprint();
        if (liveRebuild) Rebuild();
    }


    [ContextMenu("Build Creature Meshes")]
    public void Rebuild()
    {
        if (building) return;
        building = true;
        try {
            BuildCreatureMeshes();
            fingerprint = Fingerprint(creature) ^ SizeFingerprint();
        } finally {
            building = false;
        }
    }

    void BuildCreatureMeshes()
    {
        if (TryGetComponent(out RigBuilder rigBuilder))
            rigBuilder.layers.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            if (child.name != "Mesh" && child.name != "IK" && child.name != "Rig") continue;
            var materials = new HashSet<Material>();
            foreach (MeshRenderer renderer in child.GetComponentsInChildren<MeshRenderer>(true))
                materials.Add(renderer.sharedMaterial);
            foreach (Material old in materials)
                DisposeOwned(old);
            foreach (MeshFilter filter in child.GetComponentsInChildren<MeshFilter>(true))
                DisposeOwned(filter.sharedMesh);
            DisposeOwned(child.gameObject);
        }

        Transform holder = new GameObject("Mesh").transform;
        holder.SetParent(transform, false);
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.76f, 0.63f, 0.42f) };
        var nodes = new Dictionary<int, Transform>();
        var size = new CreatureSizeDefaults {
            basis = basis,
            limbLength = limbLength,
            endLength = endLength,
            limbRadius = limbRadius,
            endRadius = endRadius
        };
        List<CreaturePartMesh> bones = CreatureMeshGenerater.Build(creature, size);
        foreach (CreaturePartMesh bone in bones) {
            var go = new GameObject($"{bone.type} #{bone.id}");
            go.transform.SetParent(holder, false);
            go.transform.localPosition = bone.position;
            go.transform.localRotation = bone.rotation;
            if (bone.mesh) {
                go.AddComponent<MeshFilter>().sharedMesh = bone.mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
            nodes[bone.id] = go.transform;
        }
        foreach (CreaturePartMesh bone in bones) {
            if (!nodes.ContainsKey(bone.id) || !nodes.ContainsKey(bone.parentId)) continue;
            nodes[bone.id].SetParent(nodes[bone.parentId], true);
        }

        foreach (CreaturePartMesh bone in bones) {
            bool needTip = bone.type == NodeType.End;
            if (bone.type == NodeType.Tail) {
                needTip = true;
                foreach (CreaturePartMesh other in bones)
                    if (other.parentId == bone.id && other.type == NodeType.Tail) needTip = false;
            }
            if (!needTip) continue;
            var tip = new GameObject("Tip");
            tip.transform.SetParent(nodes[bone.id], false);
            tip.transform.localPosition = Vector3.up * bone.length;
        }

#if UNITY_EDITOR
        Transform previous = Selection.activeTransform;
        Selection.activeTransform = holder;
        EditorApplication.ExecuteMenuItem("Animation Rigging/Bone Renderer Setup");
        if (!holder.TryGetComponent<BoneRenderer>(out _)) {
            var boneRenderer = holder.gameObject.AddComponent<BoneRenderer>();
            boneRenderer.boneSize = 0.01f;
            boneRenderer.transforms = holder.GetComponentsInChildren<Transform>();
        }
        Selection.activeTransform = previous;
#endif
        CreatureIkSetup.Build(transform, holder);
    }

    void DisposeOwned(Object target)
    {
        if (!target) return;
        if (Application.isPlaying) Destroy(target);
        else DestroyImmediate(target);
    }

    int SizeFingerprint()
    {
        unchecked {
            return basis.GetHashCode()
                ^ (limbLength.GetHashCode() * 31)
                ^ (endLength.GetHashCode() * 37)
                ^ (limbRadius.GetHashCode() * 41)
                ^ (endRadius.GetHashCode() * 43);
        }
    }
    /// <summary>
    /// 计算生物数据的指纹值，用于快速比较生物数据是否发生变化
    /// </summary>
    /// <param name="data">要计算指纹的生物数据</param>
    /// <returns>生物数据的指纹值</returns>
    static int Fingerprint(CreatureData data)
    {
        if (data?.parts == null) return 0;
        unchecked {
            int hash = data.parts.Count;
            foreach (CreaturePart part in data.parts) {
                hash = hash * 31 + part.id;
                hash = hash * 31 + (int)part.type;
                hash = hash * 31 + part.side;
                hash = hash * 31 + part.parentId;
                hash = hash * 31 + part.length.GetHashCode();
                hash = hash * 31 + part.radius.GetHashCode();
            }
            return hash;
        }
    }
}
