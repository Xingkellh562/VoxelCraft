using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Advanced;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using Image = SixLabors.ImageSharp.Image;
using VoxelCraft.Rendering;
using System;
using System.Reflection;

namespace VoxelCraft
{
    internal class CraftWindow<T> : GameWindow where T: IRender, new()
    {
        private T render = new T();

        WorldBlockData world = new WorldBlockData();

        readonly object _lock = new object();

        

        public CraftWindow(int width, int height,string title) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            ClientSize = new Vector2i(width, height),
            Title = title,
            API = ContextAPI.OpenGL,
            Profile = ContextProfile.Core,
            APIVersion = new Version(4, 5)
        })
        { VSync = VSyncMode.On;
        }
        protected override void OnLoad()
        {
            base.OnLoad();
            render?.Load();
            world.player.position = world.saveData.playerPos;
            world.player.dir = world.saveData.playerDir;
            CursorState = CursorState.Grabbed;

            Thread chunkThread = new Thread(GenChunk);
            //Thread updateThread = new Thread(Update);
            chunkThread.Start(_lock);
            //updateThread.Start();
            chunkThread.IsBackground = true;
            //updateThread.IsBackground = true;
        }
        protected override void OnUnload()
        {
            world.saveData.playerPos = world.player.position;
            world.saveData.playerDir = world.player.dir;
            SaveMgr.SaveData(world.saveData);
            render?.UnLoad();
            base.OnUnload();
            
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            world.Update(UpdateTime);
        }
        


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            (render as BaseRender).cameraPos = world.player.position;
            (render as BaseRender).cameraDir = world.player.dir;
            BaseRender.Clear();
            //ChunkMgr.GenChunk(world, (render as BaseRender).cameraPos, 16, 8);
            
            foreach (var chunk in world.data.Values)
            {
                chunk.Load();
                render?.Render(UpdateTime, chunk.gridData,false);
            }
            foreach (var chunk in world.data.Values)
            {
                chunk.Load();
                render?.Render(UpdateTime, chunk.gridDataLucency,true);
            }

            SwapBuffers();
        }

        void GenChunk(object? data)
        {
            while (true)
            {
                ChunkMgr.GenChunk(world, world.player.position, 16, 8);
                //ChunkMgr.UpdateChunk(world);
                
                Thread.Sleep(10);
            }
        }

        void Update()
        {
            while(true)
            {
                world.Update(UpdateTime);
                Thread.Sleep(10);
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            render?.Resize(ClientSize);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Keys.W) world.player.axisTransform.PressW = true;
            if (e.Key == Keys.S) world.player.axisTransform.PressS = true;
            if (e.Key == Keys.A) world.player.axisTransform.PressA = true;
            if (e.Key == Keys.D) world.player.axisTransform.PressD = true;
            if (e.Key == Keys.G) world.player.axisTransform.useGravity = !world.player.axisTransform.useGravity;
            if (e.Key == Keys.Space) world.player.axisTransform.PressSpace = true;
            if (e.Key == Keys.LeftShift) world.player.axisTransform.PressShift = true;
            if (e.Key == Keys.Escape) this.Close();

        }
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Keys.W) world.player.axisTransform.PressW = false;
            if (e.Key == Keys.S) world.player.axisTransform.PressS = false;
            if (e.Key == Keys.A) world.player.axisTransform.PressA = false;
            if (e.Key == Keys.D) world.player.axisTransform.PressD = false;
            
            if (e.Key == Keys.Space) world.player.axisTransform.PressSpace = false;
            if (e.Key == Keys.LeftShift) world.player.axisTransform.PressShift = false;
        }
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            Vector4 move = new Vector4(world.player.dir, 0) * world.player.axisTransform.MouseMoveX(0.5f, e.X);
            move *= world.player.axisTransform.MouseMoveY(0.5f, e.Y, world.player.dir);
            world.player.dir = move.Xyz;

        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.Button == MouseButton.Left)
                world.player.axisTransform.PressMouseLeft = true;
            if(e.Button == MouseButton.Right)
                world.player.axisTransform.PressMouseRight = true;
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.OffsetY > 0)
            {
                world.player.nowBlock -= 1;
                world.player.nowBlock =Math.Clamp(world.player.nowBlock, 1, 8);
            }
            else if(e.OffsetY < 0)
            {
                world.player.nowBlock += 1;
                world.player.nowBlock =Math.Clamp(world.player.nowBlock, 1, 8);
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButton.Left)
                world.player.axisTransform.PressMouseLeft = false;
            if (e.Button == MouseButton.Right)
                world.player.axisTransform.PressMouseRight = false;
        }
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (CraftWindow<BaseRender> window = new CraftWindow<BaseRender>(800, 600, "craft"))
            {
                window.Run();
            }
        }
    }

}