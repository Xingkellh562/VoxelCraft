using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace VoxelCraft
{
    internal class TextureLoader
    {
        public TextureLoader() { }
        public string filePath = AppContext.BaseDirectory + "Resourges\\blockatlas.png";

        public int imageX;
        public int imageY;

        public byte[] pixelData;

        public void LoadTexture()
        {
            using (var image = Image.Load<Rgba32>(filePath))
            {
                pixelData = GetPixelData(image);
                imageX = image.Width;
                imageY = image.Height;
            }
        }
        private static byte[] GetPixelData(Image<Rgba32> image)
        {
            var bytes = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(bytes);
            return bytes;
        }
    }
}
