namespace Plus.Noise
{
    public struct QuadVectors
    {
        public Float3 bottomLeft { get; }
        public Float3 bottomRight { get; }
        public Float3 topLeft { get; }
        public Float3 topRight { get; }

        public QuadVectors(Float3 bottomLeft, Float3 bottomRight, Float3 topLeft, Float3 topRight)
        {
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
            this.topLeft = topLeft;
            this.topRight = topRight;
        }
    }

    public struct Float3
    {
        public float x;
        public float y;
        public float z;

        public Float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static float Dot(Float3 first, Float3 second)
        {
            return (first.x * second.x + first.y * second.y + first.z * second.z);
        }

        public static Float3 operator +(Float3 first, Float3 second)
        {
            return new Float3(first.x + second.x, first.y + second.y, first.z + second.z);
        }

        public static Float3 operator -(Float3 first, Float3 second)
        {
            return new Float3(first.x - second.x, first.y - second.y, first.z - second.z);
        }
    }
}

