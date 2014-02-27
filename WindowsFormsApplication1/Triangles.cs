using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
namespace WindowsFormsApplication1
{
    class Triangles
    {
        public Triangles() { }
        Vector3 a, b, c;

        public void draw()
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.Green);

            GL.Vertex3(0, 8, 15);
            GL.Vertex3(15, 8, 0);
            GL.Vertex3(0, 15, 0);
            //GL.Vertex3(15, 8, -15);
            //GL.Vertex3(-15, 8, 0);
            GL.End();
         }
        public Vector3 Normal
        {
            get
            {
                var dir = Vector3.Cross(b-a, c-a);
                return dir;
            }
        }
    }
}
