using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CHIP8
{
    public partial class Display : Form
    {
        public static int scalar = 10;
        public Display()
        {
            InitializeComponent();

            //Init chip
            Chip c = new Chip();

            //load the program into the chip
            c.loadProgram(@"D:\Downloads\pong2.c8");

            //infinite loop for the chip to run on
            for (;;)
            {
                c.run();
                if(c.needsRedraw())
                {
                    display();
                    c.removeDrawFlag();
                }
                Thread.Sleep(16);
            }
        }

        public void display()
        {
            if (displayGrid.Image == null)
            {
                displayGrid.Image = new Bitmap(displayGrid.Width, displayGrid.Height);
            }
            using (Graphics g = Graphics.FromImage(displayGrid.Image))
            {
                // draw black background
                g.Clear(Color.Black);
                Rectangle rect = new Rectangle(0, 0, 10, 10);
                g.FillRectangle(new SolidBrush(Color.White), rect);
                //g.DrawRectangle(Pens.Black, rect);
            }
            displayGrid.Invalidate();
    }

    }
}
