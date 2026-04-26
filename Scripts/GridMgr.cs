using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
namespace VoxelCraft
{
    internal class GridMgr
    {
        public GridMgr() { }
        public static bool GridGenerator(ChunkData chunk,out List<byte[]> chunkGrid,out List<byte[]> chunkGridLucency)
        {
            int length = chunk.chunkSize;
            chunkGrid = new List<byte[]>();
            chunkGridLucency = new List<byte[]>();
            for (byte y = 1; y <= length; y++)
            {
                for (byte z = 1; z <= length; z++)
                {
                    for (byte x = 1; x <= length; x++)
                    {
                        byte[] mat = BlockData.Instance.data.ContainsKey(chunk[x, y, z]) ? 
                                     BlockData.Instance[chunk[x, y, z]].material:
                                     new byte[6] {0,0,0,0,0,0 };
                        bool isSoild = BlockData.Instance[chunk[x, y, z]].isSolid;
                        bool isLucency = BlockData.Instance[chunk[x, y, z]].isLucency;
                        int id = chunk[x, y, z];
                        if (isSoild)
                        {
                            byte X = (byte)((x - 1) * 8);
                            byte Y = (byte)((y - 1) * 8);
                            byte Z = (byte)((z - 1) * 8);

                            if (!BlockData.Instance[chunk[x - 1, y, z]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[0], 0 }); //左
                            if (!BlockData.Instance[chunk[x + 1, y, z]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[1], 1 }); //右
                            if (!BlockData.Instance[chunk[x, y - 1, z]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[2], 2 }); //下
                            if (!BlockData.Instance[chunk[x, y + 1, z]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[3], 3 }); //上
                            if (!BlockData.Instance[chunk[x, y, z - 1]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[4], 4 }); //后
                            if (!BlockData.Instance[chunk[x, y, z + 1]].isSolid) chunkGrid.Add(new byte[5] { X, Y, Z, mat[5], 5 }); //前
                        }
                        else if (isLucency)
                        {
                            byte X = (byte)((x - 1) * 8);
                            byte Y = (byte)((y - 1) * 8);
                            byte Z = (byte)((z - 1) * 8);

                            if (BlockData.Instance[chunk[x - 1, y, z]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[0], 0 }); //左
                            if (BlockData.Instance[chunk[x + 1, y, z]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[1], 1 }); //右
                            if (BlockData.Instance[chunk[x, y - 1, z]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[2], 2 }); //下
                            if (BlockData.Instance[chunk[x, y + 1, z]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[3], 3 }); //上
                            if (BlockData.Instance[chunk[x, y, z - 1]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[4], 4 }); //后
                            if (BlockData.Instance[chunk[x, y, z + 1]].id != id) chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[5], 5 }); //前
                        }
                        else if (id == 80)
                        {
                            byte X = (byte)((x - 1) * 8);
                            byte Y = (byte)((y - 1) * 8);
                            byte Z = (byte)((z - 1) * 8);

                            chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[0], 0 }); //左
                            chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[1], 1 }); //右
                            chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[2], 2 }); //下
                            chunkGridLucency.Add(new byte[5] { X, (byte)(Y - 4), Z, mat[3], 3 }); //上
                            chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[4], 4 }); //后
                            chunkGridLucency.Add(new byte[5] { X, Y, Z, mat[5], 5 }); //前
                        }
                    }
                }
            }
            return true;
        }
    }
    internal class VerTexMgr
    {
        private static readonly int[,,,] AOOffsets = new int[6, 4, 3, 3]
        {
            // 面 0: 左面 (-X)
            {
                // 角0: 上后 (y=1, z=0) → +Y, -Z, +Y-Z
                { { -1,  1, -1 }, {-1,  0, -1 },{-1,  1,  0 } },
                // 角1: 上前 (y=1, z=1) → +Y, +Z, +Y+Z
                { { -1,  1,  1 }, {-1,  0,  1 },{-1,  1,  0 } },
                // 角2: 下前 (y=0, z=1) → -Y, +Z, -Y+Z
                { { -1, -1,  1 }, {-1,  0,  1 },{-1, -1,  0 } },
                // 角3: 下后 (y=0, z=0) → -Y, -Z, -Y-Z
                { { -1, -1, -1 }, {-1,  0, -1 },{-1, -1,  0 } }
            },
            // 面 1: 右面 (+X)
            {
                // 角0: 上前 (y=1, z=1) → +Y, +Z, +Y+Z
                { { 1,  1,  1 }, { 1,  0,  1 }, { 1,  1,  0 } },
                // 角1: 上后 (y=1, z=0) → +Y, -Z, +Y-Z
                { { 1,  1, -1 }, { 1,  0, -1 }, { 1,  1,  0 } },
                // 角2: 下后 (y=0, z=0) → -Y, -Z, -Y-Z
                { { 1, -1, -1 }, { 1,  0, -1 }, { 1, -1,  0 } },
                // 角3: 下前 (y=0, z=1) → -Y, +Z, -Y+Z
                { { 1, -1,  1 }, { 1,  0,  1 }, { 1, -1,  0 } }
            },
            // 面 2: 下面 (-Y)
            {
                // 角0: 前左 (x=0, z=1) → -X, +Z, -X+Z
                { { -1, -1,  1 }, {-1, -1,  0 }, {  0, -1,  1 } },
                // 角1: 前右 (x=1, z=1) → +X, +Z, +X+Z
                { {  1, -1,  1 }, { 1, -1,  0 }, {  0, -1,  1 } },
                // 角2: 后右 (x=1, z=0) → +X, -Z, +X-Z
                { {  1, -1, -1 }, { 1, -1,  0 }, {  0, -1, -1 } },
                // 角3: 后左 (x=0, z=0) → -X, -Z, -X-Z
                { { -1, -1, -1 }, {-1, -1,  0 }, {  0, -1, -1 } }
            },
            // 面 3: 上面 (+Y)
            {
                // 角0: 后左 (x=0, z=0) → -X, -Z, -X-Z
                { { -1,  1, -1 }, { 0,  1, -1 }, { -1,  1,  0 } },
                // 角1: 后右 (x=1, z=0) → +X, -Z, +X-Z
                { {  1,  1, -1 }, { 0,  1, -1 }, {  1,  1,  0 } },
                // 角2: 前右 (x=1, z=1) → +X, +Z, +X+Z
                { {  1,  1,  1 }, { 0,  1,  1 }, {  1,  1,  0 } },
                // 角3: 前左 (x=0, z=1) → -X, +Z, -X+Z
                { { -1,  1,  1 }, { 0,  1,  1 }, { -1,  1,  0 } }
            },
            // 面 4: 后面 (-Z) （已正确）
            {
                // 角0: 左下 (x=0, y=0) → -X, -Y, -X-Y
                { { -1, -1, -1 }, { 0, -1, -1 }, { -1,  0, -1 } },
                // 角1: 右下 (x=1, y=0) → +X, -Y, +X-Y
                { {  1, -1, -1 }, { 0, -1, -1 }, {  1,  0, -1 } },
                // 角2: 右上 (x=1, y=1) → +X, +Y, +X+Y
                { {  1,  1, -1 }, { 0,  1, -1 }, {  1,  0, -1 } },
                // 角3: 左上 (x=0, y=1) → -X, +Y, -X+Y
                { { -1,  1, -1 }, { 0,  1, -1 }, { -1,  0, -1 } }
            },
            // 面 5: 前面 (+Z) （已正确）
            {
                // 角0: 左上 (x=0, y=1) → -X, +Y, -X+Y
                { { -1,  0,  1 }, { 0,  1,  1 }, { -1,  1,  1 } },
                // 角1: 右上 (x=1, y=1) → +X, +Y, +X+Y
                { {  1,  0,  1 }, { 0,  1,  1 }, {  1,  1,  1 } },
                // 角2: 右下 (x=1, y=0) → +X, -Y, +X-Y
                { {  1,  0,  1 }, { 0, -1,  1 }, {  1, -1,  1 } },
                // 角3: 左下 (x=0, y=0) → -X, -Y, -X-Y
                { { -1,  0,  1 }, { 0, -1,  1 }, { -1, -1,  1 } }
            }
        };
        public VerTexMgr() { }

        /// <summary>
        /// 根据面列表生成顶点字节数组
        /// 每个面由两个三角形（6个顶点）组成，每个顶点6字节：XYZ（3字节）、UV（2字节）、法线枚举（1字节）
        /// </summary>
        /// <param name="faces">面列表，每个元素：XYZ为方块左下后顶点坐标，W为朝向（0左,1右,2下,3上,4后,5前）</param>
        /// <returns>顶点数组，可直接提交给渲染器</returns>
        public static byte[] GenerateVertexArray(List<byte[]> faces,int atlasSize,ChunkData chunk)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (var face in faces)
                {
                    if(face.Length != 5) continue;

                    int bx = face[0], by = face[1], bz = face[2];
                    int offsetU = face[3] % atlasSize;
                    int offsetV = face[3] / atlasSize;
                    int facing = face[4]; // 0-5

                    // 获取四个顶点的局部偏移和UV
                    var (offsets, uv) = GetFaceData(facing);


                    // 四个顶点的最终数据（每个顶点6个字节）
                    byte[][] vertices = new byte[4][];

                    for (int i = 0; i < 4; i++)
                    {
                        // 计算世界坐标（偏移后）
                        int x = bx + offsets[i].X * 8;
                        int y = by + offsets[i].Y * 8;
                        int z = bz + offsets[i].Z * 8;

                        uv[i].X += offsetU;
                        uv[i].Y += offsetV;
                        // UV映射到0-255
                        int u = (int)(uv[i].X * atlasSize);
                        int v = (int)(uv[i].Y * atlasSize);

                        // 钳位到有效范围
                        x = Math.Clamp(x, 0, 255);
                        y = Math.Clamp(y, 0, 255);
                        z = Math.Clamp(z, 0, 255);
                        u = Math.Clamp(u, 0, 255);
                        v = Math.Clamp(v, 0, 255);

                        byte ao = GenerateAO(bx/8,by/8,bz/8,facing,i,chunk);
                        //byte f = (byte)((ao << 3)|(facing & 0x07));

                        // 打包顶点：XYZ, UV, 法线枚举
                        vertices[i] = new byte[]
                        {
                        (byte)x, (byte)y, (byte)z,
                        (byte)u, (byte)v,
                        ao
                        };
                    }

                    // 两个三角形: (0,1,2) 和 (0,2,3)
                    int[] indices = { 0, 1, 2, 0, 2, 3 };
                    foreach (int idx in indices)
                    {
                        stream.Write(vertices[idx], 0, 6);
                    }
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// 获取指定朝向的四个顶点局部偏移（相对于方块左下后角）和UV坐标
        /// 偏移量顺序: 左下、右下、右上、左上（从外面看逆时针）
        /// </summary>
        private static (Vector3i[] offsets, Vector2[] uv) GetFaceData(int facing)
        {
            switch (facing)
            {
                case 0: // 左面 (-x)
                    return (new Vector3i[]
                    {
                    new Vector3i(0, 1, 0),
                    new Vector3i(0, 1, 1),
                    new Vector3i(0, 0, 1),
                    new Vector3i(0, 0, 0),
                    }, new Vector2[]
                    {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                    });
                case 1: // 右面 (+x)
                    return (new Vector3i[]
                    {
                    new Vector3i(1, 1, 1),
                    new Vector3i(1, 1, 0),
                    new Vector3i(1, 0, 0),
                    new Vector3i(1, 0, 1)
                    }, new Vector2[]
                    {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                    });
                case 2: // 下面 (-y)
                    return (new Vector3i[]
                    {
                    new Vector3i(0, 0, 1),
                    new Vector3i(1, 0, 1),
                    new Vector3i(1, 0, 0),
                    new Vector3i(0, 0, 0)
                    }, new Vector2[]
                    {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                    });
                case 3: // 上面 (+y)
                    return (new Vector3i[]
                    {
                    new Vector3i(0, 1, 0),
                    new Vector3i(1, 1, 0),
                    new Vector3i(1, 1, 1),
                    new Vector3i(0, 1, 1)
                    }, new Vector2[]
                    {
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0)
                    });
                case 4: // 后面 (-z)
                    return (new Vector3i[]
                    {
                    new Vector3i(0, 0, 0),
                    new Vector3i(1, 0, 0),
                    new Vector3i(1, 1, 0),
                    new Vector3i(0, 1, 0)
                    }, new Vector2[]
                    {
                    new Vector2(1, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(1, 0)
                    });
                case 5: // 前面 (+z)
                    return (new Vector3i[]
                    {
                    new Vector3i(0, 1, 1),
                    new Vector3i(1, 1, 1),
                    new Vector3i(1, 0, 1),
                    new Vector3i(0, 0, 1)
                    }, new Vector2[]
                    {
                    new Vector2(0, 0),
                    new Vector2(1, 0),
                    new Vector2(1, 1),
                    new Vector2(0, 1)
                    });
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), "朝向值必须为0~5");
            }
        }

        public static byte GenerateAO(int blockX, int blockY, int blockZ, int normal, int corner, ChunkData chunk)
        {
            int solidCount = 0;

            // 获取三个需要检查的邻居偏移
            for (int i = 0; i < 3; i++) // i=0: 轴向1, i=1: 轴向2, i=2: 对角
            {
                int dx = AOOffsets[normal, corner, i, 0];
                int dy = AOOffsets[normal, corner, i, 1];
                int dz = AOOffsets[normal, corner, i, 2];

                int nx = blockX + dx;
                int ny = blockY + dy;
                int nz = blockZ + dz;

                if (IsSolid(chunk, nx, ny, nz))
                    solidCount++;
            }

            // solidCount 范围 0~3，映射到亮度系数 0.5~1.0
            float brightness = 1.0f - (solidCount / 3.0f) * 0.8f;
            byte aoValue = (byte)(brightness * 31.0f);
            return aoValue;
        }

        private static bool IsSolid(ChunkData chunk, int x, int y, int z)
        {
            // 注意：你的 ChunkData 索引器需要坐标 +1（因为内部存储多了一圈边界）
            return BlockData.Instance[chunk[x + 1, y + 1, z + 1]].isSolid;
        }
    }

    public class GridData
    {
        public List<byte[]> grids = new List<byte[]>();
        public Vector3i chunkPos = new Vector3i();

        public byte[] _blockVertices = new byte[] { };

        public int _chunkVao;
        public int _chunkVbo;
        public GridData() { }
        public void Load()
        {
            _chunkVao = GL.GenVertexArray();
            GL.BindVertexArray(_chunkVao);

            _chunkVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _chunkVbo);
        }
        public void GenVertices(Vector3i chunkPos,ChunkData chunk)
        {
            this.chunkPos = chunkPos;
            //暂时硬编码
            _blockVertices = VerTexMgr.GenerateVertexArray(grids,16,chunk); ;
            //向vbo中传入顶点数据
            GL.BufferData(BufferTarget.ArrayBuffer, _blockVertices.Length * sizeof(byte), _blockVertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.UnsignedByte, false, 6 * sizeof(byte), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.UnsignedByte, false, 6 * sizeof(byte), 3 * sizeof(byte));
            GL.EnableVertexAttribArray(1);

            //GL.VertexAttribPointer(2, 1, VertexAttribPointerType.Byte, false, 6 * sizeof(byte), 5 * sizeof(byte));
            //GL.EnableVertexAttribArray(2);


        }
    }
}
