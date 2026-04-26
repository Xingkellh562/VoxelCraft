using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VoxelCraft
{
    public class ChunkData
    {
        public int chunkSize = 16;
        const int chunkSizeC = 18;

        public bool isLoad = false;

        public int[,,] _blockData = new int[chunkSizeC, chunkSizeC, chunkSizeC];
        public Vector3i position = new Vector3i();

        public GridData gridData = new GridData();
        public GridData gridDataLucency = new GridData();
        public int this[int x, int y , int z]
        {
            get { return _blockData[x, y, z]; }
            set { _blockData[x, y, z] = value; }
        }
        public ChunkData(Vector3i v,WorldBlockData world)
        {
            this.position = v;
            GenerateChunkData(v,new TerrainGenerator(123), world);
        }

        public void UpdataChunk()
        {
            if(GridMgr.GridGenerator(this,out List<byte[]> l1,out List<byte[]> l2))
            {
                gridData.grids = l1;
                gridDataLucency.grids = l2;
            }
        }

        public void GenerateChunkData(Vector3i chunkPos, TerrainGenerator terrain,WorldBlockData world)
        {
            int size = this.chunkSize;

            List<Vector3i> vList = new List<Vector3i>()
            {
                 Vector3i.Zero,
                 Vector3i.UnitX,
                 Vector3i.UnitZ,
                -Vector3i.UnitX,
                -Vector3i.UnitZ,
            };
           
            foreach (Vector3i v3 in vList)
            {
                if (!world.FindChunk(chunkPos.X+v3.X, chunkPos.Y, chunkPos.Z+ v3.Z))
                {
                    List<Vector2i> treeNodes = terrain.GetTree(this);
                    foreach (var node in treeNodes)
                    {
                        int worldX = (chunkPos.X + v3.X) * size + node.X;
                        int worldZ = (chunkPos.Z + v3.Z) * size + node.Y;
                        int surfaceY = terrain.GetHeight(worldX, worldZ, new Vector3(16, 60, 16));
                        terrain.AddTree(world.conBuffer, worldX, surfaceY + 3, worldZ);
                    }
                }
            }
            // 遍历区块内所有局部坐标
            for (int x = 1; x <= size; x++)
            {
                int worldX = chunkPos.X * size + x - 1;
                for (int z = 1; z <= size; z++)
                {
                    int worldZ = chunkPos.Z * size + z - 1;
                    int surfaceY = terrain.GetHeight(worldX, worldZ, new Vector3(16, 60, 16));
                    int stoneY = terrain.GetHeight(worldX, worldZ, new Vector3(16, 64, 16)) - 4;

                    // 遍历 Y 轴（1 到 size），注意区块的高度范围可能大于地形起伏范围
                    for (int y = 1; y <= size; y++)
                    {
                        int worldY = chunkPos.Y * size + y - 1;
                        int block = terrain.GetBlockType(worldX, worldY, worldZ, surfaceY, stoneY);
                        this[x, y, z] = block;

                        Vector3i v = new Vector3i(worldX, worldY, worldZ);
                        if (world.conBuffer.ContainsKey(v))
                        {
                            this[x, y, z] = world.conBuffer[v];
                            world.conBuffer.Remove(v);
                        }
                        
                        if (SaveMgr.GetBlock(worldX, worldY, worldZ, world.saveData, out int b))
                            this[x, y, z] = b;
                    }
                }
                
            }
            
        }
        public void Load()
        {
            if (!isLoad)
            {
                gridData.Load();
                gridData.GenVertices(position,this);
                gridDataLucency.Load();
                gridDataLucency.GenVertices(position,this);
                isLoad = true;
            }
        }
    }
}
