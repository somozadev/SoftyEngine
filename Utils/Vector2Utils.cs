using System;
using SFML.System;

namespace Softine.Utils
{
    public static class Vector2Utils
    {
        // Zero and One Vectors for Vector2f
        public static readonly Vector2f ZeroVectorF = new Vector2f(0f, 0f);
        public static readonly Vector2f OneVectorF = new Vector2f(1f, 1f);
        public static readonly Vector2f UpVectorF = new Vector2f(0f, 1f);
        public static readonly Vector2f DownVectorF = new Vector2f(0f, -1f);
        public static readonly Vector2f LeftVectorF = new Vector2f(-1f, 0f);
        public static readonly Vector2f RightVectorF = new Vector2f(1f, 0f);

        // Zero and One Vectors for Vector2i
        public static readonly Vector2i ZeroVectorI = new Vector2i(0, 0);
        public static readonly Vector2i OneVectorI = new Vector2i(1, 1);

        public static float AngleBetweenVectors(Vector2f v1, Vector2f v2)
        {
            float dotProduct = v1.X * v2.X + v1.Y * v2.Y;
            float magnitudeV1 = MathF.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
            float magnitudeV2 = MathF.Sqrt(v2.X * v2.X + v2.Y * v2.Y);
            if (magnitudeV1 == 0 || magnitudeV2 == 0)
                return 0;
            float cosTheta = dotProduct / (magnitudeV1 * magnitudeV2);
            cosTheta = MathUtils.Clamp(cosTheta, -1f, 1f);
            return MathF.Acos(cosTheta);
        }
        public static Vector2f Perpendicular(Vector2f v)
        {
            return new Vector2f(-v.Y, v.X);
        }
        
        #region Distance
        public static float Distance(Vector2f a, Vector2f b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float Distance(Vector2i a, Vector2i b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        #endregion

        #region Dot Product
        public static float Dot(Vector2f lhs, Vector2f rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y;
        }

        public static int Dot(Vector2i lhs, Vector2i rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y;
        }
        #endregion
        #region Cross Product
        public static float Cross(Vector2f lhs, Vector2f rhs)
        {
            return lhs.X * rhs.Y - lhs.Y * rhs.X;
        }
        public static int Cross(Vector2i lhs, Vector2i rhs)
        {
            return lhs.X * rhs.Y - lhs.Y * rhs.X;
        }
        public static Vector2f Cross(Vector2f vector, float scalar)
        {
            return new Vector2f(-scalar * vector.Y, scalar * vector.X);
        }
        public static Vector2f Cross(float scalar, Vector2f vector)
        {
            return new Vector2f(scalar * vector.Y, -scalar * vector.X);
        }
        #endregion

        #region Magnitude and Normalization
        public static float Magnitude(Vector2f vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        public static float SqrMagnitude(Vector2f vector)
        {
            return vector.X * vector.X + vector.Y * vector.Y;
        }

        public static Vector2f Normalize(Vector2f vector)
        {
            float magnitude = Magnitude(vector);
            return magnitude > 1E-05f ? vector / magnitude : ZeroVectorF;
        }
        #endregion

        #region Clamp
        public static Vector2f Clamp(Vector2f value, Vector2f min, Vector2f max)
        {
            return new Vector2f(
                MathF.Max(min.X, MathF.Min(value.X, max.X)),
                MathF.Max(min.Y, MathF.Min(value.Y, max.Y))
            );
        }

        public static Vector2i Clamp(Vector2i value, Vector2i min, Vector2i max)
        {
            return new Vector2i(
                Math.Max(min.X, Math.Min(value.X, max.X)),
                Math.Max(min.Y, Math.Min(value.Y, max.Y))
            );
        }
        #endregion

        #region Equals
        public static bool Equals(Vector2f a, Vector2f b, float epsilon = 1E-05f)
        {
            return Math.Abs(a.X - b.X) < epsilon && Math.Abs(a.Y - b.Y) < epsilon;
        }

        public static bool Equals(Vector2i a, Vector2i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        #endregion

        #region Utility Operations
        public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2f(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        public static Vector2i Lerp(Vector2i a, Vector2i b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Vector2i(
                (int)(a.X + (b.X - a.X) * t),
                (int)(a.Y + (b.Y - a.Y) * t)
            );
        }
        #endregion
    }
}
