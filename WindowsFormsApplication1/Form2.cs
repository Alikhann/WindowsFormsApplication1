using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form2 : Form
    {
        Bitmap b = new Bitmap(570, 450);


        public Form2()
        {
            InitializeComponent();
        }
        Interpolation interp = new Interpolation();
        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

    }
}
