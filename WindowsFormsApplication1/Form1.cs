﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Globalization;
using System.Linq;
using System.Drawing.Drawing2D;

namespace WindowsFormsApplication1
{
    public partial class AliGL : Form
    {
        #region ------declaring------
        bool loaded = false;
        bool isMouseDown;

        Vector3d[] vertices, vertices2, negativeVertices, negativeVertices2;
        uint[] indices;
        Vector3[] normals;
        Vector3[] vertexnormals;
        Dictionary<Vector3, Vector3> perPoint = new Dictionary<Vector3, Vector3>();
        double[] distances;
 
        //---------------for reading file---------------
        
        List<uint> list = new List<uint>();
        int number1, number2;
        string[] values;
        List<Vector3d> listOfVertices = new List<Vector3d>();
        string fileName = "G:/test/input.txt";
        //-------------------end-----------------------

        int elementCount = 0; //count of tri element to be drawn
        Camera cam;
        Matrix4 cameramatrix;
        
        public static float zfar, znear;
        public static Matrix4 projection, defaultModelview, defaultNormal, defaultMVP;
        int v_position, v_position2, i_elements, norm, bufferSize;

        double xmin, xmax, ymin, ymax, zmin, zmax;
        Vector3 cent;

        //--------------------------------lighting 

        float[] light_position;
       
        #endregion

        Bitmap b = new Bitmap(180, 30);
        int deltaR, deltaG, deltaB;

        public AliGL()
        {
            InitializeComponent();

            cam = new Camera(glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);
            this.cameramatrix = cam.cameraMatrix;
            cam.camch += cam_camch;
        }
        private void cam_camch(Matrix4 cameramatrix)
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

            
            GL.ClearColor(Color.Black);
            GL.PointSize(5f);

            //Alikhan Nugmanov


            float[] light_ambient = { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] light_diffuse = { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] light_specular = { 1.0f, 1.0f, 1.0f, 1.0f };

            light_position = new float[]{ 0.0f, 0.0f, 0.0f, 1.0f };

            float[] spotdirection = { 0.0f, 0.0f, -1.0f };

            //GL.Enable(EnableCap.Lighting);
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, light_specular);
            //GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            

             GL.Light(LightName.Light0, LightParameter.Position, light_position);
            GL.Light(LightName.Light0, LightParameter.ConstantAttenuation, 1.3f);
            GL.Light(LightName.Light0, LightParameter.SpotCutoff, 45.0f);
            GL.Light(LightName.Light0, LightParameter.SpotDirection, spotdirection);
            GL.Light(LightName.Light0, LightParameter.SpotExponent, 1.0f);

            //GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, light_ambient);
            GL.LightModel(LightModelParameter.LightModelLocalViewer, 1.0f);
            GL.LightModel(LightModelParameter.LightModelTwoSide, 1.0f);
            GL.Enable(EnableCap.Light0);
            
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial);
            GL.ShadeModel(ShadingModel.Flat);
            //GL.LightModel(LightModelParameter.LightModelLocalViewer, );

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
            negativeVertices = new Vector3d[vertices.Length];
            Array.Copy(vertices, negativeVertices, vertices.Length);
            for (int i = 0; i < vertices.Length; i ++)
            {
                negativeVertices[i].Z *= -1;
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

                //Console.WriteLine("x: {0} y: {1} z: {2}", xmax, ymax, zmax);
                //Console.WriteLine("x: {0} y: {1} z: {2}", xmin, ymin, zmin);
            /*
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
            };*/
            #endregion
            //cent = ToVector3(Vector3d.Subtract(boxvertices[1], boxvertices[6]));
            //cent /= 2;
            #region GenBuffers, BindBuffers, BufferData
            //----------------Vertex Array Buffer---------------------
            {
                GL.GenBuffers(1, out v_position);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * Vector3d.SizeInBytes), vertices, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (vertices.Length * Vector3d.SizeInBytes != bufferSize)
                    Console.WriteLine("Vertex array is not uploaded correctly!");

                //clear the buffer Binding
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            }
            //----------------Negative Vertex Array Buffer---------------------
            {
                GL.GenBuffers(1, out v_position2);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position2);
                GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, (IntPtr)(negativeVertices.Length * Vector3d.SizeInBytes), negativeVertices, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out bufferSize);
                if (negativeVertices.Length * Vector3d.SizeInBytes != bufferSize)
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
            elementCount = indices.Length;
            #endregion
            PosCam();

            vertices2 = new Vector3d[vertices.Length];
            Array.Copy(vertices, vertices2, vertices.Length);
            negativeVertices2 = new Vector3d[negativeVertices.Length];
            Array.Copy(negativeVertices, negativeVertices2, negativeVertices.Length);
            distances = new double[vertices.Length];
            checkBox2.Checked = true;

            fillColor(Color.Blue, Color.Green, 1, 60);
            fillColor(Color.Green, Color.Yellow, 60, 120);
            fillColor(Color.Yellow, Color.Red, 120, 180);

            Interpolation iii = new Interpolation();
            
           // axe.Prepare(cam);
        }
        // c1 c2  ; c1 - r,g,b   c2   - r,g,b
        // for.. (c1.r-c2.r)/n-1
            // r_new, g_new, b_new
            // Color c = Color.FrmArgb(r_new, g_new, b_new);
            //bm.setPixel(i, 2, c);
         //
        private void fillColor(Color c1, Color c2, int start, int end) 
        {
            
            int newR=0, newG=0, newB=0;
            int curR = c1.R;
            int curG = c1.G;
            int curB = c1.B;

            deltaR = Convert.ToInt32((c2.R - c1.R) / (end - start - 1));
            deltaG = Convert.ToInt32((c2.G - c1.G) / (end - start - 1));
            deltaB = Convert.ToInt32((c2.B - c1.B) / (end - start - 1));
            //Console.WriteLine("RED: {0} GREEN: {1} Blue: {2}", deltaR, deltaG, deltaB);
            
            for(int i = start; i < end; i++)
            {
                newR = curR + deltaR;
                newG = curG + deltaG;
                newB = curB + deltaB;
                //Console.WriteLine("RED: {0} GREEN: {1} Blue: {2}", newR, newG, newB);
                Color color = Color.FromArgb(newR, newG, newB);
                for (int j = 0; j < 30; j++)
                    b.SetPixel(i, j, color);
                curR = newR; curG = newG; curB = newB;
            }
            pictureBox4.Image = b;
        }
       
        private void PosCam()
        {
            cent = ToVector3(Vector3d.Subtract(new Vector3d(xmax + 2, ymin - 2, zmax + 2), new Vector3d(xmin - 2, ymax + 2, zmin - 2)));
            cent /= 2;
            var dx = cent.X;
            var dy = cent.Y;
            var dz = cent.Z;
            Console.WriteLine(dx + " " + dy + " " + dz);
            cam.SetPosition(new Vector3(dx, dy*-1, dz), 100f);
            cam.PokeCamera();
            
        }
        private void findDistance()
        {
            for(int i = 0; i < vertices2.Length; i++)
            {
                distances[i] = Math.Sqrt(Math.Pow(vertices2[i].X, 2) + Math.Pow(vertices2[i].Y, 2) + Math.Pow(vertices2[i].Z, 2))*0;
            }
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
                var v0 = vertices2[indices[i+0]];
                var v1 = vertices2[indices[i + 1]];
                var v2 = vertices2[indices[i + 2]];
                var vv0 = negativeVertices2[indices[i + 0]];
                var vv1 = negativeVertices2[indices[i + 1]];
                var vv2 = negativeVertices2[indices[i + 2]];
                GL.LineWidth(1.5f);
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(Color.DarkGray);
                GL.Vertex3(v0.X, v0.Y, v0.Z);
                GL.Vertex3(v1.X, v1.Y, v1.Z);
                GL.Vertex3(v2.X, v2.Y, v2.Z);
                GL.End();
                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex3(vv0.X, vv0.Y, vv0.Z);
                GL.Vertex3(vv1.X, vv1.Y, vv1.Z);
                GL.Vertex3(vv2.X, vv2.Y, vv2.Z);
                GL.End();
            }
        }
        private void draw()
        {
            GL.Enable(EnableCap.Lighting);
            //-------------vertex array buffer-----------------  
            {
                GL.Color3(Color.Red);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
                GL.VertexPointer(3, VertexPointerType.Double, 0, 0);
                //GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length);
            }

            //------------Element Array Buffer-----------------
            
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
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

            {
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.BindBuffer(BufferTarget.ArrayBuffer, v_position2);
                GL.VertexPointer(3, VertexPointerType.Double, 0, 0);
                GL.PointSize(5f);
                //GL.DrawArrays(PrimitiveType.Points, 0, vertices.Length);
            }

            //------------Element Array Buffer-----------------

            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, i_elements);
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
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
            xmax = ymax = zmax = 0;
            for (int i = 0; i < vertices2.Length; i++)
            {
                if (vertices2[i].X > xmax)
                    xmax = vertices2[i].X;
                if (vertices2[i].Y > ymax)
                    ymax = vertices2[i].Y;
                if (vertices2[i].Z > zmax)
                    zmax = vertices2[i].Z;
            }

            xmin = ymin = zmin = 100000;

            for (int i = 0; i < vertices2.Length; i++)
            {
                if (vertices2[i].X < xmin)
                    xmin = vertices2[i].X;
                if (vertices2[i].Y < ymin)
                    ymin = vertices2[i].Y;
                if (negativeVertices2[i].Z < zmin)
                    zmin = negativeVertices2[i].Z;
            }
            GL.Color3(Color.Cyan);
            GL.LineWidth(1f);
            #region draw using GL.BEGIN
            GL.Begin(PrimitiveType.Lines);
            
                GL.Vertex3(xmin - 2, ymin - 2, zmax + 2);
                GL.Vertex3(xmax + 2, ymin - 2, zmax + 2);//0-1
                GL.Vertex3(xmax + 2, ymin - 2, zmax + 2);
                GL.Vertex3(xmax + 2, ymax + 2, zmax + 2);//1-2
                GL.Vertex3(xmax + 2, ymax + 2, zmax + 2);
                GL.Vertex3(xmin - 2, ymax + 2, zmax + 2);//2-3
                GL.Vertex3(xmin - 2, ymax + 2, zmax + 2);
                GL.Vertex3(xmin - 2, ymin - 2, zmax + 2);//3-0
                GL.Vertex3(xmax + 2, ymin - 2, zmin - 2);
                GL.Vertex3(xmax + 2, ymax + 2, zmin - 2);//4-5
                GL.Vertex3(xmax + 2, ymax + 2, zmin - 2);
                GL.Vertex3(xmin - 2, ymax + 2, zmin - 2);//5-6
                GL.Vertex3(xmin - 2, ymax + 2, zmin - 2);
                GL.Vertex3(xmin - 2, ymin - 2, zmin - 2);//6-7
                GL.Vertex3(xmin - 2, ymin - 2, zmax + 2);
                GL.Vertex3(xmin - 2, ymin - 2, zmin - 2);//0-7
                GL.Vertex3(xmin - 2, ymax + 2, zmax + 2);
                GL.Vertex3(xmin - 2, ymax + 2, zmin - 2);//3-6
                GL.Vertex3(xmax + 2, ymax + 2, zmax + 2);
                GL.Vertex3(xmax + 2, ymax + 2, zmin - 2);//2-5
                GL.Vertex3(xmax + 2, ymin - 2, zmax + 2);
                GL.Vertex3(xmax + 2, ymin - 2, zmin - 2);//1-4
                GL.Vertex3(xmax + 2, ymin - 2, zmin - 2);
                GL.Vertex3(xmin - 2, ymin - 2, zmin - 2);//4-7
                GL.End();
            #endregion
        }
        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            if (!loaded)
                return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref cameramatrix);
            //draw_lines();
            //drawBox();
            //axe.Render();
            GL.Disable(EnableCap.Lighting);
            drawAxes();
            if (checkbox.Checked == true)
                draw_lines();
            if (checkBox1.Checked == true) 
                drawBox();
            if(checkBox2.Checked == true)
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
            
            //Console.WriteLine(cam.Position);
            
            //light_position[3] = 1.0f; // positional light
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
        
        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            glControl1_Paint(null, null);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            glControl1_Paint(null, null);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Array.Copy(vertices, vertices2, vertices.Length);
            Array.Copy(negativeVertices, negativeVertices2, negativeVertices.Length);
            findDistance();
            for(int i = 0; i < vertices2.Length; i++)
            {
                vertices2[i].Z *= trackBar1.Value;
                negativeVertices2[i].Z *= trackBar1.Value;
            }
            //Console.WriteLine(negativeVertices2[1].Z);
            for(int i =0; i < vertices2.Length; i++)
            {
                negativeVertices2[i].Z += distances[i];
            }
           // Console.WriteLine(negativeVertices2[1].Z);

            GL.BindBuffer(BufferTarget.ArrayBuffer, v_position);
            GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, new IntPtr(vertices2.Length * Vector3d.SizeInBytes), vertices2, BufferUsageHint.DynamicDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, v_position2);
            GL.BufferData<Vector3d>(BufferTarget.ArrayBuffer, new IntPtr(negativeVertices2.Length * Vector3d.SizeInBytes), negativeVertices2, BufferUsageHint.DynamicDraw);

            glControl1_Paint(null, null);

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            glControl1_Paint(null, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap b = new Bitmap(pictureBox4.Image);
            Color color = b.GetPixel(Convert.ToInt32(textBox2.Text), 1);

            pictureBox3.BackColor = color;
        }


    }              
    }
