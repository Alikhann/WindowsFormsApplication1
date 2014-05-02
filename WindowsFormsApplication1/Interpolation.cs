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

        //float x1, x2, x3, y1, y2, y3, w1, w2, w3;
        float x, y, w;
        float alpha, beta, gamma;
        float alpha1, beta1, gamma1;
        float s, s1, s2, s3;
        float delta, delta1, delta2;
        double[,] a1, a2, a3;
        double[,] a = new double[3,3];

        public float findArea(float ax, float ay, float bx, float by, float cx, float cy)
        {
            return Math.Abs((ax*(by-cy)+bx*(cy-ay)+cx*(ay-by))/2);
        }

        public double DET(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            int n = 3;
            a[0, 0] = 1; a[0, 1] = 1; a[0, 2] = 1;//static
            a[1, 0] = x1; a[1, 1] = x2; a[1, 2] = x3;
            a[2, 0] = y1; a[2, 1] = y2; a[2, 2] = y3;

            int i, j, k;
            
            double det = 0;
            for (i = 0; i < n - 1; i++)
            {
                for (j = i + 1; j < n; j++)
                {
                    det = a[j, i] / a[i, i];
                    for (k = i; k < n; k++)
                        a[j, k] = (a[j, k] - det * a[i, k]); 
                }
            }
            det = 1;
            for (i = 0; i < n; i++)
                det = det * a[i, i];
            return det;
        }
        
        public float interpolate(float x, float y, float x1, float y1, float x2, float y2, float x3, float y3, float z1, float z2, float z3)
        {
            //fucking shit..
            s = findArea(x1, y1, x2, y2, x3, y3);
            s1 = findArea(x, y, x2, y2, x3, y3);
            s2 = findArea(x1, y1, x, y, x3, y3);
            s3 = findArea(x1, y1, x2, y2, x, y);

            alpha = s1 / s;
            beta = s2 / s;
            gamma = s3 / s;
            delta = (float)DET(x1, y1, x2, y2, x3, y3);
            delta1 = (float)DET(x, y, x2, y2, x3, y3);
            delta2 = (float)DET(x1, y1, x, y, x3, y3);

            if ((alpha + beta + gamma) == 1)
            {
                alpha1 = delta1 / delta;
                beta1 = delta2 / delta;
                gamma1 = 1 - alpha1 - beta1;
                return alpha1 * z1 + beta1 * z2 + gamma1 * z3;
            }
            else return 1;
        }
        
    }
}
