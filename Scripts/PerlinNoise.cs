using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;

namespace VoxelCraft
{

    public class TerrainGenerator
    {
        private readonly PerlinNoise _heightNoise;
        private readonly PerlinNoise _detailNoise;
        private readonly int _seed;
        private readonly int[,,] trees = new int[,,]{
            {{0,0,0,0,0}, {0,0,0,0,0},{0,0,0,0,0},{70,70,70,70,70},{0,70,70,70,70},{0,0,0,0,0},{0,0,0,0,0}},
            {{0,0,0,0,0}, {0,0,0,0,0},{0,0,0,0,0},{70,70,70,70,70},{70,70,70,70,70},{0,70,70,70,0},{0,0,70,70,0}},
            {{0,0,50,0,0}, {0,0,50,0,0},{0,0,50,0,0},{70,70,50,70,70},{70,70,50,70,70},{0,70,50,70,0},{0,70,70,70,0}},
            {{0,0,0,0,0}, {0,0,0,0,0},{0,0,0,0,0},{70,70,70,70,70},{70,70,70,70,70},{0,70,70,70,0},{0,0,70,70,0}},
            {{0,0,0,0,0}, {0,0,0,0,0},{0,0,0,0,0},{70,70,70,70,70},{70,70,70,70,70},{0,0,0,0,0},{0,0,0,0,0}},
        };
        public TerrainGenerator(int seed)
        {
            _seed = seed;
            _heightNoise = new PerlinNoise(seed);
            _detailNoise = new PerlinNoise(seed + 1);
        }

        /// <summary>
        /// 获取世界坐标 (x, z) 处的地面高度（整数）
        /// </summary>
        public int GetHeight(int worldX, int worldZ,Vector3 range)
        {
            // 低频主地形（幅度 64，频率 0.005）
            float main = (float)_heightNoise.Noise(worldX * 0.005f, worldZ * 0.005f);
            // 叠加中频起伏（幅度 16，频率 0.02）
            float mid = (float)_heightNoise.Noise(worldX * 0.02f, worldZ * 0.02f) * 0.5f;
            // 叠加高频细节（幅度 4，频率 0.1）
            float detail = (float)_detailNoise.Noise(worldX * 0.1f, worldZ * 0.1f) * 0.2f;

            float height = main * range.X + mid * range.Y + detail * range.Z;
            // 增加海平面基线（比如 Y=0 为海平面）
            return (int)(height + 32f);
        }

        public List<Vector2i> GetTree(ChunkData chunk)
        {
            float main = (float)_heightNoise.Noise(chunk.position.X * 0.005f, chunk.position.Z * 0.005f);
            List<Vector2i> treeNodes = new List<Vector2i>();
            Random random = new Random(_seed*chunk.position.X*chunk.position.Z);

            for(int i = 0; i < 2; i++)
            {
                int x = random.Next(0, 16);
                int y = random.Next(0, 16); 
                Vector2i node = new Vector2i(x, y);
                bool legal = true;
                foreach(var n in treeNodes)
                {
                    if(Vector2.Distance(n, node) <= 5)
                    {
                        legal = false;
                        break;
                    }
                }
                if (legal)
                {
                    treeNodes.Add(node);
                }
            }
            return treeNodes;
        }

        public void AddTree(Dictionary<Vector3i,int> buffer,int X,int Y,int Z)
        {
            for(int x = 0; x < trees.GetLength(0); x++) 
            {
                for (int y = 0; y < trees.GetLength(1); y++)
                {
                    for (int z = 0; z < trees.GetLength(2); z++)
                    {
                        if(trees[x, y, z] != 0)
                            buffer.TryAdd(new Vector3i(X + x-2, Y + y-2, Z + z-2), trees[x,y,z]);
                    }
                }
            }
        }

        /// <summary>
        /// 根据位置和高度决定体素类型
        /// </summary>
        public int GetBlockType(int worldX, int worldY, int worldZ, int surfaceHeight,int stoneHeight)
        {
            if (worldY > stoneHeight && worldY > surfaceHeight) return 0;
            if (worldY > surfaceHeight && worldY < stoneHeight) return 20;
            if (worldY == surfaceHeight && worldY > stoneHeight) return 10;
            if (worldY > stoneHeight) return 30;
            return 20;
        }
    }
    public class PerlinNoise
    {
        private readonly int[] permutation = new int[512];
        private readonly int[] p;

        // 标准梯度方向（8个，也可用12个）
        private static readonly int[,] grad3 = {
        {1,1}, {-1,1}, {1,-1}, {-1,-1},
        {1,0}, {-1,0}, {0,1}, {0,-1}
    };

        public PerlinNoise(int seed = 0)
        {
            var rand = new Random(seed);
            // 初始化置换表 0~255
            int[] perm = new int[256];
            for (int i = 0; i < 256; i++) perm[i] = i;
            for (int i = 255; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (perm[i], perm[j]) = (perm[j], perm[i]);
            }
            // 填充两次，避免越界
            for (int i = 0; i < 512; i++) permutation[i] = perm[i & 255];
            p = permutation;
        }

        // 缓和曲线
        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        // 线性插值
        private static double Lerp(double a, double b, double t) => a + t * (b - a);

        // 获取晶格顶点的梯度点积结果
        private double Grad(int hash, double x, double y)
        {
            int h = hash & 7;          // 8个梯度方向
            double u = h < 4 ? x : y;
            double v = h < 4 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        public double Noise(double x, double y)
        {
            // 找到晶格坐标
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;
            double fx = x - Math.Floor(x);
            double fy = y - Math.Floor(y);

            // 缓和曲线权值
            double u = Fade(fx);
            double v = Fade(fy);

            // 四个角点的哈希值
            int aa = p[p[xi] + yi];
            int ab = p[p[xi] + yi + 1];
            int ba = p[p[xi + 1] + yi];
            int bb = p[p[xi + 1] + yi + 1];

            // 点积并插值
            double x1 = Lerp(Grad(aa, fx, fy), Grad(ba, fx - 1, fy), u);
            double x2 = Lerp(Grad(ab, fx, fy - 1), Grad(bb, fx - 1, fy - 1), u);
            double result = Lerp(x1, x2, v);

            // 结果范围大约 [-0.8,0.8]，映射到 [0,1]
            return (result + 0.8) / 1.6;
        }
    }
}
