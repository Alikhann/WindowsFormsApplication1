using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    class Interpolation
    {
        public Interpolation() { }

        float x1, x2, x3, y1, y2, y3, w1, w2, w3;
        float x, y, w;
        float alpha, beta, gamma;
        float s, s1, s2, s3;
        float delta, delta1, delta2;

        public double findArea(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return Math.Abs((ax*(by-cy)+bx*(cy-ay)+cx*(ay-by))/2);
        }

        public void interpolate()
        {

        }
    }
}
