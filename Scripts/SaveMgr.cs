using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using OpenTK.Mathematics;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace VoxelCraft
{
    public class SaveMgr
    {


        static XmlReaderSettings settingsR = new XmlReaderSettings
        {
            IgnoreComments = true,          
            IgnoreWhitespace = true,        
            DtdProcessing = DtdProcessing.Ignore, 
        };

        static XmlWriterSettings settingsW = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8,
            NewLineOnAttributes = false,
            OmitXmlDeclaration = false,
        };
        public static bool GetBlock(int x,int y,int z, SaveData data, out int b)
        {
            ConcurrentDictionary<Vector3i, int>  blockData = data.blockData;
            b = 0;
            if (blockData.TryGetValue(new Vector3i(x,y,z),out int block)){
                b = block;
                return true;
            }
            return false;
        }
        public static void AddBlock(int x, int y, int z,SaveData data, int type)
        {
            ConcurrentDictionary<Vector3i, int> blockData = data.blockData;
            Vector3i v = new Vector3i(x, y, z);
            if (blockData.ContainsKey(v))
                blockData[v] = type;
            else
                blockData.TryAdd(v, type);
        }

        public static bool SaveData(SaveData data)
        {
            try
            {
                File.WriteAllText(data.filePath+"BlockData.xml","");
                using (XmlWriter writer = XmlWriter.Create(data.filePath +"BlockData.xml", settingsW))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("BlockData");

                    var snapshot = data.blockData.ToArray();
                    foreach (var kvp in snapshot)
                    {
                        writer.WriteStartElement("block");
                        writer.WriteAttributeString("x", kvp.Key.X.ToString());
                        writer.WriteAttributeString("y", kvp.Key.Y.ToString());
                        writer.WriteAttributeString("z", kvp.Key.Z.ToString());
                        writer.WriteAttributeString("type", kvp.Value.ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                using (XmlWriter writer = XmlWriter.Create(data.filePath + "PlayerData.xml", settingsW))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("PlayerData");
                    
                    writer.WriteStartElement("playerPos");
                    writer.WriteAttributeString("x", data.playerPos.X.ToString());
                    writer.WriteAttributeString("y", data.playerPos.Y.ToString());
                    writer.WriteAttributeString("z", data.playerPos.Z.ToString());
                    writer.WriteEndElement();

                    writer.WriteStartElement("playerDir");
                    writer.WriteAttributeString("x", data.playerDir.X.ToString());
                    writer.WriteAttributeString("y", data.playerDir.Y.ToString());
                    writer.WriteAttributeString("z", data.playerDir.Z.ToString());
                    writer.WriteEndElement();


                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                return true;
            }
            catch 
            {
                Console.WriteLine("保存失败,请检查文件路径是否合法");
                return false;
            }
        }
        public static bool LoadData(SaveData data)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(data.filePath + "BlockData.xml", settingsR))
                {

                    while (reader.ReadToFollowing("block"))
                    {
                        Vector3i v = new Vector3i();
                        v.X = int.Parse(reader.GetAttribute("x"));
                        v.Y = int.Parse(reader.GetAttribute("y"));
                        v.Z = int.Parse(reader.GetAttribute("z"));
                        data.blockData[v] = int.Parse(reader.GetAttribute("type"));
                    }
                }
                using (XmlReader reader = XmlReader.Create(data.filePath + "PlayerData.xml", settingsR))
                {

                    while (reader.ReadToFollowing("playerPos"))
                    {
                        Vector3 v = new Vector3();
                        v.X = float.Parse(reader.GetAttribute("x"));
                        v.Y = float.Parse(reader.GetAttribute("y"));
                        v.Z = float.Parse(reader.GetAttribute("z"));
                        data.playerPos = v;
                    }
                    while (reader.ReadToFollowing("playerDir"))
                    {
                        Vector3 v = new Vector3();
                        v.X = float.Parse(reader.GetAttribute("x"));
                        v.Y = float.Parse(reader.GetAttribute("y"));
                        v.Z = float.Parse(reader.GetAttribute("z"));
                        data.playerDir = v;
                    }
                }
                return true;
            }
            catch
            {
                Console.WriteLine("读取失败,请检查文件是否存在");
                return false;
            }

        }
        
    }

    public class SaveData
    {
        public Vector3 playerPos = new Vector3(0, 150, 0);
        public Vector3 playerDir = new Vector3(0, 0, -1);
        public ConcurrentDictionary<Vector3i, int> blockData = new ConcurrentDictionary<Vector3i, int>();
        public string filePath = "";
    }
}
