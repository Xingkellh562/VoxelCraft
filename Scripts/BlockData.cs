using OpenTK.Platform.Windows;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace VoxelCraft
{
    internal class BlockData
    {
        private static BlockData? _instance;
        private static readonly object _lock = new object();
        public Dictionary<int, Block> data { get; } = new Dictionary<int, Block>();

        public Block this[int id]
        {
            get { return data[id]; }
        }

        public static BlockData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock(_lock)
                    {
                        _instance = new BlockData();
                        //加载配置文件等扩展
                    }
                }
                return _instance;
            }
        }

        public void GetBlockData(Block block)
        {
            if(!data.ContainsKey(block.id*10))
            {
                data.Add(block.id*10, block);
            }
            else
            {
                int i = 1;
                while (data.ContainsKey(block.id * 10 + i))
                {
                    i++;
                }
                i = Math.Clamp(i, 0, 9);
                data.Add(block.id * 10 + i, block);
            }
        }
        //if (chunk[x - 1, y, z] == 0) chunkGrid.Add(new byte[5] { x, y, z, 3 ,0 }); //左
        //if (chunk[x + 1, y, z] == 0) chunkGrid.Add(new byte[5] { x, y, z, 3, 1 }); //右
        //if (chunk[x, y - 1, z] == 0) chunkGrid.Add(new byte[5] { x, y, z, 1, 2 }); //下
        //if (chunk[x, y + 1, z] == 0) chunkGrid.Add(new byte[5] { x, y, z, 2, 3 }); //上
        //if (chunk[x, y, z - 1] == 0) chunkGrid.Add(new byte[5] { x, y, z, 3, 4 }); //后
        //if (chunk[x, y, z + 1] == 0) chunkGrid.Add(new byte[5] { x, y, z, 3, 5 }); //前
        private BlockData()
        {
            GetBlockData(new Block(0, "Air", new byte[6] { 3, 3, 1, 2, 3, 3 }, false, true));
            GetBlockData(new Block(1, "grass", new byte[6] { 3, 3, 1, 2, 3, 3 },true,false));
            GetBlockData(new Block(2, "stone", new byte[6] { 16, 16, 16, 16, 16, 16 },true, false));
            GetBlockData(new Block(3, "dirt", new byte[6] { 1, 1, 1, 1, 1, 1 }, true, false));
            GetBlockData(new Block(4, "wood", new byte[6] { 4, 4, 4, 4, 4, 4 }, true, false));
            GetBlockData(new Block(5, "log", new byte[6] { 5, 5, 6, 6, 5, 5 }, true, false));
            GetBlockData(new Block(5, "log", new byte[6] { 6, 6, 7, 7, 7, 7 }, true, false));
            GetBlockData(new Block(5, "log", new byte[6] { 7, 7, 5, 5, 6, 6 }, true, false));
            GetBlockData(new Block(6, "glass", new byte[6] { 32, 32, 32, 32, 32, 32 }, false, false,true));
            GetBlockData(new Block(7, "leaf", new byte[6] { 33, 33, 33, 33, 33, 33 }, false, false, true));
            GetBlockData(new Block(8, "stair", new byte[6] { 4, 4, 4, 4, 4, 4 }, false, false, false));
        }
    }

    internal class Block
    {
        public int id { get; }
        public string name { get; }
        public byte[] material { get; } = new byte[6] { 0,0,0,0,0,0};

        public bool isAir = false;

        public bool isSolid = false;

        public bool isLucency = false;
        public Block(int id,string name, byte[] material,bool isSolid,bool isAir,bool isLucency = false) 
        { 
            this.id = id;
            this.name = name;
            this.isSolid = isSolid;
            this.isAir = isAir;
            this.isLucency = isLucency;
            for(int i = 0;i < material.Length; i++)
            {
                this.material[i] = material[i];
            }
        }
    }
}
