/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 身体贴合地面法线
/// </summary>
using UnityEngine;

public class AlignBody : MonoBehaviour
{
    public LayerMask terrainLayer = ~0;

    void Update()
    {
        Vector3 origin = transform.position + transform.up;
        if (!Physics.Raycast(origin, transform.TransformDirection(Vector3.down), out RaycastHit hit, 50f, terrainLayer))
            return;
        Quaternion next = Quaternion.LookRotation(
            Vector3.Cross(hit.normal, Vector3.Cross(transform.forward, hit.normal)),
            hit.normal);
        GetComponent<Rigidbody>().MoveRotation(Quaternion.Lerp(transform.rotation, next, 0.1f));
    }
}
