/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 腿用 Two Bone IK, 尾巴用 Chain IK, 触角用 Damped Transform
/// </summary>
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public static class CreatureIkSetup
{
    public static void Build(Transform creature, Transform skeleton)
    {
        if (creature.TryGetComponent(out RigBuilder builder))
            builder.layers.Clear();

        var rigObject = new GameObject("Rig");
        rigObject.transform.SetParent(creature, false);
        var rig = rigObject.AddComponent<Rig>();
        if (!creature.TryGetComponent(out builder))
            builder = creature.gameObject.AddComponent<RigBuilder>();
        builder.layers.Add(new RigLayer(rig));

        if (!creature.TryGetComponent(out Rigidbody body))
            body = creature.gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        if (!creature.TryGetComponent<AlignBody>(out _))
            creature.gameObject.AddComponent<AlignBody>();

        var legs = new List<FootTarget>();
        foreach (Transform limb in skeleton.GetComponentsInChildren<Transform>()) {
            if (!limb.name.StartsWith("Limb")) continue;
            var chain = new List<Transform> { limb };
            Transform cursor = limb;
            while (cursor.childCount > 0) {
                Transform next = null;
                for (int i = 0; i < cursor.childCount && next == null; i++) {
                    Transform child = cursor.GetChild(i);
                    if (child.name == "Tip" || child.name.StartsWith("End"))
                        next = child;
                }
                if (next == null) break;
                chain.Add(next);
                cursor = next;
            }
            if (chain.Count < 3) continue;

            Transform root = chain[0];
            Transform mid = chain[1];
            Transform tip = chain[chain.Count - 1];
            string side = tip.position.x < creature.position.x ? "L" : "R";
            string label = $"{side} {legs.Count}";

            var constraintObject = new GameObject($"Leg IK {label}");
            constraintObject.transform.SetParent(rigObject.transform, false);
            var ik = constraintObject.AddComponent<TwoBoneIKConstraint>();
            ik.data.root = root;
            ik.data.mid = mid;
            ik.data.tip = tip;
            ik.data.targetRotationWeight = 0f;

            float reach = Vector3.Distance(root.position, mid.position) + Vector3.Distance(mid.position, tip.position);
            Vector3 tipLocal = creature.InverseTransformPoint(tip.position);
            Vector3 rootLocal = creature.InverseTransformPoint(root.position);
            Vector2 flat = new Vector2(tipLocal.x - rootLocal.x, tipLocal.z - rootLocal.z);
            float lateral = flat.magnitude;
            float maxLateral = reach * 0.72f;
            if (lateral > maxLateral && lateral > 0.0001f) {
                flat *= maxLateral / lateral;
                tipLocal.x = rootLocal.x + flat.x;
                tipLocal.z = rootLocal.z + flat.y;
            }

            var targetObject = new GameObject($"Target {label}");
            targetObject.transform.SetParent(constraintObject.transform, false);
            var foot = targetObject.AddComponent<FootTarget>();
            foot.body = creature;
            foot.footLocal = tipLocal;
            foot.stepstance = Mathf.Max(0.08f, reach * 0.22f);
            foot.high = Mathf.Max(0.04f, reach * 0.08f);
            foot.rayHeight = Mathf.Max(1.5f, reach * 1.2f);
            targetObject.transform.position = creature.TransformPoint(foot.footLocal);
            ik.data.target = targetObject.transform;

            var hint = new GameObject($"Hint {label}");
            hint.transform.SetParent(constraintObject.transform, false);
            Vector3 bend = mid.position - (root.position + tip.position) * 0.5f;
            if (bend.sqrMagnitude < 0.0001f) bend = root.TransformDirection(Vector3.forward);
            hint.transform.position = mid.position + bend.normalized * Mathf.Max(0.1f, reach * 0.15f);
            ik.data.hint = hint.transform;
            legs.Add(foot);
        }

        if (legs.Count > 0) {
            float minZ = legs[0].footLocal.z;
            float maxZ = minZ;
            for (int i = 1; i < legs.Count; i++) {
                float z = legs[i].footLocal.z;
                if (z < minZ) minZ = z;
                if (z > maxZ) maxZ = z;
            }
            float span = maxZ - minZ;
            for (int i = 0; i < legs.Count; i++) {
                FootTarget foot = legs[i];
                float along = span > 0.0001f ? Mathf.InverseLerp(minZ, maxZ, foot.footLocal.z) : 0.5f;
                float bias = Mathf.Min(foot.stepstance * 0.55f, Mathf.Abs(foot.footLocal.x) * 0.12f);
                foot.footLocal.z += (along * 2f - 1f) * Mathf.Max(span * 0.08f, bias);
                foot.transform.position = creature.TransformPoint(foot.footLocal);
            }
        }

        legs.Sort((a, b) => {
            int along = b.footLocal.z.CompareTo(a.footLocal.z);
            return along != 0 ? along : a.footLocal.x.CompareTo(b.footLocal.x);
        });
        for (int i = 0; i < legs.Count; i++) {
            if (i > 0) legs[i].leg1 = legs[i - 1];
            if (i + 1 < legs.Count) legs[i].leg2 = legs[i + 1];
        }

        BuildTail(skeleton, rigObject.transform);
        BuildAntenna(skeleton, rigObject.transform);

        if (Application.isPlaying)
            builder.Build();
    }
    private static void BuildTail(Transform skeleton, Transform rigObject)
    {
        Transform tailRoot = null;
        foreach (Transform bone in skeleton.GetComponentsInChildren<Transform>()) {
            if (!bone.name.StartsWith("Tail")) continue;
            if (bone.parent == null || !bone.parent.name.StartsWith("Tail")) tailRoot = bone;
        }
        if (tailRoot == null) return;

        var chain = new List<Transform> { tailRoot };
        Transform cursor = tailRoot;
        while (cursor.childCount > 0) {
            Transform next = null;
            for (int i = 0; i < cursor.childCount && next == null; i++) {
                Transform child = cursor.GetChild(i);
                if (child.name.StartsWith("Tail") || child.name == "Tip") next = child;
            }
            if (next == null) break;
            chain.Add(next);
            cursor = next;
        }

        Transform root = chain[0];
        Transform tip = chain[chain.Count - 1];
        int links = 1;
        for (Transform node = tip; node != null && node != root; node = node.parent) links++;
        if (links < 3 && root.parent != null) {
            root = root.parent;
            links++;
        }
        if (links < 3) return;

        var constraintObject = new GameObject("Tail IK");
        constraintObject.transform.SetParent(rigObject, false);
        var ik = constraintObject.AddComponent<ChainIKConstraint>();
        ik.data.root = root;
        ik.data.tip = tip;
        ik.data.tipRotationWeight = 0f;
        var targetObject = new GameObject("Target");
        targetObject.transform.SetParent(constraintObject.transform, false);
        targetObject.transform.position = tip.position;
        ik.data.target = targetObject.transform;
    }
    private static void BuildAntenna(Transform skeleton, Transform rig)
    {
        foreach (Transform bone in skeleton.GetComponentsInChildren<Transform>()) {
            if (!bone.name.StartsWith("Antenna")) continue;
            var constraintObject = new GameObject($"Antenna Damp {bone.name}");
            constraintObject.transform.SetParent(rig, false);
            var damp = constraintObject.AddComponent<DampedTransform>();
            damp.data.constrainedObject = bone;
            damp.data.sourceObject = bone.parent;
            damp.data.maintainAim = true;
        }
    }
}
