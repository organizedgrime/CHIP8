using System;
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

        //REFRESHRATE measured in Hz
        public static int REFRESHRATE = 60, SCALE = 10;

        public Display()
        {
            InitializeComponent();
            //load the program into the chip
            c.loadFont();

            //let the user choose their file
            using (OpenFileDialog fileChooserDialog = new OpenFileDialog())
            {
                if (fileChooserDialog.ShowDialog() == DialogResult.OK)
                {
                    c.loadProgram(fileChooserDialog.FileName);
                }
            }

            //@"C:\Users\Nico\Documents\Visual Studio 2015\Projects\CHIP8\GAMES\pong.ch8");

            //init for window
            displayGrid.Image = new Bitmap(displayGrid.Width, displayGrid.Height);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    //set the positions for each rect
                    pixels[x, y] = new Rectangle(x * SCALE, y * SCALE, x * SCALE + SCALE, y * SCALE + SCALE);
                }
            }

            new Thread(chipWorker).Start();
            new Thread(audioWorker).Start();

        }

        public void display()
        {
            using (Graphics g = Graphics.FromImage(displayGrid.Image))
            {
                // draw black background
                g.Clear(Color.Black);

                SolidBrush whiteBrush = new SolidBrush(Color.White), blackBrush = new SolidBrush(Color.Black);

                byte[] disp = c.display;
                //Debug.WriteLine(string.Join("", disp));

                for (int i = 0; i < disp.Length; i++)
                {
                    if (disp[i] == 1)
                    {
                        //Debug.WriteLine("Index: " + i + ", COORDS: " + (i % 64) + ", " + System.Math.Floor((double)(i / 64)));
                        g.FillRectangle(whiteBrush, pixels[i % 64, (int)Math.Floor((double)(i / 64))]);
                    }
                    else
                    {
                        g.FillRectangle(blackBrush, pixels[i % 64, (int)Math.Floor((double)(i / 64))]);
                    }
                }
            }
            displayGrid.Invalidate();
        }

        private void chipWorker()
        {
            Double msPerFrame = 1000.0 / REFRESHRATE;

            //infinite loop for the chip to run on
            for (;;)
            {
                DateTime frameStart = DateTime.Now;
                DateTime frameEnd = frameStart.AddMilliseconds(msPerFrame);

                c.run();
                if (c.needRedraw)
                {
                    //Debug.WriteLine("REDRAWING");
                    display();
                    c.needRedraw = false;
                }

                TimeSpan ts = frameEnd.Subtract(frameStart);
                if (ts.Milliseconds > 0.0)
                {
                    Debug.WriteLine("Sleeping for {0}ms.", ts.Milliseconds);
                    Thread.Sleep(ts.Milliseconds); //Can be changed to determine speed
                }
            }
        }

        private void audioWorker()
        {
            for (;;)
            {
                if (c.sound_timer > 0)
                {
                    //TODO need to modify this, it stops the game for a bit
                    Console.Beep(2000, 17);
                }
            }
        }

        //keypress event
        private void Display_KeyPress(object sender, KeyPressEventArgs e)
        {
            //map the key to chip8
            c.keyPressed = (ushort)"1234qwerasdfzxcv".IndexOf(e.KeyChar);
        }

        private void Display_KeyUp(object sender, KeyEventArgs e)
        {
            c.keyPressed = 0;
        }
    }
}