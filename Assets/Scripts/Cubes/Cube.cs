using UnityEngine;

namespace Cubes
{
    public class Cube
    {
        public bool isActive;
        public Vector3 position;

        public Cube()
        {
            this.isActive = false;
            this.position = Vector3.zero;
        }
        public Cube(Vector3 position)
        {
            this.isActive = true;
            this.position = position;
        }
    }
}