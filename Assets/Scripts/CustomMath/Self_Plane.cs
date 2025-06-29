namespace CustomMath
{
    public struct Self_Plane
    {
        #region Variables

        private Vec3 normal;
        private float distance;

        public Vec3 Normal
        {
            get { return normal.normalizedVec3; }
        }

        public float Distance
        {
            get { return distance; }
        }

        public Self_Plane flipped
        {
            get { return new Self_Plane(normal * -1, -distance); }
        }

        #endregion

        #region Constructors

        public Self_Plane(Vec3 inNormal, Vec3 inPoint)
        {
            normal = inNormal;
            distance = Vec3.Dot(inNormal, inPoint) / normal.magnitude;
        }

        public Self_Plane(Vec3 inNormal, float d)
        {
            normal = inNormal;
            distance = d;
        }

        public Self_Plane(Vec3 a, Vec3 b, Vec3 c)
        {
            //https://math.stackexchange.com/questions/1034568/find-the-plane-a-triangle-lies-on
            normal = Vec3.Cross(b - a, c - a);
            Vec3 triangleCenter = (a + b + c) / 3;
            distance = Vec3.Dot(normal, triangleCenter) / -normal.magnitude;
        }

        #endregion

        #region Functions

        public static Self_Plane Translate(Self_Plane plane, Vec3 translation)
        {
            float newPlaneDistance = plane.distance + Vec3.Dot(plane.normal, translation) / plane.normal.magnitude;
            return new Self_Plane(plane.normal, newPlaneDistance);
        }

        public Vec3 ClosestPointOnPlane(Vec3 point)
        {
            Vec3 normalizedNormal = normal.normalizedVec3;

            float t = GetDistanceToPoint(point) / -Vec3.Dot(normalizedNormal, normalizedNormal);
            return new Vec3(normalizedNormal.x * t + point.x, normalizedNormal.y * t + point.y, normalizedNormal.z * t + point.z);
        }

        public void Flip()
        {
            normal *= -1;
            distance *= -1;
        }

        public float GetDistanceToPoint(Vec3 point)
        {
            return (Vec3.Dot(normal, point) + (distance * normal.magnitude)) / normal.magnitude;
        }

        public bool GetSide(Vec3 point)
        {
            return Vec3.Dot(normal, (point - (normal * distance))) > 0;
        }

        public bool SameSide(Vec3 inPt0, Vec3 inPt1)
        {
            return GetSide(inPt0) == GetSide(inPt1);
        }

        public void Set3Points(Vec3 a, Vec3 b, Vec3 c)
        {
            normal = Vec3.Cross(b - a, c - a);
            Vec3 triangleCenter = (a + b + c) / 3;
            distance = Vec3.Dot(normal, triangleCenter) / -normal.magnitude;
        }

        public void SetNormalAndPosition(Vec3 inNormal, Vec3 inPoint)
        {
            normal = inNormal;
            distance = Vec3.Dot(inNormal, inPoint) / -normal.magnitude;
        }

        public void Translate(Vec3 translation)
        {
            distance += Vec3.Dot(normal, translation) / normal.magnitude;
        }

        #endregion
    }
}