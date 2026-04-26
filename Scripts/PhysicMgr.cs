using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace VoxelCraft
{
    public class PhysicMgr
    {
        public Vector3 velocity = new Vector3();
        public Vector3 acceleratedVelocity = new Vector3(0,0,0);

        public float damp = 0.1f;
        public PhysicMgr() { }

        public Vector3 PhysicUpdate(Vector3 pos, float updateTime, BoxAABB box, WorldBlockData world)
        {
            
            velocity += acceleratedVelocity * updateTime;
            velocity -= damp * velocity;  // 阻力

            Vector3 newPos = pos;

            float yMove = velocity.Y * updateTime;
            newPos.Y += yMove;
            if (box.CollidesWithWorld(world, newPos))
            {
                if (yMove > 0) // 向上
                {
                    // 顶部碰到方块底部：maxPos 对齐到 floor(worldMaxPos)
                    //float worldMaxY = newPos.Y + box.maxPos.Y;
                    //newPos.Y = (float)Math.Round(worldMaxY) - box.maxPos.Y;
                    newPos.Y -= yMove;
                }
                else if (yMove < 0) // 向下
                {
                    // 底部碰到方块顶部：minPos 对齐到 floor(worldMinY) + 1
                    //float worldMinY = newPos.Y + box.minPos.Y;
                    //newPos.Y = (float)Math.Round(worldMinY)+1 - box.minPos.Y;
                    newPos.Y -= yMove;
                }
                velocity.Y = 0;
            }

            float xMove = velocity.X * updateTime;
            newPos.X += xMove;
            if (box.CollidesWithWorld(world, newPos))
            {
                if (xMove > 0) // 向右
                {
                    //float worldMaxX = newPos.X + box.maxPos.X;
                    //newPos.X = (float)Math.Round(worldMaxX) - box.maxPos.X;
                    newPos.X -= xMove;
                }
                else if (xMove < 0) // 向左
                {
                    //float worldMinX = newPos.X + box.minPos.X;
                    //newPos.X = (float)Math.Round(worldMinX)+1 - box.minPos.X;
                    newPos.X -= xMove;
                }
                velocity.X = 0;
            }

            float zMove = velocity.Z * updateTime;
            newPos.Z += zMove;
            if (box.CollidesWithWorld(world, newPos))
            {
                if (zMove > 0)
                {
                    //float worldMaxZ = newPos.Z + box.maxPos.Z;
                    //newPos.Z = (float)Math.Round(worldMaxZ) - box.maxPos.Z;
                    newPos.Z -= zMove;
                }
                else if (zMove < 0)
                {
                    //float worldMinZ = newPos.Z + box.minPos.Z;
                    //newPos.Z = (float)Math.Round(worldMinZ)+1 - box.minPos.Z;
                    newPos.Z -= zMove;
                }
                velocity.Z = 0;
            }

            return newPos;
        }
    }

    public class BoxAABB
    {
        public Vector3 minPos { get; private set; }
        public Vector3 maxPos { get; private set; }
        public BoxAABB(Vector3 minPos,Vector3 maxPos) 
        { 
            this.minPos = minPos;
            this.maxPos = maxPos;
        }

        public bool CollidesWithWorld(WorldBlockData world,Vector3 entityPos)
        {

            int chunkSize = 16;

            int minChunkX = (int)Math.Floor(minPos.X + entityPos.X);
            int minChunkY = (int)Math.Floor(minPos.Y + entityPos.Y);
            int minChunkZ = (int)Math.Floor(minPos.Z + entityPos.Z);

            int maxChunkX = (int)Math.Floor(maxPos.X + entityPos.X);
            int maxChunkY = (int)Math.Floor(maxPos.Y + entityPos.Y);
            int maxChunkZ = (int)Math.Floor(maxPos.Z + entityPos.Z);

            //int worldX = (int)Math.Ceiling(entityPos.X);
            //int worldY = (int)Math.Ceiling(entityPos.Y);
            //int worldZ = (int)Math.Ceiling(entityPos.Z);

            //if (worldX % 16 == 0)

                //if (minChunkX > maxChunkX) maxChunkX += chunkSize;
                //if (minChunkY > maxChunkY) maxChunkY += chunkSize;
                //if (minChunkZ > maxChunkZ) maxChunkZ += chunkSize;

            for (int x = minChunkX; x<= maxChunkX; x++)
            {
                for (int y = minChunkY; y <= maxChunkY; y++)
                {
                    for (int z = minChunkZ; z <= maxChunkZ; z++)
                    {
                        if (!BlockData.Instance[world.FindBlock(x, y, z)].isAir) { return true; }
                    }
                }
            }

            return false;
        }
    }
}
