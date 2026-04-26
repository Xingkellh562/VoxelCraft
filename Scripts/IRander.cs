using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelCraft.Rendering
{
    /// <summary>
    /// 定义核心渲染功能,管理渲染设置等
    /// </summary>
    public interface IRender
    {
        void Load();                
        void Render(double deltaTime,GridData g,bool isLucencyGrid);
        void Resize(Vector2i size);
        void UnLoad();

        Vector3 ClearColor { get; set; }
    }
}
