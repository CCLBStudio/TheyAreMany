using UnityEngine;

namespace CCLBStudio.Utils
{
    public static class VectorExtender
    {
        public static Vector2 ApplyRotation(this Vector2 vector, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
        
            return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
        }

        public static Vector3 ApplyRotation(this Vector3 vector, float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            float newX = cos * vector.x - sin * vector.y;
            float newY = sin * vector.x + cos * vector.y;

            return new Vector3(newX, newY, vector.z);
        }

        public static Vector3 Random01()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }
    }
}