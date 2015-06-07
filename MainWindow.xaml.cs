using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using System.Drawing;

namespace WpfApplication_Color_Filling
{
    public struct Queue
    {
        public int x, y;

        public Queue(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public WriteableBitmap bm;
        //public Graphics graphics;
        byte blue = 51, green = 51, red = 51;
        byte[] byteArray;
        byte[] startColor;
        
        private Dictionary<int, Action<int, int, int, int>> _algorithms = new Dictionary<int, Action<int, int, int, int>>();
        List<Queue> queue = new List<Queue>();

        private void Swap<T>(ref T lhs, ref T rhs) { T temp; temp = lhs; lhs = rhs; rhs = temp; }

        public MainWindow()
        {
            InitializeComponent();
            initImage();
        }

        private void initImage()
        {
            loadBitmap("fro.png");
            image.Source = bm;
        }

        private void loadBitmap(string name)
        {
            BitmapSource bmp = BitmapFrame.Create(new Uri(name, UriKind.Relative), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 1);      // Just ignore the last parameter
            bm = null;
            bm = new WriteableBitmap(700, 480, 96, 96, bmp.Format, bmp.Palette);
            Int32Rect r = new Int32Rect(0, 0, 700, 480);
            bm.Lock();
            bmp.CopyPixels(r, bm.BackBuffer, bm.BackBufferStride * 480, bm.BackBufferStride);
            bm.AddDirtyRect(r);
            bm.Unlock();
            
            //return wbmp;
        }

        private void circle(int x1, int y1, int radius1)
        {
            int x = 0,
                y = radius1,
                delta = 1 - 2 * radius1,
                error = 0;
            while (y >= 0)
            {

                drawPixel(x1 + x, y1 + y);
                drawPixel(x1 + x, y1 - y);
                drawPixel(x1 - x, y1 + y);
                drawPixel(x1 - x, y1 - y);
                error = 2 * (delta + y) - 1;
                if ((delta < 0) && (error <= 0))
                {
                    delta += 2 * ++x + 1;
                    continue;
                }
                error = 2 * (delta - x) - 1;
                if ((delta > 0) && (error > 0))
                {
                    delta += 1 - 2 * --y;
                    continue;
                }
                x++;
                delta += 2 * (x - y);
                y--;
            }
        }

        private void drawPixel(int x1, int y1)
        {
            if (x1 < 1 || x1 > 699 || y1 < 1 || y1 > 479)
            {
                return;
            }
            byte[] colorData = { blue, green, red, 255 };
            Int32Rect rect = new Int32Rect(x1, y1, 1, 1);
            bm.WritePixels(rect, colorData, 4, 0);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            setPicture();
        }

        private void drawCircle(int x, int y)
        {
            for (int i = 1; i < 15; i++)
            {
                circle(x, y, i);
            }
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Point p = Mouse.GetPosition(image);
            //drawCircle((int)p.X, (int)p.Y);
            //down = true;
            Point p = Mouse.GetPosition(image);

            makeByteArray();
            copyPixels();
            setStartColor((int)p.X, (int)p.Y);

            int offset = (int)p.Y * bm.PixelWidth * bm.Format.BitsPerPixel / 8 + (int)p.X * bm.Format.BitsPerPixel / 8;
            if (byteArray[offset] == blue && byteArray[offset + 1] == green
                && byteArray[offset + 2] == red && byteArray[offset + 3] == 255)
            {
                return;
            }

            switch (comboAlgorithm.SelectedIndex)
            {
                case 0: floodFillRecursion((int)p.X, (int)p.Y, 0);
                    break;
                default: floodFillLoop((int)p.X, (int)p.Y);
                    break;
            }
            //floodFillRecursion((int)p.X, (int)p.Y, 0);

            bm.WritePixels(new Int32Rect(0, 0, bm.PixelWidth, bm.PixelHeight),
                byteArray,
                bm.PixelWidth * bm.Format.BitsPerPixel / 8,
                0);
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            //Point p = Mouse.GetPosition(image);

            //makeByteArray();
            //copyPixels();

            //floodFillRecursion((int)p.X, (int)p.Y);
            //if (down)
            //{
            //    drawCircle((int)p.X, (int)p.Y);
            //}
        }
                
        private void comboPicture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setPicture();
        }

        private void setPicture()
        {
            switch (comboPicture.SelectedIndex)
            {
                case 0: 
                    loadBitmap("fro.png");
                    image.Source = bm;
                    break;
                case 1: 
                    loadBitmap("boo.png");
                    image.Source = bm;
                    break;
                default: 
                    loadBitmap("elf.png");
                    image.Source = bm;
                    break;
            }
        }

        private void comboColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setColor(); 
        }

        private void setColor()
        {
            switch (comboColor.SelectedIndex)
            {
                case 0:
                    blue = 255;
                    green = 255;
                    red = 255;
                    break;
                case 1:
                    blue = 80;
                    green = 80;
                    red = 255;
                    break;
                case 2:
                    blue = 100;
                    green = 255;
                    red = 255;
                    break;
                case 3:
                    blue = 31;
                    green = 220;
                    red = 31;
                    break;
                case 4:
                    blue = 255;
                    green = 51;
                    red = 50;
                    break;
                case 5:
                    blue = 0;
                    green = 0;
                    red = 0;
                    break;
                case 6:
                    blue = 153;
                    green = 0;
                    red = 255;
                    break;
                default:
                    blue = 0;
                    green = 51;
                    red = 51;
                    break;
            }
        }

        private void makeByteArray()
        {
            byteArray = new byte[bm.PixelWidth * bm.PixelHeight * bm.Format.BitsPerPixel / 8];
        }

        private void copyPixels()
        {
            bm.CopyPixels(byteArray, 2800, 0);
        }

        private void setStartColor(int x, int y)
        {
            int offset = y * bm.PixelWidth * bm.Format.BitsPerPixel / 8 + x * bm.Format.BitsPerPixel / 8;
            startColor = new byte[4] 
            { 
                byteArray[offset],
                byteArray[offset + 1], 
                byteArray[offset + 2], 
                byteArray[offset + 3] 
            };
        }

        private int floodFillRecursion(int x, int y, int n)
        {
            if (x < 0 || x > bm.PixelWidth - 1 || y < 0 || y > bm.PixelHeight - 1 || n > 8000)
            {
                return n;
            }
            int offset = y * bm.PixelWidth * bm.Format.BitsPerPixel / 8 + x * bm.Format.BitsPerPixel / 8;

            if (byteArray[offset] != startColor[0] || byteArray[offset + 1] != startColor[1]
                || byteArray[offset + 2] != startColor[2] || byteArray[offset + 3] != startColor[3])
            {
                return n;
            }

            byteArray[offset] = blue;
            byteArray[offset + 1] = green;
            byteArray[offset + 2] = red;
            byteArray[offset + 3] = 255;

            n = floodFillRecursion(x, y - 1, ++n);

            n = floodFillRecursion(x + 1, y, ++n);

            n = floodFillRecursion(x, y + 1, ++n);

            n = floodFillRecursion(x - 1, y, ++n);

            return n;
        }

        private void floodFillLoop(int x, int y)
        {
            //Queue s, f;

            /* Clear buffer */
            queue.Clear();

            /* Set start offset and check if the color is not equal replacement one */
            int offset = y * bm.PixelWidth * bm.Format.BitsPerPixel / 8 + x * bm.Format.BitsPerPixel / 8;
            //if (byteArray[offset] == blue && byteArray[offset + 1] == green
            //    && byteArray[offset + 2] == red && byteArray[offset + 3] == 255)
            //    return;

            /* Add start point into buffer */
            queue.Add(new Queue(x, y));

            /* Check all buffer elements */
            while (queue.Count > 0)
            {
                //s = f = queue[0];
                int tempX = queue[0].x,
                    tempY = queue[0].y,
                    tempOffset = tempY * bm.PixelWidth * bm.Format.BitsPerPixel / 8 
                    + tempX * bm.Format.BitsPerPixel / 8;

                /* Check all elements to the west which are target-colored */
                while (tempX > -1 && isSameColorAsChange(tempOffset))
                {
                    /* Add north point to List if have to */
                    if (tempY - 1 > -1 &&
                        isSameColorAsChange(tempOffset - bm.PixelWidth * bm.Format.BitsPerPixel / 8))
                    {
                        queue.Add(new Queue(tempX, tempY - 1));
                    }

                    /* Add south point to List if have to */
                    if (tempY + 1 < bm.PixelHeight &&
                        isSameColorAsChange(tempOffset + bm.PixelWidth * bm.Format.BitsPerPixel / 8))
                    {
                        queue.Add(new Queue(tempX, tempY + 1));
                    }

                    /* Fill temp point */
                    byteArray[tempOffset] = blue;
                    byteArray[tempOffset + 1] = green;
                    byteArray[tempOffset + 2] = red;
                    byteArray[tempOffset + 3] = 255;

                    /* Go one point left */
                    tempX--;
                    tempOffset -= 4;
                }

                tempX = queue[0].x + 1;
                tempOffset = tempY * bm.PixelWidth * bm.Format.BitsPerPixel / 8
                    + tempX * bm.Format.BitsPerPixel / 8; 

                /* Check all elements to Japan which are target-colored  */
                while (tempX < bm.PixelWidth && isSameColorAsChange(tempOffset))
                {
                    /* Add north point to List if have to */
                    if (tempY - 1 > -1 &&
                        isSameColorAsChange(tempOffset - bm.PixelWidth * bm.Format.BitsPerPixel / 8))
                    {
                        queue.Add(new Queue(tempX, tempY - 1));
                    }

                    /* Add south point to List if have to */
                    if (tempY + 1 < bm.PixelHeight &&
                        isSameColorAsChange(tempOffset + bm.PixelWidth * bm.Format.BitsPerPixel / 8))
                    {
                        queue.Add(new Queue(tempX, tempY + 1));
                    }

                    /* Fill temp point */
                    byteArray[tempOffset] = blue;
                    byteArray[tempOffset + 1] = green;
                    byteArray[tempOffset + 2] = red;
                    byteArray[tempOffset + 3] = 255;

                    /* Go one point left */
                    tempX++;
                    tempOffset += 4;
                }

                /* Pop curent Q point */
                queue.RemoveAt(0);
            }
        }

        private bool isSameColorAsChange(int offset)
        {
            if (offset < 0 || offset > byteArray.Length - 1)
            {
                return false;
            }
            if (byteArray[offset] == startColor[0] &&
                    byteArray[offset + 1] == startColor[1] &&
                    byteArray[offset + 2] == startColor[2] &&
                    byteArray[offset + 3] == startColor[3])
            {
                return true;
            }
            return false;
        }

        private void buttonFill_Click(object sender, RoutedEventArgs e)
        {
            makeByteArray();
            copyPixels();
        }

    }
}