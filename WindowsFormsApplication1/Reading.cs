using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using System.Globalization;

namespace WindowsFormsApplication1
{
    class Reading
    {
        public Reading() { }
        StreamReader read = new StreamReader(@"C:/input.txt");
        
        String line;
        String[] str;
        int num1;
        int num2;
        int cnt = 0;
        string fileName = "C:/input.txt";
        List<Vector3d> listOfVertices = new List<Vector3d>();
        Vector3d[] vertices; 

        public void readFile()
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            num1 = Convert.ToInt32(File.ReadAllLines(@fileName).First());
            
            using(StreamReader rd = new StreamReader(@fileName))
            {
                for(int i = 1; i < num1 + 2; i ++)
                {
                    line = rd.ReadLine();
                    str = line.Split(' ');
                    //var x = float.Parse(str[0], NumberStyles.Any, ci);
                    //var y = float.Parse(str[1], NumberStyles.Any, ci);
                    //var z = float.Parse(str[2], NumberStyles.Any, ci);

                    //listOfVertices.Add(new Vector3d(x, y, z));
                }
            }
            vertices = listOfVertices.ToArray();
            foreach (var vert in vertices)
                Console.WriteLine(vert);

                //num1 = Convert.ToInt32(read.ReadLine());/*
                /* for (int i = 1; i < num1 + 2; i++)
                {
                    str = line.Split(' ');
                    Console.WriteLine(str);
                }*/

                Console.WriteLine(num1);
        }
        
    }

}
