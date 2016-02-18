using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CHIP8
{
    public partial class Display : Form
    {
        //Init chip
        Chip c = new Chip();

        Rectangle[,] pixels = new Rectangle[64, 32];

        public Display()
        {
            InitializeComponent();
            
            //load the program into the chip
            c.loadFont();
            c.loadProgram(@"C:\Users\Nico\Documents\Visual Studio 2015\Projects\CHIP8\GAMES\pong.ch8");

            //init for window
            displayGrid.Image = new Bitmap(displayGrid.Width, displayGrid.Height);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    //set the positions for each rect
                    pixels[x, y] = new Rectangle(x * 10, y * 10, x * 10 + 10, y * 10 + 10);
                }
            }
            display();
            chipWorker.RunWorkerAsync(null);
        }

        public void display()
        {
            using (Graphics g = Graphics.FromImage(displayGrid.Image))
            {
                // draw black background
                g.Clear(Color.Black);

                byte[] disp = c.getDisplay();
                //Debug.WriteLine(string.Join("", disp));

                for (int i = 0; i < disp.Length; i++)
                {
                    if (disp[i] == 1)
                    {
                        //Debug.WriteLine("Index: " + i + ", COORDS: " + (i % 64) + ", " + System.Math.Floor((double)(i / 64)));
                        g.FillRectangle(new SolidBrush(Color.White), pixels[i % 64, (int)Math.Floor((double)(i / 64))]);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), pixels[i % 64, (int)Math.Floor((double)(i / 64))]);
                    }
                }
            }
            displayGrid.Invalidate();
        }

        private void chipWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //infinite loop for the chip to run on
            for (;;)
            {
                c.run();
                if (c.needsRedraw())
                {
                    Debug.WriteLine("REDRAWING");
                    display();
                    c.removeDrawFlag();
                }
                //Thread.Sleep(1000); //60 hertz
                //Thread.Sleep(16); //60 hertz
                Thread.Sleep(1); //
            }
        }

        //keypress event
        private void Display_KeyPress(object sender, KeyPressEventArgs e)
        {
            //map the key to chip8
            c.setKey((ushort)"1234qwerasdfzxcv".IndexOf(e.KeyChar));
        }

        private void Display_KeyUp(object sender, KeyEventArgs e)
        {
            c.setKey(0);
        }
    }
}
