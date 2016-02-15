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
            c.loadProgram(@"D:\Downloads\pong2.c8");

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
                Thread.Sleep(16);
            }
        }

        public void display()
        {
            using (Graphics g = Graphics.FromImage(displayGrid.Image))
            {
                // draw black background
                g.Clear(Color.Black);

                byte[] disp = c.getDisplay();
                for (int i = 0; i < (64 * 32); i++)
                {
                    if(disp[i] == 1)
                    {
                        g.FillRectangle(new SolidBrush(Color.White), pixels[i % 64, i % 32]);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(Color.Black), pixels[i % 64, i % 32]);
                    }
                }
            }
            displayGrid.Invalidate();
    }

    }
}
