using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace VoxelCraft
{
    public class WorldBlockData
    {
        //private WorldBlockData _instance;
        float time = 0;

        public ConcurrentDictionary<Vector3i,ChunkData> data = new ConcurrentDictionary<Vector3i,ChunkData>();

        public Player player = new Player();
        //public ConcurrentQueue<ChunkData> LoadQueue = new ConcurrentQueue<ChunkData>();

        public SaveData saveData = new SaveData();
        public ChunkData this[Vector3i v]
        {
            get 
            {
                if (data.ContainsKey(v)) return data[v];
                else return null;
            }
        }

        public Dictionary<Vector3i, int> conBuffer = new Dictionary<Vector3i, int>();
        public WorldBlockData() {
            saveData.filePath = AppContext.BaseDirectory + "Saves\\world1\\";
            SaveMgr.LoadData(saveData);
        }
        public bool FindChunk(int x,int y,int z)
        {
            if(data.ContainsKey(new Vector3i(x, y + 1, z))|| data.ContainsKey(new Vector3i(x, y - 1, z)))
                return true;
            else return false;
        }
        public int FindBlock(int x,int y,int z)
        {
            int chunkX = x >> 4;
            int chunkY = y >> 4;
            int chunkZ = z >> 4;
            if (data.TryGetValue(new Vector3i(chunkX, chunkY, chunkZ), out ChunkData chunk))
            {
                int localX = (x & 15)+1;
                int localY = (y & 15)+1;
                int localZ = (z & 15)+1;
                return chunk[localX, localY, localZ];
            }
            return 0; // 空气
        }

        public void SetBlock(int x, int y, int z,int value)
        {
            int chunkX = x >> 4;
            int chunkY = y >> 4;
            int chunkZ = z >> 4;
            if (data.TryGetValue(new Vector3i(chunkX, chunkY, chunkZ), out ChunkData chunk))
            {
                int localX = (x & 15) + 1;
                int localY = (y & 15) + 1;
                int localZ = (z & 15) + 1;
                chunk[localX, localY, localZ] = value;
                SaveMgr.AddBlock(x, y, z,this.saveData,value);
                chunk.isLoad = false;
                BoundaryUpdate(chunk);
                chunk.UpdataChunk();
            }
        }
        //public WorldBlockData Instance
        //{
        //    get
        //    {
        //        if (_instance == null)
        //        {
        //            _instance = new WorldBlockData();
        //        }
        //        return _instance;
        //    }
        //}

        public void AddNewChunk(Vector3i v)
        {
            List<Vector3i> neighbours = GetNeighbourChunks(v);
            ChunkData c = new ChunkData(v,this);
            ChunkData chunk = c;
            if (data.ContainsKey(v))
            {
                data[v] = chunk;
                //LoadQueue.Enqueue(chunk);
            }
            else
            {
                data.TryAdd(v, chunk);
                //LoadQueue.Enqueue(chunk);
            }

            //for (int x = v.X*16; x < (v.X+1) * 16; x++)
            //{
            //    for (int y = v.Y * 16; y < (v.Y + 1) * 16; y++)
            //    {
            //        for (int z = v.Z * 16; z < (v.Z + 1) * 16; z++)
            //        {
            //            Vector3i v1 = new Vector3i(x, y, z);
            //            if (conBuffer.ContainsKey(v1)) SetBlock(x, y, z, conBuffer[v1]);
            //        }
            //    }
            //}
            BoundaryUpdate(data[v]);
        }
        public void DeleteChunk(Vector3i v)
        {
            ChunkData c;
            ChunkData chunk;
            if (data.ContainsKey(v)) c = data[v];
            else return;
            chunk = c;
            chunk._blockData = new int[18,18,18];
            BoundaryUpdate(chunk);
            chunk.UpdataChunk();
            //foreach (Vector3i offset in neighbours)
            //{
            //    data[v + offset].isLoad = false;
            //    data[v + offset].UpdataChunk();
            //    //LoadQueue.Enqueue(chunk);
            //}
            data.TryRemove(v,out ChunkData c2);
        }
        public void BoundaryUpdate(ChunkData c)
        {
            Vector3i v = c.position;
            List<Vector3i> neighbours = GetNeighbourChunks(v);
            ChunkData chunk = c;
            foreach (Vector3i offset in neighbours)
            {
                // 确定邻居方向（哪个轴非零及正负）
                int axis = -1;
                int sign = 0;
                if (offset.X != 0) { axis = 0; sign = Math.Sign(offset.X); }
                else if (offset.Y != 0) { axis = 1; sign = Math.Sign(offset.Y); }
                else if (offset.Z != 0) { axis = 2; sign = Math.Sign(offset.Z); }
                else continue; // 无效邻居

                // 根据方向确定当前区块和邻居区块的边界索引
                int internalSelf, ghostSelf, internalNeighbour, ghostNeighbour;
                if (sign > 0) // 正方向
                {
                    internalSelf = c.chunkSize;      // 当前区块右内部边界
                    ghostSelf = c.chunkSize + 1;    // 当前区块右 ghost
                    internalNeighbour = 1;           // 邻居左内部边界
                    ghostNeighbour = 0;              // 邻居左 ghost
                }
                else // 负方向
                {
                    internalSelf = 1;                // 当前区块左内部边界
                    ghostSelf = 0;                  // 当前区块左 ghost
                    internalNeighbour = c.chunkSize; // 邻居右内部边界
                    ghostNeighbour = c.chunkSize + 1; // 邻居右 ghost
                }

                Vector3i neighbourCoord = v + offset;
                if (!data.ContainsKey(neighbourCoord)) continue; // 邻居不存在则跳过（视需求可改为异常）
                ChunkData neighbour = data[neighbourCoord];

                // 根据轴向交换边界数据（遍历另外两个维度的所有内部索引）
                if (axis == 0) // X 轴
                {
                    for (int y = 1; y <= c.chunkSize; y++)
                        for (int z = 1; z <= c.chunkSize; z++)
                        {
                            neighbour[ghostNeighbour, y, z] = chunk[internalSelf, y, z];
                            chunk[ghostSelf, y, z] = neighbour[internalNeighbour, y, z];
                        }
                }
                else if (axis == 1) // Y 轴
                {
                    for (int x = 1; x <= c.chunkSize; x++)
                        for (int z = 1; z <= c.chunkSize; z++)
                        {
                            neighbour[x, ghostNeighbour, z] = chunk[x, internalSelf, z];
                            chunk[x, ghostSelf, z] = neighbour[x, internalNeighbour, z];
                        }
                }
                else // Z 轴
                {
                    for (int x = 1; x <= c.chunkSize; x++)
                        for (int y = 1; y <= c.chunkSize; y++)
                        {
                            neighbour[x, y, ghostNeighbour] = chunk[x, y, internalSelf];
                            chunk[x, y, ghostSelf] = neighbour[x, y, internalNeighbour];
                        }
                }
                
            }
            foreach (Vector3i offset in neighbours)
            {
                data[v + offset].isLoad = false;
                data[v + offset].UpdataChunk();
                //LoadQueue.Enqueue(data[v + offset]);
            }
        }
        public List<Vector3i> GetNeighbourChunks(Vector3i v)
        {
            List<Vector3i> result = new List<Vector3i>();
            List<Vector3i> vList = new List<Vector3i>();
            vList.Add(Vector3i.UnitX);
            vList.Add(Vector3i.UnitY);
            vList.Add(Vector3i.UnitZ);
            vList.Add(-Vector3i.UnitX);
            vList.Add(-Vector3i.UnitY);
            vList.Add(-Vector3i.UnitZ);
            foreach(Vector3i v3 in vList)
            {
                if(data.ContainsKey(v3 + v)) result.Add(v3);
            }
            return result;
        }

        public void Update(double UpdateTime)
        {
            player.axisTransform.Update((float)UpdateTime, 50);

            player.body.velocity +=
                player.axisTransform.Forward(player.dir, 0.1f) * 100 +
                player.axisTransform.Right(player.dir, 0.1f) * 100 +
                player.axisTransform.Up(0.1f) * 100;
            Vector3 pos = player.position;
            player.position = player.body.PhysicUpdate(pos, (float)UpdateTime, player.box, this);

            if (player.axisTransform.PressMouseLeft && time >= 0.2f)
            {
                if (Radial.RaycastVoxel(this, player.position, player.dir, 6, out Vector3i hitPos, out float hitDist, out Vector3i normal))
                    SetBlock(hitPos.X, hitPos.Y, hitPos.Z, 0);

                time = 0;
            }
            else if (player.axisTransform.PressMouseRight && time >= 0.2f)
            {
                if (Radial.RaycastVoxel(this, player.position, player.dir, 6, out Vector3i hitPos, out float hitDist, out Vector3i normal))
                {
                    Vector3i target = hitPos - normal;
                    Vector3i playerPos = new Vector3i((int)Math.Floor(player.position.X),
                                                      (int)Math.Floor(player.position.Y),
                                                      (int)Math.Floor(player.position.Z));
                    if (BlockData.Instance[FindBlock(target.X, target.Y, target.Z)].isAir)
                        if (player.nowBlock == 5)
                        {
                            if (normal.Y != 0) SetBlock(target.X, target.Y, target.Z, player.nowBlock * 10);
                            if (normal.Z != 0) SetBlock(target.X, target.Y, target.Z, player.nowBlock * 10 + 2);
                            if (normal.X != 0) SetBlock(target.X, target.Y, target.Z, player.nowBlock * 10 + 1);
                        }
                        else
                        {
                            SetBlock(target.X, target.Y, target.Z, player.nowBlock * 10);
                        }
                    if (player.box.CollidesWithWorld(this,player.position))
                        SetBlock(target.X, target.Y, target.Z, 0);

                }
                time = 0;
            }


            time += (float)UpdateTime;
        }
    }

    internal class ChunkMgr 
    {
        public static Vector3i GetPlayerChunks(Vector3 playerPos,int chunkSize)
        {
            return (Vector3i)playerPos / chunkSize;
        }
        public static List<Vector3i> GetPointsInSphericalShell(int r1, int r2)
        {
            var result = new List<Vector3i>();
            if (r2 <= 0) return result;               // 最大距离必须 >0

            int maxDistSq = r2 * r2;
            int minDistSq = r1 > 0 ? r1 * r1 : -1;    // 若 r1 ≤ 0，则下限不生效

            // 坐标搜索范围：各轴绝对值不超过 r2-1（因为距离 < r2）
            int bound = r2 - 1;
            for (int x = -bound; x <= bound; x++)
            {
                int xSq = x * x;
                if (xSq >= maxDistSq) continue;       // 剪枝

                for (int y = -bound; y <= bound; y++)
                {
                    int xySq = xSq + y * y;
                    if (xySq >= maxDistSq) continue;

                    for (int z = -bound; z <= bound; z++)
                    {
                        int distSq = xySq + z * z;
                        if (distSq >= maxDistSq) continue;
                        if (minDistSq >= 0 && distSq <= minDistSq) continue;
                        result.Add(new Vector3i(x, y, z));
                    }
                }
            }
            return result;
        }

        public static ConcurrentQueue<Vector3i> createQueue = new ConcurrentQueue<Vector3i>();

        public static ConcurrentQueue<Vector3i> deleteQueue = new ConcurrentQueue<Vector3i>();
        public static void GenChunk(WorldBlockData world, Vector3 playerPos,int chunkSize,int r)
        {
            List<Vector3i> l1 = GetPointsInSphericalShell(-1,r);
            List<Vector3i> l2 = GetPointsInSphericalShell(r+1, r+2);
            for (int i = 0; i < l1.Count; i++)
            {
                l1[i] += GetPlayerChunks(playerPos, chunkSize);
            }
            for (int i = 0; i < l2.Count; i++)
            {
                l2[i] += GetPlayerChunks(playerPos, chunkSize);
            }
            foreach (var v in l1)
            {
                if (!world.data.ContainsKey(v)) world.AddNewChunk(v);//createQueue.Enqueue(v);
            }
            foreach (var v in l2)
            {
                if (world.data.ContainsKey(v)) world.DeleteChunk(v);//deleteQueue.Enqueue(v);
            }
        }
        public static void UpdateChunk(WorldBlockData world)
        {
            if(createQueue.TryDequeue(out Vector3i v))
            {
                world.AddNewChunk(v);
            }
            if(deleteQueue.TryDequeue(out Vector3i v2))
            {
                world.DeleteChunk(v2);
            }
        }
    }
}
