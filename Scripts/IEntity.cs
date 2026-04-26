using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoxelCraft
{
    public interface IEntity
    {
        public void MoveForward();
    }

    public class Entity : IEntity
    {
        public Vector3 position;
        public Vector3 dir;
        public void MoveForward()
        {

        }
    }

    public class Player: Entity
    {
        public AxisTransform axisTransform = new AxisTransform();

        public int nowBlock = 4;

        public PhysicMgr body = new PhysicMgr();

        public BoxAABB box = new BoxAABB(new Vector3(-0.2f, -1.5f, -0.2f), new Vector3(0.2f, 0.3f, 0.2f));
    }

}
