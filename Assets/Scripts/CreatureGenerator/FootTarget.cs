/// <author>lusen</author>
/// <date>2026-11-06</date>
/// <summary>
/// 腿的落点. 射线沿身体下方检测地面, 相邻腿落地后才迈步.
/// </summary>
using UnityEngine;

[DefaultExecutionOrder(200)]
public class FootTarget : MonoBehaviour
{
    public Transform body;
    public LayerMask terrainLayer = ~0;
    public Vector3 footLocal;
    public float rayHeight = 2f;
    public float stepstance = 0.35f;
    public float high = 0.1f;
    public float speed = 2f;
    public FootTarget leg1, leg2;

    Vector3 newposition, oldposition, currentposition;
    float lerp = 1f;

    public bool Grounded => lerp >= 1f;

    private void Start()
    {
        newposition = transform.position;
        currentposition = transform.position;
    }

    void Update()
    {
        transform.position = currentposition;
        Vector3 origin = body.TransformPoint(footLocal) + body.up * rayHeight;
        if (Physics.Raycast(origin, -body.up, out RaycastHit info, rayHeight + 8f, terrainLayer))
        {
            float gap = Vector3.Distance(newposition, info.point);
            bool othersGrounded = (leg1 == null || leg1.Grounded) && (leg2 == null || leg2.Grounded);
            if (Grounded && gap > stepstance && (othersGrounded || gap > stepstance * 2f))
            {
                lerp = 0f;
                oldposition = currentposition;
                newposition = info.point;
            }
            else if (!Grounded)
                newposition = info.point;
        }

        if (lerp < 1f)
        {
            float span = Mathf.Max(stepstance, Vector3.Distance(oldposition, newposition));
            lerp += Time.deltaTime * speed * span / Mathf.Max(stepstance, 0.01f);
            float t = Mathf.Clamp01(lerp);
            Vector3 footposition = Vector3.Lerp(oldposition, newposition, t);
            footposition += body.up * Mathf.Sin(t * Mathf.PI) * high;
            currentposition = footposition;
        }
        else
            oldposition = newposition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newposition, 0.05f);
    }
}
