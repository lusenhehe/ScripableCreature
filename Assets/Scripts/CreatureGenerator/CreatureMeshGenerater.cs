/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 由 CreatureData 生成骨骼网格. 每段自带长度和半径.
/// 腿在 Body 上插一个 Hip 再接 Limb; 多对腿沿体节铺开, 末段无腿鼓成腹部.
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CreatureSizeDefaults
{
    public float basis = 0.2f;
    public float limbLength = 1f;
    public float endLength = 1.2f;
    public float limbRadius = 0.055f;
    public float endRadius = 0.036f;
}

public struct CreaturePartMesh
{
    public int id;
    public int parentId;
    public NodeType type;
    public Mesh mesh;
    public Vector3 position;
    public Quaternion rotation;
    public float length;

    public Vector3 EndCenter => position + rotation * Vector3.up * length;
}

public static class CreatureMeshGenerater
{
    public static List<CreaturePartMesh> Build(CreatureData data, CreatureSizeDefaults size = null)
    {
        size ??= new CreatureSizeDefaults();
        List<CreaturePart> parts = data.parts;
        var children = new Dictionary<int, List<int>>();
        int root = -1;
        for (int i = 0; i < parts.Count; i++) {
            CreaturePart part = parts[i];
            if (part.parentId < 0) {
                if (root < 0) root = i;
                continue;
            }
            if (!children.TryGetValue(part.parentId, out List<int> list))
                children[part.parentId] = list = new List<int>();
            list.Add(i);
        }

        var result = new List<CreaturePartMesh>(parts.Count);
        if (root >= 0) Place(parts, children, root, Vector3.zero, Vector3.back, size, result, 0.12f, -1);
        return result;
    }

    private static void Place(List<CreaturePart> parts, Dictionary<int, List<int>> children, int index, Vector3 start, Vector3 direction, CreatureSizeDefaults size, List<CreaturePartMesh> result, float legForward, int parentBoneId)
    {
        CreaturePart part = parts[index];
        children.TryGetValue(part.id, out List<int> kids);
        int limbs = Count(parts, kids, NodeType.Limb);
        int pairs = (limbs + 1) / 2;
        bool abdomen = part.type == NodeType.Body && part.parentId >= 0 && IsAbdomen(parts, kids);
        Size(part, pairs, abdomen, size, out float startRadius, out float endRadius, out float length, out float[] stations, out float[] radii);

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        Vector3 end = start + rotation * Vector3.up * length;
        result.Add(new CreaturePartMesh {
            id = part.id,
            parentId = parentBoneId,
            type = part.type,
            mesh = Tube(stations, radii),
            position = start,
            rotation = rotation,
            length = length
        });

        if (kids == null) return;
        int limbIndex = 0;
        foreach (int childIndex in kids) {
            CreaturePart child = parts[childIndex];
            if (child.type == NodeType.Antenna && part.type == NodeType.Head) {
                Place(parts, children, childIndex, AntennaRoot(start, rotation, length, size.basis, child.side), Direction(child, legForward), size, result, legForward, part.id);
                continue;
            }
            if (child.type == NodeType.Limb) {
                float forward = legForward;
                float along = 0.5f;
                if (pairs > 1) {
                    along = (limbIndex / 2) / (float)(pairs - 1);
                    forward = Mathf.Lerp(0.75f, -0.65f, along);
                }
                limbIndex++;
                float t = pairs > 1 ? Mathf.Lerp(0.72f, 1f, along) : 1f;
                Vector3 attach = Vector3.Lerp(start, end, t);
                Vector3 dir = Direction(child, forward);
                if (part.type == NodeType.Body) {
                    float capsule = Mathf.Lerp(startRadius, endRadius, t);
                    if (stations != null && radii != null)
                        capsule = RadiusAt(t, length, stations, radii);
                    const float HipCube = 0.02f;
                    float hipLength = Mathf.Max(capsule, size.basis * 0.35f);
                    int hipId = -1000 - child.id;
                    Quaternion hipRotation = Quaternion.FromToRotation(Vector3.up, dir);
                    result.Add(new CreaturePartMesh {
                        id = hipId,
                        parentId = part.id,
                        type = NodeType.Hip,
                        mesh = Cube(HipCube),
                        position = attach,
                        rotation = hipRotation,
                        length = hipLength
                    });
                    Place(parts, children, childIndex, attach + hipRotation * Vector3.up * hipLength, dir, size, result, forward, hipId);
                } else {
                    Place(parts, children, childIndex, attach, dir, size, result, forward, part.id);
                }
                continue;
            }
            Place(parts, children, childIndex, child.type == NodeType.Head ? start : end, Direction(child, legForward), size, result, legForward, part.id);
        }
    }

    private static void Size(CreaturePart part, int pairs, bool abdomen, CreatureSizeDefaults size, out float startRadius, out float endRadius, out float length, out float[] stations, out float[] radii)
    {
        bool trunk = part.type == NodeType.Body && pairs > 1;
        float basis = size.basis;

        startRadius = part.radius > 0f ? part.radius : DefaultRadius(part, trunk, abdomen, size);
        endRadius = startRadius * DefaultTaper(part.type, trunk, abdomen, part.parentId < 0);
        length = part.length > 0f ? part.length : DefaultLength(part, trunk, abdomen, pairs, startRadius, endRadius, size);

        if (part.type == NodeType.Head) {
            float cranium = basis * 0.95f;
            stations = new[] { 0f, length * 0.3f, length * 0.52f, length };
            radii = new[] { startRadius, cranium, cranium, endRadius };
            return;
        }
        if (abdomen) {
            float belly = startRadius * 1.35f;
            endRadius = startRadius * 0.55f;
            stations = new[] { 0f, length * 0.5f, length };
            radii = new[] { startRadius, belly, endRadius };
            return;
        }
        if (trunk && part.parentId < 0) {
            float thick = basis * 0.9f;
            endRadius = thick * 0.92f;
            stations = new[] { 0f, length * 0.22f, length };
            radii = new[] { startRadius, thick, endRadius };
            return;
        }
        stations = new[] { 0f, length };
        radii = new[] { startRadius, endRadius };
    }

    private static float DefaultRadius(CreaturePart part, bool trunk, bool abdomen, CreatureSizeDefaults size) => part.type switch {
        NodeType.Head => size.basis * 0.42f,
        NodeType.Body when part.parentId < 0 && trunk => size.basis * 0.42f,
        NodeType.Body when part.parentId < 0 => size.basis * 0.42f,
        NodeType.Body when abdomen => size.basis * 0.85f,
        NodeType.Body => size.basis * 0.85f,
        NodeType.Limb => size.limbRadius,
        NodeType.End => size.endRadius,
        NodeType.Tail => size.basis * 0.55f,
        NodeType.Antenna => size.basis * 0.1f,
        _ => size.basis * 0.4f
    };

    private static float DefaultTaper(NodeType type, bool trunk, bool abdomen, bool root) => type switch {
        NodeType.Head => 0.52f,
        NodeType.Body when root && trunk => 2.2f,
        NodeType.Body when root => 2.35f,
        NodeType.Body when abdomen => 0.55f,
        NodeType.Body => 0.94f,
        NodeType.Limb => 0.65f,
        NodeType.End => 0.55f,
        NodeType.Tail => 0.72f,
        NodeType.Antenna => 0.72f,
        _ => 0.7f
    };

    private static float DefaultLength(CreaturePart part, bool trunk, bool abdomen, int pairs, float startRadius, float endRadius, CreatureSizeDefaults size)
    {
        if (part.type == NodeType.Limb) return size.limbLength;
        if (part.type == NodeType.End) return size.endLength;
        if (part.type == NodeType.Head) return size.basis * 2.5f;
        if (part.type == NodeType.Body && trunk) return size.basis * (1.05f + 0.22f * pairs);
        if (abdomen) return startRadius * 2.4f;
        if (part.type == NodeType.Antenna) return size.basis * 0.85f;
        float span = part.type switch {
            NodeType.Body => 3.2f,
            NodeType.Tail => 2.6f,
            _ => 2f
        };
        return Mathf.Max(startRadius, endRadius) * span;
    }

    private static int Count(List<CreaturePart> parts, List<int> kids, NodeType type)
    {
        if (kids == null) return 0;
        int count = 0;
        foreach (int child in kids)
            if (parts[child].type == type) count++;
        return count;
    }

    private static bool IsAbdomen(List<CreaturePart> parts, List<int> kids)
    {
        if (kids == null) return true;
        foreach (int child in kids) {
            NodeType type = parts[child].type;
            if (type == NodeType.Body || type == NodeType.Limb) return false;
        }
        return true;
    }

    private static float RadiusAt(float t, float length, float[] stations, float[] radii)
    {
        float y = Mathf.Clamp01(t) * length;
        for (int i = 0; i < stations.Length - 1; i++) {
            if (y > stations[i + 1] && i < stations.Length - 2) continue;
            float u = Mathf.InverseLerp(stations[i], stations[i + 1], y);
            return Mathf.Lerp(radii[i], radii[i + 1], u);
        }
        return radii[^1];
    }

    private static Vector3 AntennaRoot(Vector3 start, Quaternion rotation, float length, float basis, int side)
    {
        Axes(rotation, out Vector3 forward, out Vector3 right, out Vector3 up);
        float lateral = side == 0 ? 1f : side;
        return start + forward * (length * 0.28f) + up * basis * 0.78f + right * lateral * basis * 0.2f;
    }

    private static void Axes(Quaternion rotation, out Vector3 forward, out Vector3 right, out Vector3 up)
    {
        forward = rotation * Vector3.up;
        right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude < 0.0001f) right = Vector3.right;
        right.Normalize();
        up = Vector3.Cross(forward, right);
    }

    private static Vector3 Direction(CreaturePart child, float legForward)
    {
        float side = child.side;
        if (child.type == NodeType.Head) return new Vector3(0f, 0.25f, 1f).normalized;
        if (child.type == NodeType.Body) return Vector3.back;
        if (child.type == NodeType.Tail) return new Vector3(0f, -0.4f, -1f).normalized;
        if (child.type == NodeType.Limb) return new Vector3(side, -0.55f, legForward).normalized;
        if (child.type == NodeType.Antenna) {
            float lateral = side == 0f ? 1f : side;
            return new Vector3(lateral * 0.45f, 1f, -0.2f).normalized;
        }
        return new Vector3(side * 0.35f, -1f, legForward * 0.55f).normalized;
    }

    private static Mesh Tube(float[] y, float[] radius)
    {
        const int Sides = 12;
        const int Steps = 4;
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        int AddPole(Vector3 center, float poleRadius, float poleY)
        {
            int pole = vertices.Count;
            var normal = new Vector3(0f, poleY, 0f);
            normals.Add(normal);
            vertices.Add(center + normal * poleRadius);
            return pole;
        }

        int AddRing(Vector3 center, float ringRadius, float phi)
        {
            int ring = vertices.Count;
            float sine = Mathf.Sin(phi);
            float horizontal = Mathf.Cos(phi);
            for (int i = 0; i < Sides; i++) {
                float angle = i * Mathf.PI * 2f / Sides;
                var normal = new Vector3(horizontal * Mathf.Cos(angle), sine, horizontal * Mathf.Sin(angle));
                normals.Add(normal);
                vertices.Add(center + normal * ringRadius);
            }
            return ring;
        }

        int AddFrustumRing(float station, float ringRadius, float normalY, float normalRadial)
        {
            int ring = vertices.Count;
            for (int i = 0; i < Sides; i++) {
                float angle = i * Mathf.PI * 2f / Sides;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                normals.Add(new Vector3(cosine * normalRadial, normalY, sine * normalRadial));
                vertices.Add(new Vector3(cosine * ringRadius, station, sine * ringRadius));
            }
            return ring;
        }

        void Link(int lower, int upper)
        {
            for (int i = 0; i < Sides; i++) {
                int next = (i + 1) % Sides;
                triangles.Add(lower + i);
                triangles.Add(upper + i);
                triangles.Add(lower + next);
                triangles.Add(lower + next);
                triangles.Add(upper + i);
                triangles.Add(upper + next);
            }
        }

        void Fan(int pole, int ring, bool north)
        {
            for (int i = 0; i < Sides; i++) {
                int next = (i + 1) % Sides;
                triangles.Add(pole);
                triangles.Add(ring + (north ? next : i));
                triangles.Add(ring + (north ? i : next));
            }
        }

        int south = AddPole(Vector3.zero, radius[0], -1f);
        int previous = 0;
        for (int step = 1; step <= Steps; step++) {
            int ring = AddRing(Vector3.zero, radius[0], -Mathf.PI * 0.5f * (Steps - step) / Steps);
            if (step == 1) Fan(south, ring, false);
            else Link(previous, ring);
            previous = ring;
        }

        for (int station = 0; station < y.Length - 1; station++) {
            float y0 = y[station];
            float y1 = y[station + 1];
            float r0 = radius[station];
            float r1 = radius[station + 1];
            float delta = r1 - r0;
            float height = y1 - y0;
            float slope = Mathf.Max(0.0001f, Mathf.Sqrt(height * height + delta * delta));
            Link(AddFrustumRing(y0, r0, -delta / slope, height / slope), AddFrustumRing(y1, r1, -delta / slope, height / slope));
        }

        var top = new Vector3(0f, y[^1], 0f);
        previous = AddRing(top, radius[^1], 0f);
        for (int step = 1; step < Steps; step++) {
            int ring = AddRing(top, radius[^1], Mathf.PI * 0.5f * step / Steps);
            Link(previous, ring);
            previous = ring;
        }
        Fan(AddPole(top, radius[^1], 1f), previous, true);

        var mesh = new Mesh { name = "Bone" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh Cube(float edge)
    {
        float h = edge * 0.5f;
        var vertices = new Vector3[] {
            new(-h, 0f, -h), new(h, 0f, -h), new(h, 0f, h), new(-h, 0f, h),
            new(-h, edge, -h), new(h, edge, -h), new(h, edge, h), new(-h, edge, h)
        };
        var normals = new Vector3[] {
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            Vector3.up, Vector3.up, Vector3.up, Vector3.up,
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward,
            Vector3.left, Vector3.left, Vector3.left, Vector3.left,
            Vector3.right, Vector3.right, Vector3.right, Vector3.right
        };
        var corners = new Vector3[] {
            vertices[0], vertices[1], vertices[2], vertices[3],
            vertices[4], vertices[7], vertices[6], vertices[5],
            vertices[0], vertices[4], vertices[5], vertices[1],
            vertices[3], vertices[2], vertices[6], vertices[7],
            vertices[0], vertices[3], vertices[7], vertices[4],
            vertices[1], vertices[5], vertices[6], vertices[2]
        };
        var triangles = new int[36];
        for (int face = 0; face < 6; face++) {
            int i = face * 4;
            int t = face * 6;
            triangles[t] = i;
            triangles[t + 1] = i + 1;
            triangles[t + 2] = i + 2;
            triangles[t + 3] = i;
            triangles[t + 4] = i + 2;
            triangles[t + 5] = i + 3;
        }
        var mesh = new Mesh { name = "Hip" };
        mesh.SetVertices(corners);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
