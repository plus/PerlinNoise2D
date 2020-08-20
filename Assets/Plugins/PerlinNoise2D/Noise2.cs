using System;

namespace Plus.Noise
{
    public class Noise2
    {
        public float[,] GetChunk(int globalX, int globalZ, int chunkWidth, int quadWidthInChunks, int octavesCount)
        {
            if (quadWidthInChunks < 1 << octavesCount)
            {
                throw new ArgumentException($"Количество чанков(quadWidthInChunks:{quadWidthInChunks}) в Quad'e должно быть не меньше чем 2 ^ кол-ва октав, octavesCount:{octavesCount}");
            }

            var result = new float[chunkWidth, chunkWidth];
            for (int octave = 0; octave <= octavesCount; octave++)
            {
                int scaleFactor = 1 << octave;
                var (quadX, quadZ, chunkId) = GetChunkIdInQuadByGlobalPosition(globalX, globalZ, chunkWidth, quadWidthInChunks / scaleFactor);
                var values = GetChunkOctave(quadX, quadZ, chunkId, chunkWidth, quadWidthInChunks / scaleFactor, octave);

                for (int x = 0; x < values.GetLength(0); x++)
                {
                    for (int z = 0; z < values.GetLength(1); z++)
                    {
                        result[x, z] += values[x, z] / scaleFactor;
                    }
                }
            }

            // Normalize.
            float normalizeCoefficient = 0f;
            for (int i = 0; i <= octavesCount; i++)
            {
                normalizeCoefficient += 1 / (float)(1 << i);
            }

            for (int x = 0; x < chunkWidth; x++)
            {
                for (int z = 0; z < chunkWidth; z++)
                {
                    result[x, z] = result[x, z] / normalizeCoefficient;
                }
            }

            return result;
        }

        protected virtual float SmoothFunction(float x)
        {
            if (x <= 0) return 0;
            if (x >= 1) return 1;
            return 3f * x * x - 2f * x * x * x;
        }

        private float[,] GetChunkOctave(int quadX, int quadZ, int chunkId, int chunkWidth, int quadWidthInChunks, int octaveSeed)
        {
            var result = new float[chunkWidth, chunkWidth];

            var leftBottomPosition = new Float3(quadX, 0f, quadZ);
            var rightBottomPosition = new Float3(quadX + 1, 0f, quadZ);
            var leftTopPosition = new Float3(quadX, 0f, quadZ + 1);
            var rightTopPosition = new Float3(quadX + 1, 0f, quadZ + 1);
            QuadVectors vectors = GetQuadVectors(quadX, quadZ, octaveSeed);
            var chunkPositionLocal = GetChunkInternal(chunkId, quadWidthInChunks);

            for (int x = 0; x < chunkWidth; x++)
            {
                for (int z = 0; z < chunkWidth; z++)
                {
                    // internalX, internalX - 0..1 (t)
                    float internalX = (chunkPositionLocal.internalX * chunkWidth + x) / (float)(chunkWidth * quadWidthInChunks);
                    float internalZ = (chunkPositionLocal.internalZ * chunkWidth + z) / (float)(chunkWidth * quadWidthInChunks);
                    var pointPosition = new Float3(quadX + internalX, 0f, quadZ + internalZ);

                    float dotLeftBottom = Float3.Dot(pointPosition - leftBottomPosition, vectors.BottomLeft);
                    float dotRightBottom = Float3.Dot(pointPosition - rightBottomPosition, vectors.BottomRight);
                    float dotLeftTop = Float3.Dot(pointPosition - leftTopPosition, vectors.TopLeft);
                    float dotRightTop = Float3.Dot(pointPosition - rightTopPosition, vectors.TopRight);

                    float interpolationLeft, interpolationRight, interpolationFull;
                    float t, a, b;

                    t = internalZ;
                    a = dotLeftBottom;
                    b = dotLeftTop;
                    interpolationLeft = Interpolate(a, b, t);

                    t = internalZ;
                    a = dotRightBottom;
                    b = dotRightTop;
                    interpolationRight = Interpolate(a, b, t);

                    t = internalX;
                    a = interpolationLeft;
                    b = interpolationRight;
                    interpolationFull = Interpolate(a, b, t);

                    result[x, z] = interpolationFull / (float)(Math.Sqrt(2) / 2f); // Todo: caching sqrt
                    result[x, z] = (result[x, z] + 1f) / 2f;
                }
            }

            return result;
        }

        private (int quadX, int quadZ, int chunkId) GetChunkIdInQuadByGlobalPosition(int x, int z, int chunkWidth, int quadWidthInChunks)
        {
            int quadWidth = chunkWidth * quadWidthInChunks;
            int quadX = x / quadWidth;
            if (x < 0 && x % quadWidth != 0) quadX -= 1;
            int quadZ = z / quadWidth;
            if (z < 0 && z % quadWidth != 0) quadZ -= 1;

            int internalX = x - quadX * quadWidth;
            int internalZ = z - quadZ * quadWidth;
            int chunkId = internalX / chunkWidth + (internalZ / chunkWidth) * quadWidthInChunks;

            return (quadX, quadZ, chunkId);
        }

        /// <summary>
        /// Получение опорных векторов по координатам quad'а.
        /// В качестве seed используется octaveIndex.
        /// </summary>
        /// <param name="quadX"></param>
        /// <param name="quadZ"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private QuadVectors GetQuadVectors(int quadX, int quadZ, int seed)
        {
            float randomAngle;
            int leftXIndex = quadX;
            int rightXIndex = leftXIndex + 1;
            int bottomZIndex = quadZ;
            int topZIndex = bottomZIndex + 1;

            randomAngle = 360f * GetRandomXXHash(leftXIndex, bottomZIndex, seed);
            var leftBottom = new Float3((float)Math.Cos(randomAngle), 0f, (float)Math.Sin(randomAngle));

            randomAngle = 360f * GetRandomXXHash(rightXIndex, bottomZIndex, seed);
            var rightBottom = new Float3((float)Math.Cos(randomAngle), 0f, (float)Math.Sin(randomAngle));

            randomAngle = 360f * GetRandomXXHash(leftXIndex, topZIndex, seed);
            var leftTop = new Float3((float)Math.Cos(randomAngle), 0f, (float)Math.Sin(randomAngle));

            randomAngle = 360f * GetRandomXXHash(rightXIndex, topZIndex, seed);
            var rightTop = new Float3((float)Math.Cos(randomAngle), 0f, (float)Math.Sin(randomAngle));

            return new QuadVectors(leftBottom, rightBottom, leftTop, rightTop);
        }

        private (int internalX, int internalZ) GetChunkInternal(int chunkId, int quadWidthInChunks)
        {
            int internalZ = chunkId / quadWidthInChunks;
            int internalX = chunkId - internalZ * quadWidthInChunks;

            return (internalX, internalZ);
        }

        private float Interpolate(float a, float b, float t)
        {
            return a * (1f - SmoothFunction(t)) + b * SmoothFunction(t);
        }

        private float GetRandomXXHash(int x, int z, int seed)
        {
            var randomHashObject = new XXHash(seed);
            return randomHashObject.GetHash(x, z);
        }
    }
}