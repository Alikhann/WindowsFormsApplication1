using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WindowsFormsApplication1
{

    class Axes
    {
        public static Matrix4 projection;
        Camera dCam;
        bool loaded = false;
        static float w = 0.22f;
        static float ww = 0.1f;
        Matrix4 mv, mvp;

        int pointID, colorID;


        float[] points = {                    
                    -0.5f, -w, 0.5f,  
                    0, -w, 0,
                    -0.5f, w, 0.5f,
                    0, w, 0,

                    0, -w, 0,
                    0.5f, -w, 0.5f,
                    0, w, 0,
                    0.5f, w, 0.5f,
                    
                    0.5f, -w, 0.5f,
                    0, -ww, -1,
                    0.5f, w, 0.5f,
                    0, ww, -1,


                    0, -ww, -1,
                    -0.5f, -w, 0.5f,
                    0, ww, -1,
                    -0.5f, w, 0.5f,
                    
                    -0.5f, w, 0.5f,
                    0, ww, -1,
                    0, w, 0,
                    0.5f, w, 0.5f,


                    0, -w, -0.001f,
                    0, -ww, -1,
                                        
                    
                    0.5f, -w, 0.5f,
                    0, -w, -0.001f,
                    0, -ww, -1,
                    -0.5f, -w, 0.5f,                                                                                                    
                         };

        float[] colors = {
                    0,1,1,1,   
                    0,0,1,1, 
                    1,0,0,1, 
                    1,1,0,1, 

                    0,0,1,1, 
                    0,1,1,1, 
                    1,1,0,1, 
                    1,0,0,1, 
                    
                    0,1,1,1, 
                    1,1,1,1, 
                    1,0,0,1, 
                    1,1,1,1, 


                    1,1,1,1, 
                    0,1,1,1, 
                    1,1,1,1, 
                    1,0,0,1, 
                    
                    1,0,0,1, 
                    1,1,1,1, 
                    1,1,0,1, 
                    1,0,0,1,

                    0,0,1,1, 
                    1,1,1,1,
                    
                    0,1,1,1, 
                    0,0,1,1, 
                    1,1,1,1, 
                    0,1,1,1,   
                  };

        public Axes()
        {
            mv = Matrix4.LookAt(new Vector3(0, 0, 2f), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        }

        public static void UpdateProjection(float fov, float aspect, float near, float far, GLControl c)
        {
            projection = Matrix4.CreatePerspectiveOffCenter(-0.97f, 0.04f, 0.04f, -0.97f / aspect, 1, 100);
            //projection = Matrix4.CreatePerspectiveFieldOfView(fov, aspect, 1, 100);
        }

        //int myMegaCoolShader3000, locMVP;

        public void Render()
        {
            //GL.PushMatrix();   
          //  GL.UseProgram(myMegaCoolShader3000);
          //  GL.UniformMatrix4(locMVP, false, ref mvp);
          //  dikkins.Draw();
            //GL.PopMatrix();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, points.Length);
        }

        public void camch()
        {
            GL.PushMatrix();
            Matrix4 nmv;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref mv);
            //GL.Translate(1.1f, -0.65f, 0);
            GL.Scale(0.08f, -0.08f, 0.08f);
            float dca = (dCam.alpha - 3.1415f / 2.0f); // -п/2            
            //how deep do YOU know the pi?
            GL.Rotate(MathHelper.RadiansToDegrees(dCam.alpha) - Camera.InitialAngleDec, 0, 1, 0);
            GL.Rotate(MathHelper.RadiansToDegrees(-dCam.alphay) + Camera.InitialAngleDec, (float)Math.Cos(dca), 0, (float)Math.Sin(dca));
            GL.GetFloat(GetPName.ModelviewMatrix, out nmv);
            mvp = Matrix4.Mult(nmv, projection);
            GL.PopMatrix();
        }

        public void Prepare(Camera c)
        {
            dCam = c;
            GL.GenBuffers(1, out pointID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, pointID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(points.Length * sizeof(float)), points, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out colorID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, colorID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(colors.Length *  sizeof(float)), colors, BufferUsageHint.StaticDraw);

            //dikkins = new GLBatch();
            //dikkins.Begin(OpenTK.Graphics.OpenGL.BeginMode.TriangleStrip, 0);
            //dikkins.CopyVertexData3f(points);
            //dikkins.CopyColorData4f(colors);
            //dikkins.End();
            //myMegaCoolShader3000 = OpenGL.loadShaderPairWithAttr("DumbShader.vp", "DumbShader.fp", new int[] { 0, 1 }, new string[] { "vVertex", "vColor" });
            //locMVP = GL.GetUniformLocation(myMegaCoolShader3000, "mvpMatrix");
        }

    }
}
