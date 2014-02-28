using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        bool loaded = false;
        //float x = -1.0f, y = -1.0f, z = 1.0f;

        

        string[] str;
        int xx = 0, yy = 0, zz = 0;

        static float CAMERA_FOVY = 45.0f;
        static float CAMERA_ZFAR = 1000.0f;
        static float CAMERA_ZNEAR = 0.1f;

        static float MOUSE_ORBIT_SPEED = 0.30f;
        static float MOUSE_DOLLY_SPEED = 0.02f;
        static float MOUSE_TRACK_SPEED = 0.05f;

        ECameraMode camera_mode = ECameraMode.CAMERA_NONE;

        Point mouse_previous = new Point();
        Point mouse_current = new Point();
        bool isMouseDown = false;

        float[] g_cameraPos = new float[3];
        float[] g_targetPos = new float[3];
        float g_heading;
        float g_pitch;
        float dx = 0.0f;
        float dy = 0.0f;

        Vector3[] verticess;
        Vector3[] normals;
        uint[] indicess;


        //---------------for reading file---------------
        Vector3d[] newvert;
        uint[] index;
        List<uint> list = new List<uint>();
        int number1, number2;
        string[] values;
        List<Vector3d> listOfVertices = new List<Vector3d>();
        string fileName = "G:/test/input.txt";
        //-------------------end-----------------------
        int elementCount = 0; //count of tri element to be drawn

    
        int v_position;
        int i_elements;
        int norm;
        int bufferSize;
        int size;


        int cnt = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true;

            glControl1.Dock = DockStyle.Fill;
            ResetCamera();

            this.MouseWheel += new MouseEventHandler(glControl1_MouseWheelChanged);
            this.MouseMove += new MouseEventHandler(glControl1_MouseWheelChanged);
            //ch

            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            /*
            using (StreamReader read = new StreamReader(@"G:\test\input.txt"))
            {
                string line;
                while ((line = read.ReadLine()) != null)
                {
                str = line.Split(' ');
                }
                //xx = Convert.ToInt32(str[0]);
                //yy = Convert.ToInt32(str[1]);
               // zz = Convert.ToInt32(str[2]);
            }
            */

            number1 = Convert.ToInt32(File.ReadAllLines(@fileName).First());
            number2 = Convert.ToInt32(File.ReadAllLines(@fileName).Skip(number1 + 1).Take(1).First());
            String[] lines = File.ReadAllLines(@fileName);
           
            
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            
            for(int i = 1; i <= number1; i++)
            {
                values = lines[i].Split(' ');
                var x = float.Parse(values[0], NumberStyles.Any, ci);
                var y = float.Parse(values[1], NumberStyles.Any, ci);
                var z = float.Parse(values[2], NumberStyles.Any, ci)*500;

                listOfVertices.Add(new Vector3d(x, y, z));
            }
            for(int i = number1 + 2; i<lines.Length; i++)
            {
                values = lines[i].Split(' ');
                list.Add(uint.Parse(values[0]));
                list.Add(uint.Parse(values[1]));
                list.Add(uint.Parse(values[2]));
            }
            /*
            for (int i = 1; i < lines.Length; i++)
            {
                //if (i == 0) continue; 
                if (i <= number1)
                {
                    values = lines[i].Split(' ');
                    
                    x = float.Parse(values[0], NumberStyles.Any, ci);
                    y = float.Parse(values[1], NumberStyles.Any, ci);
                    z = float.Parse(values[2], NumberStyles.Any, ci);

                    listOfVertices.Add(new Vector3d(x, y, z));
                    //newvert[i] = new Vector3d(x, y, z);
                }
                else if (i >= number1 + 2)
                {
                    values = lines[i].Split(' ');
                    list.Add(uint.Parse(values[0]));
                    list.Add(uint.Parse(values[1]));
                    list.Add(uint.Parse(values[2]));
                }
            }*/
            newvert = listOfVertices.ToArray();
            index = list.ToArray();


            Console.WriteLine(number1 + " " + number2);
            Console.WriteLine(index.Length);
            foreach (var v in index)
                Console.Write(v + "\t");

            
            
            GL.ClearColor(Color.Black);
            GL.PointSize(5f);
            
            #region------Vertices and Normals----------
            verticess = new Vector3[]
            {
                // Front face
				new Vector3 (-1.0f, -1.0f, 1.0f), 
				new Vector3 (1.0f, -1.0f, 1.0f), 
				new Vector3 (1.0f, 1.0f, 1.0f), 
				new Vector3 (-1.0f, 1.0f, 1.0f),
				// Right face
				new Vector3 (1.0f, -1.0f, 1.0f), 
				new Vector3 (1.0f, -1.0f, -1.0f), 
				new Vector3 (1.0f, 1.0f, -1.0f), 
				new Vector3 (1.0f, 1.0f, 1.0f),
				// Back face
				new Vector3 (1.0f, -1.0f, -1.0f), 
				new Vector3 (-1.0f, -1.0f, -1.0f), 
				new Vector3 (-1.0f, 1.0f, -1.0f), 
				new Vector3 (1.0f, 1.0f, -1.0f),
				// Left face
				new Vector3 (-1.0f, -1.0f, -1.0f), 
				new Vector3 (-1.0f, -1.0f, 1.0f), 
				new Vector3 (-1.0f, 1.0f, 1.0f), 
				new Vector3 (-1.0f, 1.0f, -1.0f),
				// Top Face	
				new Vector3 (-1.0f, 1.0f, 1.0f), 
				new Vector3 (1.0f, 1.0f, 1.0f),
				new Vector3 (1.0f, 1.0f, -1.0f), 
				new Vector3 (-1.0f, 1.0f, -1.0f),
				// Bottom Face
				new Vector3 (1.0f, -1.0f, 1.0f), 
				new Vector3 (-1.0f, -1.0f, 1.0f),
				new Vector3 (-1.0f, -1.0f, -1.0f), 
				new Vector3 (1.0f, -1.0f, -1.0f),
            };

            normals = new Vector3[]
            {
                // Front face
				new Vector3 ( 0f, 0f, 1f), 
				new Vector3 ( 0f, 0f, 1f),
				new Vector3 ( 0f, 0f, 1f),
				new Vector3 ( 0f, 0f, 1f), 
				// Right face
				new Vector3 ( 1f, 0f, 0f), 
				new Vector3 ( 1f, 0f, 0f), 
				new Vector3 ( 1f, 0f, 0f), 
				new Vector3 ( 1f, 0f, 0f),
				// Back face
				new Vector3 ( 0f, 0f, -1f), 
				new Vector3 ( 0f, 0f, -1f), 
				new Vector3 ( 0f, 0f, -1f),  
				new Vector3 ( 0f, 0f, -1f), 
				// Left face
				new Vector3 ( -1f, 0f, 0f),  
				new Vector3 ( -1f, 0f, 0f), 
				new Vector3 ( -1f, 0f, 0f),  
				new Vector3 ( -1f, 0f, 0f),
				// Top Face	
				new Vector3 ( 0f, 1f, 0f),  
				new Vector3 ( 0f, 1f, 0f), 
				new Vector3 ( 0f, 1f, 0f),  
				new Vector3 ( 0f, 1f, 0f),
				// Bottom Face
				new Vector3 ( 0f, -1f, 0f),  
				new Vector3 ( 0f, -1f, 0f), 
				new Vector3 ( 0f, -1f, 0f),  
				new Vector3 ( 0f, -1f, 0f)
            };

            indicess = new uint[]
            {
                /*
                // Font face
				0, 1, 2, 2, 3, 0, 
				// Right face
				7, 6, 5, 5, 4, 7, 
				// Back face
				11, 10, 9, 9, 8, 11, 
				// Left face
				15, 14, 13, 13, 12, 15, 
				// Top Face	
				19, 18, 17, 17, 16, 19,
				// Bottom Face
				23, 22, 21, 21, 20, 23
              */
             0, 1, 2, 3, 
             4, 5, 6, 7, 
             8, 9, 10, 11, 
             12, 13, 14, 15, 
             16, 17, 18, 19, 
             20, 21, 22, 23
              
            };

            #endregion

            #region temp
            /*
            size = xx * yy * zz * 24;
            newvert = new Vector3[size];

            
            for (int i = 0; i < zz; i++)
            {
                for (int j = 0; j < yy; j++)
                {
                    for (int k = 0; k < xx; k++)
                    {
                        for (int l = 0; l < vertices.Length; l++)
                        {
                            vertices[l].X += x;
                            vertices[l].Y += y;
                            vertices[l].Z += z;
                        }

                            x++;
                    }
                    y++;
                    x = -1.0f;
                }
                z--;
                y = -1.0f;
            }
            /*
            Parallel.For(0, newvert.Length, i =>
                {
                    Console.WriteLine("Go: {0}", i);
                });*/

            //-------if I declare second Vector3[] for new vertices...
            //so, let's imagine, how it should look like
            //


            /*
            for (int i = 0; i < newvert.Length; i += 24)
                for (int j = 0; j < vertices.Length; j++ )
                { 
                    //some math logic for generating other vertices
                    newvert.Concat(vertices);
                }
            */
            #endregion

            //----------------Vertex Array Buffer---------------------
            {
                GL.GenBuffers(1, out v_position);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, (IntPtr)(newvert.Length * Vector3d.SizeInBytes), newvert, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (newvert.Length * Vector3d.SizeInBytes != bufferSize)
                    Console.WriteLine("Vertex array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }

            //----------------Normal Array Buffer--------------------- 
            //if(normals != null)
            //{
            //    GL.GenBuffers(1, out norm);
            //    GL.BindBuffer(BufferTarget.ArrayBuffer, norm);
            //    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.StaticDraw);
            //    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
            //    if (normals.Length * Vector3.SizeInBytes != bufferSize)
            //        Console.WriteLine("Normal array not uploaded correctly!");

            //    //clear the buffer Binding
            //    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //}
            //----------------Element Array Buffer---------------------
            {   
                GL.GenBuffers(1, out i_elements);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(index.Length * sizeof(uint)), index, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (index.Length * sizeof(uint) != bufferSize)
                    Console.WriteLine("Element array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            }

            elementCount = index.Length;

        }
        

        
        void Draw()
        {
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            gluLookAt(g_cameraPos[0], g_cameraPos[1], g_cameraPos[2], g_targetPos[0], g_targetPos[1], g_targetPos[2], 0.0f, 1.0f, 0.0f);
            GL.Rotate(g_pitch, 1.0f, 0.0f, 0.0f);
            GL.Rotate(g_heading, 0.0f, 1.0f, 0.0f);

            GL.Disable(EnableCap.Lighting);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(20.0f, 0.0f, 0.0f);
            GL.End();
            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 20.0f, 0.0f);
            GL.End();
            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 20.0f);
            GL.End();

            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
           
            
            
            //Vertex Array Buffer
            {
            //    GL.Color3(Color.Red);
            //    GL.EnableClientState(ArrayCap.VertexArray);
            //    GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                
            //    GL.VertexPointer(3, VertexPointerType.Double, 0, 0);
                
            //    //GL.DrawArrays(PrimitiveType.Points, 0, newvert.Length);
            //}

            ////Element Array Buffer
            //{
            //   // GL.EnableClientState(ArrayCap.IndexArray);
            //    GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);

            //   // GL.Enable(EnableCap.Lighting);
            //    //GL.Enable(EnableCap.Light0);
            //    //GL.LightModel(LightModelParameter.LightModelTwoSide, 0);

            //    //GL.BindBuffer(BufferTarget.ArrayBuffer, norm);
            //    //GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, 0);
            //    //GL.EnableClientState(ArrayCap.NormalArray);
                
            //    //GL.FrontFace(FrontFaceDirection.Ccw);
            //    GL.PointSize(3f);
            //   GL.DrawElements(PrimitiveType.Points, elementCount, DrawElementsType.UnsignedInt, 0);
            //    GL.Color3(Color.Turquoise);
            //    GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, 0);
                Triangles tr = new Triangles();
                tr.draw();
                GL.Color3(Color.White);
                GL.LineWidth(2.0f);
                //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                //GL.DrawElements(PrimitiveType.LineStrip, elementCount, DrawElementsType.UnsignedInt, 0);
            }

            GL.DisableClientState(ArrayCap.VertexArray);
           // GL.DisableClientState(ArrayCap.NormalArray);
               
            glControl1.SwapBuffers();
        }
        private void glControl1_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("Resize time: " + DateTime.Now);

            if (Width != 0 && Height != 0)
            {
                GL.Viewport(0, 0, Width, Height);

                double aspectRatio = Width / (double)Height;

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();

                gluPerspective(CAMERA_FOVY, aspectRatio, CAMERA_ZNEAR, CAMERA_ZFAR);

                glControl1.Invalidate();
            }
        }
        internal void gluPerspective(double fovy, double aspect, double zNear, double zFar)
        {
            /*
            double xmin, xmax, ymin, ymax;

            ymax = zNear * Math.Tan(fovy * Math.PI / 360.0);
            ymin = -ymax;

            xmin = ymin * aspect;
            xmax = ymax * aspect;

            GL.Frustum(xmin, xmax, ymin, ymax, zNear, zFar);//left, right, bottom, top
             */
            Matrix4d m = Matrix4d.Identity;
            double sine, cotangent, deltaZ;
            double radians = fovy / 2 * Math.PI / 180;

            deltaZ = zFar - zNear;
            sine = Math.Sin(radians);
            if ((deltaZ == 0) || (sine == 0) || (aspect == 0))
            {
                return;
            }
            //TODO: check why this cos was written COS?
            cotangent = Math.Cos(radians) / sine;

            m.M11 = cotangent / aspect;
            m.M22 = cotangent;
            m.M33 = -(zFar + zNear) / deltaZ;
            m.M34 = -1;
            m.M43 = -2 * zNear * zFar / deltaZ;
            m.M44 = 0;

            GL.MultMatrix(ref m);
        }
        Vector3 forward = new Vector3();
        Vector3 up = new Vector3();
        Vector3 right = new Vector3();
        void gluLookAt(float eyex, float eyey, float eyez, float centerx, float centery, float centerz, float upx, float upy, float upz)
        {
            //float[] m = new float[16];
            Matrix4 m;
            forward.X = centerx - eyex;
            forward.Y = centery - eyey;
            forward.Z = centerz - eyez;

            up.X = upx;
            up.Y = upy;
            up.Z = upz;

            forward.Normalize();

            /* Side = tForward x tUp */
            Vector3.Cross(ref forward, ref up, out right);

            right.Normalize();

            /* Recompute tUp as: tUp = tRight x tForward */
            Vector3.Cross(ref right, ref forward, out up);
            /*
            // set right vector
            m[0] = right.X; m[1] = up.X; m[2] = -forward.X; m[3] = 0;
            // set up vector
            m[4] = right.Y; m[5] = up.Y; m[6] = -forward.Y; m[7] = 0;
            // set forward vector
            m[8] = right.Z; m[9] = up.Z; m[10] = -forward.Z; m[11] = 0;
            // set translation vector
            m[12] = 0; m[13] = 0; m[14] = 0; m[15] = 1;
            */
            m = Matrix4.Identity;
            m.M11 = right.X;
            m.M21 = right.Y;
            m.M31 = right.Z;

            m.M21 = up.X;
            m.M22 = up.Y;
            m.M23 = up.Z;

            m.M31 = -forward.X;
            m.M32 = -forward.Y;
            m.M33 = -forward.Z;
            GL.MultMatrix(ref m);
              
            GL.Translate(-eyex, -eyey, -eyez);
        }
        void ResetCamera()
        {
            g_targetPos[0] = 0;
            g_targetPos[1] =10;
            g_targetPos[2] = 10;

            g_cameraPos[0] = g_targetPos[0];
            g_cameraPos[1] = g_targetPos[1];
            g_cameraPos[2] = g_targetPos[2] + 100 + CAMERA_ZNEAR;

            g_pitch = 0.0f;
            g_heading = 0.0f;
        }
        enum ECameraMode
        {
            CAMERA_NONE, CAMERA_TRACK, CAMERA_DOLLY, CAMERA_ORBIT
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("keydown");
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Left:
                    g_heading += 90.0f;
                    break;
                case Keys.Right:
                    g_heading += -90.0f;
                    break;
                case Keys.Up:
                    g_pitch += 90.0f;
                    break;
                case Keys.Down:
                    g_pitch += -90.0f;
                    break;
            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            OnKeyDown(new KeyEventArgs(keyData));
            return base.ProcessDialogKey(keyData);
        }
        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            isMouseDown = true;
            switch (e.Button)
            {
                case MouseButtons.Right:
                    camera_mode = ECameraMode.CAMERA_TRACK;
                    break;
                case MouseButtons.Middle:
                    camera_mode = ECameraMode.CAMERA_DOLLY;
                    break;
                case MouseButtons.Left:
                    camera_mode = ECameraMode.CAMERA_ORBIT;
                    break;
            }

            mouse_previous.X = MousePosition.X;
            mouse_previous.Y = MousePosition.Y;
        }
        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            camera_mode = ECameraMode.CAMERA_NONE;

            isMouseDown = false;
        }
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            mouse_current.X = MousePosition.X;
            mouse_current.Y = MousePosition.Y;
            
            switch (camera_mode)
            {
                case ECameraMode.CAMERA_TRACK:

                    dx = mouse_current.X - mouse_previous.X;
                    dx *= MOUSE_TRACK_SPEED;
                    dy = mouse_current.Y - mouse_previous.Y;
                    dy *= MOUSE_TRACK_SPEED;

                    g_cameraPos[0] -= dx;
                    g_cameraPos[1] += dy;

                    g_targetPos[0] -= dx;
                    g_targetPos[1] += dy;

                    break;

                case ECameraMode.CAMERA_DOLLY:
                
                    dy = mouse_current.Y - mouse_previous.Y;
                    dy *= MOUSE_DOLLY_SPEED;

                    g_cameraPos[2] += dy;
                    
                   // glControl1_MouseWheelChanged(sender, e);
                    break;

                case ECameraMode.CAMERA_ORBIT:

                    dx = mouse_current.X - mouse_previous.X;
                    dx += MOUSE_ORBIT_SPEED;

                    dy = mouse_current.Y - mouse_previous.Y;
                    dy += MOUSE_ORBIT_SPEED;

                    g_heading += dx;
                    g_pitch += dy;
                    
                    break;
            }
            mouse_previous.X = mouse_current.X;
            mouse_previous.Y = mouse_current.Y;

            if (isMouseDown)
            {
                glControl1.Refresh();
            }

        }
        private void glControl1_MouseWheelChanged(Object sender, MouseEventArgs e)
        {
        }
                      
    }
}