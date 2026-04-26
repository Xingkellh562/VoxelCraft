
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VoxelCraft.Rendering
{
    public class BaseRender : IRender
    {
        private BaseShader? _shader;

        //public List<byte[]> grids = new List<byte[]>();
        //public Vector3i chunkPos = new Vector3i();

        private Vector2i windowSize = new Vector2i(800,600);
        public Vector3 ClearColor { get ; set; } = new Vector3(0.8f, 0.8f, 1);

        //摄像机数据
        public Vector3 cameraPos = new Vector3(0, 150, 0);
        public Vector3 cameraDir = new Vector3(0, 0, -1);

        /// <summary>
        /// 加载着色器纹理等
        /// </summary>
        public void Load()
        {
            //启用一些设置
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(TriangleFace.Front);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);


            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, 1.0f);

            Console.WriteLine("加载编译器......");
            //这中间部分是加载编译器的代码
            _shader = new BaseShader();
            _shader.LoadShader();

            Console.WriteLine("编译器加载成功......");
        }
        /// <summary>
        /// 负责画面更新
        /// </summary>
        /// <param name="deltaTime"></param>//每帧用时
        public void Render(double deltaTime, GridData g, bool isLucencyGrid)
        {
            if(isLucencyGrid)
            {
                GL.Enable(EnableCap.Blend);
                //GL.Disable(EnableCap.CullFace);
                //GL.DepthMask(false);
            }
            else
            {
                GL.Disable(EnableCap.Blend);
                //GL.Enable(EnableCap.CullFace);
                //GL.DepthMask(false);
            }
            _shader?.GenVertices(g._chunkVao,g._chunkVbo,g._blockVertices,g.chunkPos);
            //重设变换矩阵
            _shader?.GetMatrix(cameraPos, cameraDir, windowSize);
            
            _shader?.Draw();
        }
        /// <summary>
        /// 窗口大小改变
        /// </summary>
        /// <param name="size"></param>
        public void Resize(Vector2i size)
        {
            windowSize = size;
            GL.Viewport(0, 0, size.X, size.Y);
        }

        public static void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        /// <summary>
        /// 窗口关闭
        /// </summary>
        public void UnLoad()
        {
            _shader?.Dispose();
        }
    }
}
