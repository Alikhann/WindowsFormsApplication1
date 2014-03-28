using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Threading;

namespace WindowsFormsApplication1
{
    public class Camera
    {
        public static Vector3 lastPos;
        Vector4 unRotatedRight;
        Vector4 unRotatedUp;
        public static Vector4 rotatedRight = Vector4.UnitX;
        public static Vector3 cameraLookDirection;

        public Matrix4 cameraMatrix;
        private Vector3 eye;
        private Vector3 dir;
        private Vector3 up = Vector3.UnitY;
        private Vector3 start;
        public event CameraChanged camch;
        bool moving;
        bool translating;
        float radius = 10;
        PointF down;

        public static float InitialAngleDec = 90;

        public float alpha = MathHelper.DegreesToRadians(InitialAngleDec);
        public float alphay = MathHelper.DegreesToRadians(InitialAngleDec);

        int width
        {
            get;
            set;
        }
        int height
        {
            get;
            set;
        }

        public Vector3 Position
        {
            get { return dir + eye; }
        }

        public Vector3 LookDirection
        {
            get { return dir; }
        }

        public Vector3 Up
        {
            get { return up; }
        }

        protected float getXnormalized(float x, int width)
        {
            return ((2.0f * x) / width) - 1.0f;
        }

        protected float getYnormalized(float y, int height)
        {
            return 1.0f - ((2.0f * y) / height);
        }

        protected float projectTBToSphere(float r, float x, float y)
        {
            try
            {
                float z = 0;
                float d = (float)Math.Sqrt((float)(x * x + y * y));
                if (d < r * 0.70710678118654752440d)
                {
                    //Inside sphere
                    z = (float)Math.Sqrt((float)(r * r - d * d));
                }
                else
                {
                    //On hyperbola
                    float t = (float)(r / 1.41421356237309504880d);
                    z = t * t / d;
                }
                return z;
            }
            catch (Exception) { return 0; }
        }

        public Camera(Vector3 camdir, Vector3 camup, Vector3 cameye, int width, int height)
        {
            dir = camdir;
            eye = cameye;
            up = camup;
            cameraMatrix = Matrix4.LookAt(eye, dir, up);
            lastPos = eye;
            this.width = width;
            this.height = height;

            unRotatedRight = Vector4.Transform(Vector4.UnitX, Matrix4.CreateRotationY((-alpha)));
            unRotatedUp = Vector4.Transform(Vector4.UnitY, Matrix4.CreateRotationZ((-alphay)));
        }

        public Camera(int width, int height)
        {
            dir = new Vector3(0, 0, -1);
            eye = new Vector3(0f, 0, 15f);
            cameraMatrix = Matrix4.LookAt(eye, dir, up);
            lastPos = eye;
            this.width = width;
            this.height = height;

            unRotatedRight = Vector4.Transform(Vector4.UnitX, Matrix4.CreateRotationY((-alpha)));
            unRotatedUp = Vector4.UnitY;
        }

        float savedalphay;
        public void MouseMove(Point e)
        {
            if (moving)
            {
                var x = -getXnormalized(e.X, width);
                var y = -getYnormalized(e.Y, height);
                var z = projectTBToSphere(radius, x, y);

                var v = new Vector3((float)x, (float)y, (float)z);
                var v1 = new Vector3((float)x, 0, (float)z);

                var startnoy = new Vector3(start.X, 0, start.Z);
                var movement = (v1 - startnoy);
                var axe = Vector3.Cross(v1, startnoy);
                var ml = movement.Length / 10;

                if (axe.Y < 0)
                    ml = -ml;
                alpha += ml * 15;

                var v2 = new Vector3((float)0, y, (float)z);
                var startnox = new Vector3(0, start.Y, start.Z);
                movement = (v2 - startnox);
                axe = Vector3.Cross(v2, startnox);

                ml = movement.Length / 10;
                if (axe.X > 0)
                    ml = -ml;
                alphay += ml * 15;

                float msay = (float)Math.Sin(alphay);
                bool sign = false;
                if (msay < 0.01f)
                {
                    sign = true;
                }
                if (sign)
                    alphay = savedalphay;

                msay = (float)Math.Sin(alphay);

                savedalphay = alphay;

                float cx = (float)Math.Cos(alpha) * msay;
                float cz = (float)Math.Sin(alpha) * msay;
                float cy = (float)Math.Cos(alphay);

                eye.X = cx * radius;
                eye.Z = cz * radius;
                eye.Y = cy * radius;

                rotatedRight = Vector4.Transform(unRotatedRight, Matrix4.CreateRotationY((-alpha)));

                cameraMatrix = Matrix4.LookAt(eye + dir, dir, up);
                cameraLookDirection = dir;
                lastPos = eye + dir;

                start = v;
                camch(cameraMatrix);
            }

            if (translating)
            {
                Vector2 last = new Vector2(down.X, down.Y);
                Vector2 v = new Vector2(e.X, e.Y);
                Vector2 tr = v - last;
                tr = tr / 50;

                Vector3 trans = new Vector3(0, 0, tr.X);
                trans = Vector3.Transform(trans, Matrix4.CreateRotationY((float)(-alpha)));
                trans = trans * (radius / 15);
                dir = dir + trans;

                trans = new Vector3(0, tr.Y, 0);
                float a2 = -(alphay - (float)Math.PI / 2f);

                trans = Vector3.Transform(trans, Matrix4.CreateRotationZ((float)(a2)));
                trans = Vector3.Transform(trans, Matrix4.CreateRotationY((float)(-alpha)));
                trans = trans * (radius / 15);
                dir = dir + trans;

                cameraMatrix = Matrix4.LookAt(eye + dir, dir, up);
                cameraLookDirection = dir;
                lastPos = eye + dir;
                down.X = v.X;
                down.Y = v.Y;
                camch(cameraMatrix);
            }
        }

        public void SetPosition(Vector3 dir, float zoom)
        {
            this.dir = dir;
            cameraMatrix = Matrix4.LookAt(eye + dir, dir, up);
            lastPos = eye + dir;
            camch(cameraMatrix);
            radius = zoom;
        }

        public void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            MouseMove(new Point(e.X, e.Y));
        }

        public void MouseUp()
        {
            moving = false;
            translating = false;
        }

        public void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp();
        }

        public void MouseDown(Point e, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                down = e;
                var x = -getXnormalized(e.X, width);
                var y = -getYnormalized(e.Y, height);
                var z = projectTBToSphere(radius, x, y);
                start = new Vector3((float)x, (float)y, (float)z);
                moving = true;
            }
            if (button == MouseButton.Middle)
            {
                down = e;
                translating = true;
            }
        }

        public void PokeCamera()
        {
            var p1 = new Point(0, 0);

            MouseDown(p1, MouseButton.Left);
            p1.X = 1;
            MouseMove(p1);
            MouseUp();
        }

        public void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown(new Point(e.X, e.Y), e.Button);
        }

        public void Mouse_WheelChanged(object sender, MouseWheelEventArgs e)
        {
            radius = radius * (float)(Math.Pow(1.1, -e.Delta));
            down = e.Position;
            var x = -getXnormalized(e.X, width);
            var y = -getYnormalized(e.Y, height);
            var z = projectTBToSphere(radius, x, y);
            start = new Vector3((float)x, (float)y, (float)z);
            moving = true;
            Mouse_Move(null, new MouseMoveEventArgs(e.X, e.Y, 0, 0));
            moving = false;
        }

        public static void PrintMatrix(Matrix4 cameraMatrix)
        {
            Console.WriteLine("-------");
            Console.WriteLine(cameraMatrix.Column0.X.ToString("000.0000") + "\t" + cameraMatrix.Column1.X.ToString("000.0000") + "\t" + cameraMatrix.Column2.X.ToString("000.0000") + "\t" + cameraMatrix.Column3.X.ToString("000.0000") + "\t");
            Console.WriteLine(cameraMatrix.Column0.Y.ToString("000.0000") + "\t" + cameraMatrix.Column1.Y.ToString("000.0000") + "\t" + cameraMatrix.Column2.Y.ToString("000.0000") + "\t" + cameraMatrix.Column3.Y.ToString("000.0000") + "\t");
            Console.WriteLine(cameraMatrix.Column0.Z.ToString("000.0000") + "\t" + cameraMatrix.Column1.Z.ToString("000.0000") + "\t" + cameraMatrix.Column2.Z.ToString("000.0000") + "\t" + cameraMatrix.Column3.Z.ToString("000.0000") + "\t");
            Console.WriteLine(cameraMatrix.Column0.W.ToString("000.0000") + "\t" + cameraMatrix.Column1.W.ToString("000.0000") + "\t" + cameraMatrix.Column2.W.ToString("000.0000") + "\t" + cameraMatrix.Column3.W.ToString("000.0000") + "\t");
        }

    }

    public delegate void CameraChanged(Matrix4 cameramatrix);
}
