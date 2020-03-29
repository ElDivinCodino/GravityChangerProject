using UnityEngine;

public class GravitySphere : GravitySource
{
    [SerializeField, Min(0f)]
    float gravityIntensity = 9.81f;

    public bool isGravityCentripetal;

    [Header("Centripetal force")]
    [Tooltip("Maximum radius of maximum influence of sphere.")]
    [SerializeField, Min(0f)]
    float outerRadius = 10f;
    
    [Tooltip("Maximum radius of influence. After, sphere has 0 influence.")]
    [SerializeField, Min(0f)]
    float outerFalloffRadius = 15f;

    [Header("Centrifugal force")]
    [Tooltip("Maximum radius of maximum influence of sphere.")]
    [SerializeField, Min(0f)]
    float innerRadius = 5f;
    
    [Tooltip("Maximum radius of influence. After, sphere has 0 influence.")]
    [SerializeField, Min(0f)]
    float innerFalloffRadius = 1f;

    float outerFalloffFactor, innerFalloffFactor;

    void Awake()
    {
        OnValidate();
        gravityIntensity = isGravityCentripetal ? gravityIntensity : -gravityIntensity;
    }

    void OnValidate()
    {
        innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
        innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);

        outerRadius = Mathf.Max(outerRadius, innerRadius);
        outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

        innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 vector = transform.position - position;
        float distance = vector.magnitude;
        
        if (distance > outerFalloffRadius || distance < innerFalloffRadius)
        {
            return Vector3.zero;
        }

        float g = gravityIntensity / distance;

        if (distance > outerRadius)
        {
            // Linearly decrease the gravity for gravity falloff
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }
        else if (distance < innerRadius)
        {
            // Linearly decrease the gravity for gravity falloff
            g *= 1f - (innerRadius - distance) * innerFalloffFactor;
        }

        return g * vector;
    }

    void OnDrawGizmos()
    {
        Vector3 p = transform.position;

        if (innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, innerFalloffRadius);
        }

        Gizmos.color = Color.yellow;

        if (innerRadius > 0f && innerRadius < outerRadius)
        {
            Gizmos.DrawWireSphere(p, innerRadius);
        }

        Gizmos.DrawWireSphere(p, outerRadius);

        if (outerFalloffRadius > outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }
}