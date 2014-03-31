using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Globalization;
using System.Linq;

namespace WindowsFormsApplication1
{
    public partial class AliGL : Form
    {
        #region ------declaring------
        bool loaded = false;
        bool isMouseDown;

        Vector3d[] vertices;
        uint[] indices;
        Vector3[] normals;
        Vector3[] vertexnormals;
        Dictionary<Vector3, Vector3> perPoint = new Dictionary<Vector3, Vector3>();
 
        //---------------for reading file---------------
        
        List<uint> list = new List<uint>();
        int number1, number2;
        string[] values;
        List<Vector3d> listOfVertices = new List<Vector3d>();
        string fileName = "C:/input.txt";
        //-------------------end-----------------------

        int elementCount = 0; //count of tri element to be drawn
        Camera cam;
        Matrix4 cameramatrix;
        
        public static float zfar, znear;
        public static Matrix4 projection;

        public static Matrix4 defaultMVP;
        public static Matrix4 defaultModelview;
        public static Matrix4 defaultNormal;
    
        int v_position;
        int i_elements;
        int norm;
        int bufferSize;
        int line_in;
        float a;

        Axes axe = new Axes();

        float xxx;

        Vector3d[] boxvertices;
        uint[] boxindices;
        int box_id;
        int boxid;

        double xmin, xmax, ymin, ymax, zmin, zmax;
        Vector3 cent, dist;
        #endregion

        public AliGL()
        {
            InitializeComponent();
            cam = new Camera(glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);
            this.cameramatrix = cam.cameraMatrix;
            cam.camch += cam_camch;
        }
        void cam_camch(Matrix4 cameramatrix)
        {
            this.cameramatrix = cameramatrix;
            defaultModelview = cameramatrix;
        }
        
        public static Vector3 ToVector3(Vector3d input)
        {
            return new Vector3((float)input.X, (float)input.Y, (float)input.Z);
        }
        private void glControl1_Load(object sender, EventArgs e)
        {
            loaded = true;
            //ResetCamera();.
            glControl1.MouseDown += new MouseEventHandler(glControl1_MouseDown);
            glControl1.MouseMove += glControl1_MouseMove;
            glControl1.MouseUp += glControl1_MouseUp;
            glControl1.MouseWheel += glControl1_MouseWheelChanged;
            
            //Alikhan Nugmanov
            GL.Enable(EnableCap.DepthTest);
           
            GL.Enable(EnableCap.ColorMaterial);

            GL.ShadeModel(ShadingModel.Flat);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 0.0f, 1.0f, 1.0f });
            //GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.5f, 0.5f, 0.5f });
            //GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.SpotExponent, 0.0f);
            GL.LightModel(LightModelParameter.LightModelTwoSide, 0.5f);

            #region reading from file
            number1 = Convert.ToInt32(File.ReadAllLines(@fileName).First());
            number2 = Convert.ToInt32(File.ReadAllLines(@fileName).Skip(number1 + 1).Take(1).First());
            String[] lines = File.ReadAllLines(@fileName);

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            for (int i = 1; i <= number1; i++)
            {
                double x, y, z;
                values = lines[i].Split(' ');
                double.TryParse(values[0], NumberStyles.Any, ci, out x);
                double.TryParse(values[1], NumberStyles.Any, ci, out y);
                double.TryParse(values[2], NumberStyles.Any, ci, out z); ;
                listOfVertices.Add(new Vector3d(x, y, z));
            }
            for (int i = number1 + 2; i < lines.Length; i++)
            {
                values = lines[i].Split(' ');
                list.Add(uint.Parse(values[0]));
                list.Add(uint.Parse(values[1]));
                list.Add(uint.Parse(values[2]));
            }
            vertices = listOfVertices.ToArray();
            indices = list.ToArray();
            normals = new Vector3[vertices.Length];
            vertexnormals = new Vector3[vertices.Length];

            for (int i = 0; i < indices.Length; i += 3)
            {
               // Console.WriteLine(indices[i] + " " + indices[i + 1] + " " + indices[i + 2]);
                Vector3 v0 = ToVector3(vertices[indices[i + 0]]);
                Vector3 v1 = ToVector3(vertices[indices[i + 1]]);
                Vector3 v2 = ToVector3(vertices[indices[i + 2]]);

                Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

                normals[indices[i]] = normals[indices[i + 1]] = normals[indices[i + 2]] = normal ;
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = Vector3.Normalize(normals[i]);
                //Console.WriteLine(normals[i]);
                vertexnormals[i] = new Vector3(0, 0, 0);
            }
            for (int i = 0; i < indices.Length; i += 3)
            {
                vertexnormals[indices[i]] = Vector3.Normalize(vertexnormals[indices[i]] + normals[indices[i]]);
                vertexnormals[indices[i + 1]] = Vector3.Normalize(vertexnormals[indices[i + 1]] + normals[indices[i + 1]]);
                vertexnormals[indices[i + 2]] = Vector3.Normalize(vertexnormals[indices[i + 2]] + normals[indices[i + 2]]);
                //Console.WriteLine("i: {0} i+1: {1} i+2: {2}", vertexnormals[indices[i]], vertexnormals[indices[i+1]], vertexnormals[indices[i+2]]);
           /*
                Vector3 n = normals[indices[i]];

                if (!perPoint.ContainsKey(normals[indices[i]]))
                    perPoint.Add(normals[indices[i]], n);
                else
                    perPoint[normals[indices[i]]] = perPoint[normals[indices[i]]] + n;
                if (!perPoint.ContainsKey(normals[indices[i + 1]]))
                    perPoint.Add(normals[indices[i + 1]], n);
                else
                    perPoint[normals[indices[i + 1]]] = perPoint[normals[indices[i + 1]]] + n;
                if (!perPoint.ContainsKey(normals[indices[i + 2]]))
                    perPoint.Add(normals[indices[i + 2]], n);
                else
                    perPoint[normals[indices[i + 2]]] = perPoint[normals[indices[i + 2]]] + n;
              */ 
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexnormals[i] = Vector3.Normalize(vertexnormals[i]);
            }
            #endregion
            #region *******box around the object*******

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].X > xmax)
                    xmax = vertices[i].X;
                if (vertices[i].Y > ymax)
                    ymax = vertices[i].Y;
                if (vertices[i].Z > zmax)
                    zmax = vertices[i].Z;
            }
            
            xmin = ymin = zmin = 1000;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].X < xmin)
                    xmin = vertices[i].X;
                if (vertices[i].Y < ymin)
                    ymin = vertices[i].Y;
                if (vertices[i].Z < zmin)
                    zmin = vertices[i].Z;
            }   

                Console.WriteLine("x: {0} y: {1} z: {2}", xmax, ymax, zmax);
                Console.WriteLine("x: {0} y: {1} z: {2}", xmin, ymin, zmin);

            boxvertices = new Vector3d[]
            {
                new Vector3d(xmin - 2, ymin - 2, zmax + 2),
                new Vector3d(xmax + 2, ymin - 2, zmax + 2),//1
                new Vector3d(xmax + 2, ymax + 2, zmax + 2),
                new Vector3d(xmin - 2, ymax + 2, zmax + 2),
                new Vector3d(xmax + 2, ymin - 2, zmin - 2),
                new Vector3d(xmax + 2, ymax + 2, zmin - 2), 
                new Vector3d(xmin - 2, ymax + 2, zmin - 2),//6
                new Vector3d(xmin - 2, ymin - 2, zmin - 2)
            };
            
            boxindices = new uint[]
            {
                0, 1, 1, 2,
                2, 3, 3, 0,
                4, 5, 5, 6,
                6, 7, 0, 7,
                3, 6, 2, 5, 
                1, 4, 4, 7
            };
            #endregion
            cent = ToVector3(Vector3d.Subtract(boxvertices[1], boxvertices[6]));
            cent /= 2;

            GL.ClearColor(Color.Black);
            GL.PointSize(5f);

            #region GenBuffers, BindBuffers, BufferData
            //----------------Vertex Array Buffer---------------------
            {
                GL.GenBuffers(1, out v_position);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vector3d.SizeInBytes), vertices, BufferUsageHint.DynamicDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (vertices.Length * Vector3d.SizeInBytes != bufferSize)
                    Console.WriteLine("Vertex array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            
            //----------------Normal Array Buffer--------------------- 
            if (normals != null)
            {
                GL.GenBuffers(1, out norm);
                GL.BindBuffer(BufferTarget.ArrayBuffer, norm);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(normals.Length * Vector3.SizeInBytes), normals, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (normals.Length * Vector3.SizeInBytes != bufferSize)
                    Console.WriteLine("Normal array not uploaded correctly!");

                //    //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            //---------------Element Array Buffer---------------------
            {
                GL.GenBuffers(1, out i_elements);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.DynamicDraw);
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (indices.Length * sizeof(uint) != bufferSize)
                    Console.WriteLine("Element array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }
            //------------------Box Array Buffer------------
            {
                GL.GenBuffers(1, out box_id);
                GL.BindBuffer(BufferTarget.ArrayBuffer, box_id);
                GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, (IntPtr)(boxvertices.Length * Vector3d.SizeInBytes), boxvertices, BufferUsageHint.DynamicDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (boxvertices.Length * Vector3d.SizeInBytes != bufferSize)
                    Console.WriteLine("Box Vertex array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            //------------------Box Element Buffer----------------
            {
                GL.GenBuffers(1, out boxid);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, boxid);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(boxindices.Length * sizeof(uint)), boxindices, BufferUsageHint.DynamicDraw);
                GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (boxindices.Length * sizeof(uint) != bufferSize)
                    Console.WriteLine("Box Element array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }
            elementCount = indices.Length;
            #endregion
            PosCam();
           // axe.Prepare(cam);
        }
        void PosCam()
        {
            var dx = cent.X;
            var dy = cent.Y;
            var dz = cent.Z;
            Console.WriteLine(dx + " " + dy + " " + dz);
            cam.SetPosition(new Vector3(dx, dy*-1, dz), 100f);
            cam.PokeCamera();
            
        }
        private void drawAxes()
        {
            GL.LineWidth(3f);
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
            }
        private void draw_lines()
        {
            for(int i = 0; i < indices.Length; i += 3)
            {
                var v0 = vertices[indices[i+0]];
                var v1 = vertices[indices[i + 1]];
                var v2 = vertices[indices[i + 2]];
                GL.LineWidth(1.5f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(Color.White);
                GL.Vertex3(v0.X, v0.Y, v0.Z);
                GL.Vertex3(v1.X, v1.Y, v1.Z);
                GL.Vertex3(v2.X, v2.Y, v2.Z);
                GL.End();
            }
        }
        private void draw()
        {
            
            //-------------vertex array buffer-----------------  
            {
                GL.Color3(Color.Red);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                GL.VertexPointer(3, VertexPointerType.Double, 0, 0);
                GL.PointSize(5f);
                //GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length);
            }

            //------------Element Array Buffer-----------------
            
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                //GL.Color3(Color.White);
                GL.LineWidth(0.5f);
                //GL.DrawElements(PrimitiveType.Lines, elementCount, DrawElementsType.UnsignedInt, 0);
                GL.Color3(Color.Gray);
                GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, 0);
            }

            //------------Normal Array Buffer------------------
            
            {
                GL.EnableClientState(ArrayCap.NormalArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, norm);
                GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, 0);
            }

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }
        private void drawBox()
        {
            GL.LineWidth(1f);
            GL.Color3(Color.Cyan);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, box_id);
            GL.VertexPointer(3, VertexPointerType.Double, 0, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, boxid);
            GL.DrawElements(PrimitiveType.Lines, boxindices.Length, DrawElementsType.UnsignedInt, 0);
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref cameramatrix);

            GL.Disable(EnableCap.Lighting);

            drawAxes();

            //draw_lines();
            //drawBox();
            //axe.Render();

            if (checkBox1.Checked == true) 
                drawBox();

            draw();

            glControl1.SwapBuffers();
        }
        private void glControl1_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("Resize time: " + DateTime.Now);
           
            GL.Viewport(glControl1.ClientRectangle.X, glControl1.ClientRectangle.Y, glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);
            znear = 0.1f;
            zfar = 256000;
            projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, glControl1.ClientRectangle.Width / (float)glControl1.ClientRectangle.Height, znear, zfar);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            glControl1.Invalidate();

        }
        #region temp 2
        //internal void gluPerspective(double fovy, double aspect, double zNear, double zFar)
        //{
        //    /*
        //    double xmin, xmax, ymin, ymax;

        //    ymax = zNear * Math.Tan(fovy * Math.PI / 360.0);
        //    ymin = -ymax;

        //    xmin = ymin * aspect;
        //    xmax = ymax * aspect;

        //    GL.Frustum(xmin, xmax, ymin, ymax, zNear, zFar);//left, right, bottom, top
        //     */
        //    Matrix4d m = Matrix4d.Identity;
        //    double sine, cotangent, deltaZ;
        //    double radians = fovy / 2 * Math.PI / 180;

        //    deltaZ = zFar - zNear;
        //    sine = Math.Sin(radians);
        //    if ((deltaZ == 0) || (sine == 0) || (aspect == 0))
        //    {
        //        return;
        //    }
        //    //TODO: check why this cos was written COS?
        //    cotangent = Math.Cos(radians) / sine;

        //    m.M11 = cotangent / aspect;
        //    m.M22 = cotangent;
        //    m.M33 = -(zFar + zNear) / deltaZ;
        //    m.M34 = -1;
        //    m.M43 = -2 * zNear * zFar / deltaZ;
        //    m.M44 = 0;

        //    GL.MultMatrix(ref m);
        //}
        //Vector3 forward = new Vector3();
        //Vector3 up = new Vector3();
        //Vector3 right = new Vector3();
        //void gluLookAt(float eyex, float eyey, float eyez, float centerx, float centery, float centerz, float upx, float upy, float upz)
        //{
        //    //float[] m = new float[16];
        //    Matrix4 m;
        //    forward.X = centerx - eyex;
        //    forward.Y = centery - eyey;
        //    forward.Z = centerz - eyez;
        //    up.X = upx;
        //    up.Y = upy;
        //    up.Z = upz;

        //    forward.Normalize();

        //    /* Side = tForward x tUp */
        //    Vector3.Cross(ref forward, ref up, out right);

        //    right.Normalize();

        //    /* Recompute tUp as: tUp = tRight x tForward */
        //    Vector3.Cross(ref right, ref forward, out up);
        //    /*
        //    // set right vector
        //    m[0] = right.X; m[1] = up.X; m[2] = -forward.X; m[3] = 0;
        //    // set up vector
        //    m[4] = right.Y; m[5] = up.Y; m[6] = -forward.Y; m[7] = 0;
        //    // set forward vector
        //    m[8] = right.Z; m[9] = up.Z; m[10] = -forward.Z; m[11] = 0;
        //    // set translation vector
        //    m[12] = 0; m[13] = 0; m[14] = 0; m[15] = 1;
        //    *///sdsd
        //    m = Matrix4.Identity;
        //    m.M11 = right.X;
        //    m.M21 = right.Y;
        //    m.M31 = right.Z;

        //    m.M21 = up.X;
        //    m.M22 = up.Y;
        //    m.M23 = up.Z;

        //    m.M31 = -forward.X;
        //    m.M32 = -forward.Y;
        //    m.M33 = -forward.Z;
              
        //    GL.MultMatrix(ref m);
        //    GL.Translate(-eyex, -eyey, -eyez);
        //}
        //void ResetCamera()
        //{
            
        //    g_targetPos[0] = 0;
        //    g_targetPos[1] = 10;
        //    g_targetPos[2] = 0;

        //    g_cameraPos[0] = g_targetPos[0];
        //    g_cameraPos[1] = g_targetPos[1];
        //    g_cameraPos[2] = g_targetPos[2] + 150 + CAMERA_ZNEAR;

        //    g_pitch = 0.0f;
        //    g_heading = 0.0f;
        //}
        //enum ECameraMode
        //{
        //    CAMERA_NONE, CAMERA_TRACK, CAMERA_DOLLY, CAMERA_ORBIT
        //}
        #endregion
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("keydown");
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();
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
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                cam.MouseDown(new System.Drawing.Point(e.X, e.Y), OpenTK.Input.MouseButton.Left);
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                cam.MouseDown(new System.Drawing.Point(e.X, e.Y), OpenTK.Input.MouseButton.Middle);
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                cam.MouseDown(new System.Drawing.Point(e.X, e.Y), OpenTK.Input.MouseButton.Right);
            
            isMouseDown = true;
           
        }
        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            cam.MouseUp();
           
            isMouseDown = false;
              
        }
             
        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            cam.MouseMove(new System.Drawing.Point(e.X, e.Y));
            
            if (isMouseDown)
            {
                glControl1.Refresh();
            }
            

        }
        private void glControl1_MouseWheelChanged(Object sender, MouseEventArgs e)
        {
            
            cam.Mouse_WheelChanged(null, new OpenTK.Input.MouseWheelEventArgs(e.X, e.Y, 1, e.Delta / 100));
            glControl1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            draw_lines();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
            //GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * Vector3d.SizeInBytes), vertices, BufferUsageHint.StaticDraw);

            var x = 1;
            Vector3d[] vert = vertices;


            //Console.WriteLine(trackBar1.Value.ToString());
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbox.Checked == true) 
            {
                draw_lines();
                Console.WriteLine("true");
            }
        }
        
        }              
    }
