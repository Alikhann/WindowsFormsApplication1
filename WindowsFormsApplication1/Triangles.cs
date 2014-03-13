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
        
        Vector3 a, b, c;
        Vector3 u, v;

        int vertexID;
        int indexID;
        int normalID;

        Vector3[] vertices;
        uint[] indices;
        Vector3[] normals;
        Dictionary<Point, Vector3> normalsPerPoint = new Dictionary<Point, Vector3>();

        public Triangles() 
        {
            vertices = new Vector3[]
            {
                new Vector3(0.0f, 8.0f, 15.0f),
                new Vector3(15.0f, 8.0f, 0.0f),
                new Vector3(0.0f, 15.0f, 0.0f),
                new Vector3(15.0f, 8.0f, -15.0f),
                new Vector3(-15.0f, 8.0f, 0.0f)
            };
            indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2, 
                4, 0, 2,
                3, 4, 2
            };
            normals = new Vector3[vertices.Length];

            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 v0 = vertices[indices[i + 0]];
                Vector3 v1 = vertices[indices[i + 1]];
                Vector3 v2 = vertices[indices[i + 2]];

                Vector3 normal = Vector3.Normalize(Vector3.Cross(v2 - v0, v1 - v0));

                normals[indices[i + 0]] += normal;
                normals[indices[i + 1]] += normal;
                normals[indices[i + 2]] += normal;

            }
            for (int i = 0; i < vertices.Length; i ++)
            {
                normals[i] = Vector3.Normalize(normals[i])*-1;
                Console.WriteLine(normals[i]);
            }
            
          

            GL.GenBuffers(1, out vertexID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * Vector3.SizeInBytes), vertices, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out indexID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out normalID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.StaticDraw);

        }

        public Vector3[] Normal
        {
            get
            {
                for(int i = 0; i < indices.Length; i += 3)
                {
                    Vector3 v0 = vertices[indices[i + 0]];
                    Vector3 v1 = vertices[indices[i + 1]];
                    Vector3 v2 = vertices[indices[i + 2]];

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(v2 - v0, v1 - v0));

                    normals[indices[i + 0]] += normal;
                    normals[indices[i + 1]] += normal;
                    normals[indices[i + 2]] += normal;
                }
                    
                    return normals;
            }
        }
        
        public void draw()
        {
            //GL.Begin(PrimitiveType.Triangles);
            //GL.Color3(Color.Green);

            //GL.Vertex3(0, 8, 15);   //v1
            //GL.Vertex3(15, 8, 0);   //v2
            //GL.Vertex3(0, 15, 0);   //v3
            //GL.Vertex3(15, 8, -15);
            //GL.Vertex3(-15, 8, 0);
            //GL.End();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexID);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.EnableClientState(ArrayCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, normalID);
            GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, 0);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexID);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.Color3(Color.White);
            GL.LineWidth(2f);
            GL.DrawElements(PrimitiveType.LineStrip, indices.Length, DrawElementsType.UnsignedInt, 0);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);

           
         }



    }
}
