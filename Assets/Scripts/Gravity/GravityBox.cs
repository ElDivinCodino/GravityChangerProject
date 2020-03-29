using UnityEngine;

public class GravityBox : GravitySource
{
    [SerializeField, Min(0f)]
    float gravityIntensity = 9.81f;

    public bool isGravityCentripetal;

    [SerializeField]
    Vector3 boundaryDistance = Vector3.one;

    [SerializeField, Min(0f)]
    float innerDistance = 0f, innerFalloffDistance = 0f;

    [SerializeField, Min(0f)]
    float outerDistance = 0f, outerFalloffDistance = 0f;

    float innerFalloffFactor, outerFalloffFactor;

    void Awake()
    {
        OnValidate();
        gravityIntensity = isGravityCentripetal ? gravityIntensity : -gravityIntensity;
    }

    void OnValidate()
    {
        boundaryDistance = Vector3.Max(boundaryDistance, Vector3.zero);
        float maxInner = Mathf.Min(Mathf.Min(boundaryDistance.x, boundaryDistance.y), boundaryDistance.z);
        innerDistance = Mathf.Min(innerDistance, maxInner);
        innerFalloffDistance = Mathf.Max(Mathf.Min(innerFalloffDistance, maxInner), innerDistance);
        outerFalloffDistance = Mathf.Max(outerFalloffDistance, outerDistance);

        innerFalloffFactor = 1f / (innerFalloffDistance - innerDistance);
        outerFalloffFactor = 1f / (outerFalloffDistance - outerDistance);
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        // Make the position relative to the box's position (InverseTransformDirection to support cubes with arbitrary rotation)
        position = transform.InverseTransformDirection(position - transform.position);

        Vector3 vector = Vector3.zero;

        // Determine whether the given position lies inside or outside the box. This is done by determining it per dimension and counting along how many we end up outside.
        // Check if the position lies beyond the right face
        int outside = 0;
        if (position.x > boundaryDistance.x)
        {
            vector.x = boundaryDistance.x - position.x;
            outside = 1;
        }
        // If it is not beyond the right face, check if it is beyond the left face instead
        else if (position.x < -boundaryDistance.x)
        {
            vector.x = -boundaryDistance.x - position.x;
            outside = 1;
        }
        // Do the same for y an z
        if (position.y > boundaryDistance.y)
        {
            vector.y = boundaryDistance.y - position.y;
            outside += 1;
        }
        else if (position.y < -boundaryDistance.y)
        {
            vector.y = -boundaryDistance.y - position.y;
            outside += 1;
        }

        if (position.z > boundaryDistance.z)
        {
            vector.z = boundaryDistance.z - position.z;
            outside += 1;
        }
        else if (position.z < -boundaryDistance.z)
        {
            vector.z = -boundaryDistance.z - position.z;
            outside += 1;
        }

        // If position is outside at least one face, the distance to the boundary is equal to the length of the adjusted vector.
        if (outside > 0)
        {
            float distance = outside == 1 ? Mathf.Abs(vector.x + vector.y + vector.z) : vector.magnitude;
            if (distance > outerFalloffDistance)
            {
                return Vector3.zero;
            }
            float g = gravityIntensity / distance;
            if (distance > outerDistance)
            {
                g *= 1f - (distance - outerDistance) * outerFalloffFactor;
            }
            return transform.TransformDirection(g * vector);
        }

        // Calculate the absolute distances from the center
        Vector3 distances;
        distances.x = boundaryDistance.x - Mathf.Abs(position.x);
        distances.y = boundaryDistance.y - Mathf.Abs(position.y);
        distances.z = boundaryDistance.z - Mathf.Abs(position.z);

        // Get the smallest distance, and assign the result to the appropriate component of the vector.
        if (distances.x < distances.y)
        {
            if (distances.x < distances.z)
            {
                vector.x = GetGravityComponent(position.x, distances.x);
            }
            else
            {
                vector.z = GetGravityComponent(position.z, distances.z);
            }
        }
        else if (distances.y < distances.z)
        {
            vector.y = GetGravityComponent(position.y, distances.y);
        }
        else
        {
            vector.z = GetGravityComponent(position.z, distances.z);
        }
        return transform.TransformDirection(vector); // (TransformDirection instead of simply vector to support cubes with arbitrary rotation)
    }

    // First parameter is the relevant position coordinate relative to the box's center. The second is the distance to the nearest face along the relevant axis.
    float GetGravityComponent(float coordinate, float distance)
    {
        if (distance > innerFalloffDistance)
        {
            return 0f;
        }

        float g = gravityIntensity;

        if (distance > innerDistance)
        {
            // Linearly decrease the gravity for gravity falloff
            g *= 1f - (distance - innerDistance) * innerFalloffFactor;
        }

        // flip gravity if the coordinate is less than zero, because that means player is on the opposite side of the center.
        return coordinate > 0f ? -g : g;
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 size;
        if (innerFalloffDistance > innerDistance)
        {
            Gizmos.color = Color.cyan;
            size.x = 2f * (boundaryDistance.x - innerFalloffDistance);
            size.y = 2f * (boundaryDistance.y - innerFalloffDistance);
            size.z = 2f * (boundaryDistance.z - innerFalloffDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
        if (innerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            size.x = 2f * (boundaryDistance.x - innerDistance);
            size.y = 2f * (boundaryDistance.y - innerDistance);
            size.z = 2f * (boundaryDistance.z - innerDistance);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, 2f * boundaryDistance);

        if (outerDistance > 0f)
        {
            Gizmos.color = Color.yellow;
            DrawGizmosOuterCube(outerDistance);
        }
        if (outerFalloffDistance > outerDistance)
        {
            Gizmos.color = Color.cyan;
            DrawGizmosOuterCube(outerFalloffDistance);
        }
    }

    void DrawGizmosRect(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    // Draws an outer cube, given a distance
    void DrawGizmosOuterCube(float distance)
    {
        Vector3 a, b, c, d;

        // Draw a square for the right face at the appropriate distance
        a.y = b.y = boundaryDistance.y;
        d.y = c.y = -boundaryDistance.y;
        b.z = c.z = boundaryDistance.z;
        d.z = a.z = -boundaryDistance.z;
        a.x = b.x = c.x = d.x = boundaryDistance.x + distance;
        DrawGizmosRect(a, b, c, d);

        // Draw the left face
        a.x = b.x = c.x = d.x = -a.x;
        DrawGizmosRect(a, b, c, d);

        // Repeat for the other 4 faces
        a.x = d.x = boundaryDistance.x;
        b.x = c.x = -boundaryDistance.x;
        a.z = b.z = boundaryDistance.z;
        c.z = d.z = -boundaryDistance.z;
        a.y = b.y = c.y = d.y = boundaryDistance.y + distance;
        DrawGizmosRect(a, b, c, d);
        a.y = b.y = c.y = d.y = -a.y;
        DrawGizmosRect(a, b, c, d);

        a.x = d.x = boundaryDistance.x;
        b.x = c.x = -boundaryDistance.x;
        a.y = b.y = boundaryDistance.y;
        c.y = d.y = -boundaryDistance.y;
        a.z = b.z = c.z = d.z = boundaryDistance.z + distance;
        DrawGizmosRect(a, b, c, d);
        a.z = b.z = c.z = d.z = -a.z;
        DrawGizmosRect(a, b, c, d);

        // Add a single additional wireframe cube between the rounded corner points of the cube
        distance *= 0.5773502692f;
        Vector3 size = boundaryDistance;
        size.x = 2f * (size.x + distance);
        size.y = 2f * (size.y + distance);
        size.z = 2f * (size.z + distance);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}