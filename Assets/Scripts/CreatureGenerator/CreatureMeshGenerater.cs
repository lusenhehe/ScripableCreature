/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 由 CreatureData 生成骨骼网格. 每段是半球 + 圆棱台 + 半球,
/// 局部 +Y 从起始球心指向末端球心. 子段起始半径等于父段末端半径, 段内按部位收细.
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public struct CreaturePartMesh
{
    public int id;
    public NodeType type;
    public Mesh mesh;
    public Vector3 position;
    public Quaternion rotation;
    public float length;

    public Vector3 EndCenter => position + rotation * Vector3.up * length;
}

public static class CreatureMeshGenerater
{
    public static List<CreaturePartMesh> Build(CreatureData data, float radius = 0.2f)
    {
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
        if (root >= 0) Place(parts, children, root, Vector3.zero, Vector3.back, radius, NodeType.Body, result);
        return result;
    }

    private static void Place(List<CreaturePart> parts, Dictionary<int, List<int>> children, int index, Vector3 start, Vector3 direction, float startRadius, NodeType parentType, List<CreaturePartMesh> result)
    {
        CreaturePart part = parts[index];
        float endRadius = startRadius * Taper(part.type, parentType);
        float length = startRadius * Span(part.type);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        Vector3 end = start + rotation * Vector3.up * length;
        result.Add(new CreaturePartMesh {
            id = part.id,
            type = part.type,
            mesh = Bone(startRadius, endRadius, length),
            position = start,
            rotation = rotation,
            length = length
        });

        if (!children.TryGetValue(part.id, out List<int> kids)) return;
        int antenna = 0;
        foreach (int childIndex in kids) {
            CreaturePart child = parts[childIndex];
            bool atStart = child.type == NodeType.Head;
            Place(parts, children, childIndex, atStart ? start : end, Direction(part.type, child, ref antenna), atStart ? startRadius : endRadius, part.type, result);
        }
    }

    private static Vector3 Direction(NodeType parent, CreaturePart child, ref int antenna)
    {
        float side = child.side;
        if (child.type == NodeType.Head) return new Vector3(0f, 0.25f, 1f).normalized;
        if (child.type == NodeType.Body) return Vector3.back;
        if (child.type == NodeType.Tail) return new Vector3(0f, -0.4f, -1f).normalized;
        if (child.type == NodeType.Limb) return new Vector3(side, -0.55f, 0.15f).normalized;
        if (child.type == NodeType.Joint) return new Vector3(side * 0.25f, -1f, 0.1f).normalized;
        if (parent != NodeType.Head) return new Vector3(side * 0.1f, -1f, 0.35f).normalized;
        float spread = 0.4f + 0.3f * (antenna / 2);
        antenna++;
        return new Vector3(side * spread, 1f, 0.2f).normalized;
    }

    private static float Taper(NodeType type, NodeType parent) => type switch {
        NodeType.Body => 0.94f,
        NodeType.Head => 0.82f,
        NodeType.Limb => 0.5f,
        NodeType.Joint => 0.85f,
        NodeType.Tail => 0.78f,
        NodeType.End => parent == NodeType.Head ? 0.3f : 0.6f,
        _ => 1f
    };

    private static float Span(NodeType type) => type switch {
        NodeType.Body => 3.2f,
        NodeType.Head => 2.6f,
        NodeType.Limb => 2.4f,
        NodeType.Joint => 2.3f,
        NodeType.Tail => 2.6f,
        _ => 1.8f
    };

    private static Mesh Bone(float startRadius, float endRadius, float length)
    {
        const int Sides = 12;
        const int Steps = 4;
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var triangles = new List<int>();

        int AddPole(Vector3 center, float radius, float y)
        {
            int index = vertices.Count;
            var normal = new Vector3(0f, y, 0f);
            normals.Add(normal);
            vertices.Add(center + normal * radius);
            return index;
        }

        int AddRing(Vector3 center, float radius, float phi)
        {
            int start = vertices.Count;
            float y = Mathf.Sin(phi);
            float horizontal = Mathf.Cos(phi);
            for (int i = 0; i < Sides; i++) {
                float angle = i * Mathf.PI * 2f / Sides;
                var normal = new Vector3(horizontal * Mathf.Cos(angle), y, horizontal * Mathf.Sin(angle));
                normals.Add(normal);
                vertices.Add(center + normal * radius);
            }
            return start;
        }

        int AddFrustumRing(float y, float radius, float normalY, float normalRadial)
        {
            int start = vertices.Count;
            for (int i = 0; i < Sides; i++) {
                float angle = i * Mathf.PI * 2f / Sides;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                normals.Add(new Vector3(cosine * normalRadial, normalY, sine * normalRadial));
                vertices.Add(new Vector3(cosine * radius, y, sine * radius));
            }
            return start;
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

        var bottom = Vector3.zero;
        var top = new Vector3(0f, length, 0f);
        int south = AddPole(bottom, startRadius, -1f);
        int previous = 0;
        for (int step = 1; step <= Steps; step++) {
            int ring = AddRing(bottom, startRadius, -Mathf.PI * 0.5f * (Steps - step) / Steps);
            if (step == 1) Fan(south, ring, false);
            else Link(previous, ring);
            previous = ring;
        }

        float delta = endRadius - startRadius;
        float slope = Mathf.Sqrt(length * length + delta * delta);
        Link(AddFrustumRing(0f, startRadius, -delta / slope, length / slope), AddFrustumRing(length, endRadius, -delta / slope, length / slope));

        previous = AddRing(top, endRadius, 0f);
        for (int step = 1; step < Steps; step++) {
            int ring = AddRing(top, endRadius, Mathf.PI * 0.5f * step / Steps);
            Link(previous, ring);
            previous = ring;
        }
        Fan(AddPole(top, endRadius, 1f), previous, true);

        var mesh = new Mesh { name = "Bone" };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
