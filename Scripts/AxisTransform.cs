using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using VoxelCraft.Rendering;

namespace VoxelCraft
{
    public class AxisTransform
    {
        private float fwdAxis = 0;
        private float rightAxis = 0;
        private float jumpAxis = 0;


        public bool PressW = false;
        public bool PressS = false;

        public bool PressA = false;
        public bool PressD = false;

        public bool PressSpace = false;
        public bool PressShift = false;

        public bool PressMouseLeft = false;
        public bool PressMouseRight = false;

        public float pastMouseX = 400;
        public float pastMouseY = 300;

        public bool useGravity = false;

        public const float moveScale = 0.1f;
        public AxisTransform() { }

        private void AxisWS(float updateTime, float coefficient)
        {
            if (PressW)
            {
                fwdAxis += updateTime * coefficient;
                if (fwdAxis > 1) fwdAxis = 1;
            }
            if (PressS)
            {
                fwdAxis -= updateTime * coefficient;
                if (fwdAxis < -1) fwdAxis = -1;
            }
            if (fwdAxis>0)
            {
                fwdAxis -= (updateTime * coefficient) / 2;
                if (fwdAxis < 0) fwdAxis = 0;
            }
            else 
            {
                fwdAxis += (updateTime * coefficient) / 2;
                if (fwdAxis > 0) fwdAxis = 0;
            }
        }
        private void AxisAD(float updateTime, float coefficient)
        {
            if (PressD)
            {
                rightAxis += updateTime * coefficient;
                if (rightAxis > 1) rightAxis = 1;
            }
            if (PressA)
            {
                rightAxis -= updateTime * coefficient;
                if (rightAxis < -1) rightAxis = -1;
            }
            if (rightAxis > 0)
            {
                rightAxis -= (updateTime * coefficient) / 2;
                if (rightAxis < 0) rightAxis = 0;
            }
            else
            {
                rightAxis += (updateTime * coefficient) / 2;
                if (rightAxis > 0) rightAxis = 0;
            }

        }
        private void AxisSpSh(float updateTime, float coefficient)
        {
            if (PressSpace)
            {
                jumpAxis += updateTime * coefficient;
                if (jumpAxis > 1) jumpAxis = 1;
            }
            if (PressShift)
            {
                jumpAxis -= updateTime * coefficient;
                if (jumpAxis < -1) jumpAxis = -1;
            }
            if (jumpAxis > 0)
            {
                jumpAxis -= (updateTime * coefficient) / 2;
                if (jumpAxis < 0) jumpAxis = 0;
            }
            else
            {
                jumpAxis += (updateTime * coefficient) / 2;
                if (jumpAxis > 0) jumpAxis = 0;
            }
        }
        public Vector3 Forward(Vector3 cameraDir,float speed)
        {
            return -Vector3.Cross(Vector3.Cross(cameraDir, Vector3.UnitY), Vector3.UnitY).Normalized() * speed * moveScale * fwdAxis;
        }
        public Vector3 Right(Vector3 cameraDir, float speed)
        {
            return Vector3.Cross(cameraDir,Vector3.UnitY).Normalized() * speed * moveScale * rightAxis;
        }
        public Vector3 Up(float speed)
        {
            return Vector3.UnitY * speed * moveScale * jumpAxis;
        }
        public Matrix4 MouseMoveX(float sensitivity, float mouseX) 
        {
            Vector3 arbitraryAxis = Vector3.UnitY;
            float angleRad = MathHelper.DegreesToRadians(sensitivity * (pastMouseX - mouseX));
            pastMouseX = mouseX;
            return Matrix4.CreateFromAxisAngle(arbitraryAxis, angleRad);
        }
        public Matrix4 MouseMoveY(float sensitivity, float mouseY,Vector3 cameraDir)
        {
            Vector3 arbitraryAxis = Vector3.Cross(cameraDir, Vector3.UnitY).Normalized();
            float angleRad = MathHelper.DegreesToRadians(sensitivity * (pastMouseY - mouseY));
            pastMouseY = mouseY;
            return Matrix4.CreateFromAxisAngle(arbitraryAxis, angleRad);
        }
        public void Update(float updateTime,float coefficient)
        {

            AxisWS(updateTime, coefficient);
            AxisAD(updateTime, coefficient);
            AxisSpSh(updateTime, coefficient);
            
        }
    }
}
