using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WhitePhoto
{
    class ColorQuantization
    {
        #region Members / Properties

        //Members
        private int imageWidth, imageHeight;
        private Bitmap originalImage;
        private Bitmap ditheredImage;
        private Bitmap generatedImage;
        private const int HSIZE = 32768;
        public int[] histogram; //Maps the # of pixels to a color
        public List<int> histPtr; //Maps a color to it's index in the histogram
        private RGBBox[] list;
        private int boxcount = 0;
        private int maxboxes;
        private int[] origHis;
        private Boolean grayscale;
        private int[,] colorMap;
        private List<Color> colorPalette;

        //Properties
        public int Width { get { return imageWidth; } }
        public int Height { get { return imageHeight; } }
        public Bitmap DitheredImage { get { return ditheredImage; } }
        public Bitmap GeneratedImage { get { return generatedImage; } }
        public int[] Histogram { get { return origHis; } }
        public int[,] ColorMap { get { return colorMap; } }

        #region Custom Event

        public event EventHandler<EventArgs> QuantizationCompleted;

        /// <summary>Used to Fire Quantization finished event, when the color quantization process has completed
        /// </summary>
        /// <param name="e"></param>
        public void OnQuantizationCompleted( EventArgs e )
        {
            if ( QuantizationCompleted != null )
                QuantizationCompleted(this, e);
        }

        #endregion

        //Enum used for the longest dimension of the box. Used to assist in sorting/splitting
        private enum LongAxis
        {
            Red,
            Green,
            Blue
        }

        #endregion

        #region Constructors

        /// <summary>Construct a new Color Quantization instance
        /// </summary>
        /// <param name="image"></param>
        /// <param name="maxboxes"></param>
        /// <param name="grayscale"></param>
        public ColorQuantization( Bitmap image, int maxboxes, Boolean grayscale )
        {
            if ( image == null )
                throw new Exception("Image does not exist!");

            originalImage = image;
            this.maxboxes = maxboxes;
            imageWidth = image.Width;
            imageHeight = image.Height;
            this.grayscale = grayscale;
        }

        /// <summary>Quantize the image with a pre-determined color palette
        /// </summary>
        /// <param name="image"></param>
        /// <param name="colorPalette"></param>
        public ColorQuantization( Bitmap image, List<Color> colorPalette )
        {
            if ( image == null )
                throw new Exception("Image does not exist!");

            originalImage = image;
            imageWidth = image.Width;
            imageHeight = image.Height;
            this.maxboxes = colorPalette.Count;

            this.colorPalette = colorPalette;
        }

        #endregion

        public Bitmap YouShallNotPass()
        {
            return GeneratedImage;

        }

        /// <summary>Start the Quantization of the image
        /// </summary>
        public void BeginQuantization()
        {
            histogram = new int[HSIZE];
            histPtr = new List<int>();

            ConvertRGBToInt(originalImage);

            list = new RGBBox[maxboxes];

            long timer = DateTime.Now.Ticks;

            BeginMedianCutQuantization();

            Console.Write("Quantization: {0}ms\n", (double) (DateTime.Now.Ticks - timer) / 10000.0);
            timer = DateTime.Now.Ticks;

            if ( colorPalette == null )
                GenerateColorMap();
            else
                ColorPaletteToColorMap();

            Console.Write("Color Map: {0}ms\n", (double) (DateTime.Now.Ticks - timer) / 10000.0);
            timer = DateTime.Now.Ticks;

            LoadImage();

            Console.Write("Image Loading: {0}ms\n", (double) (DateTime.Now.Ticks - timer) / 10000.0);
            timer = DateTime.Now.Ticks;

            OnQuantizationCompleted(new EventArgs()); //Fire Quantization finished event to notify any subscribers

            stop:
            Task.Delay(100);
        }

        /// <summary>Begins the median cut portion of the quantization
        /// </summary>
        private void BeginMedianCutQuantization()
        {
            RGBBox box, boxA = null, boxB = null;
            int color = 0;
            box = new RGBBox(0, 31, 0, 31, 0, 31);

            //Contruct the intial cube, which encloses all colors in the image
            for ( int i = 0; i < HSIZE; i++ )
            {
                if ( histogram[i] != 0 )
                {
                    histPtr.Add(i);
                    color++;
                    box.count += histogram[i];
                }
            }
            box.lower = 0;
            box.upper = color - 1;
            Shrink(ref box);

            list[boxcount] = box;
            boxcount++;

            int splitloc, median, level, count = 0;

            if ( colorPalette == null )
            {
                while ( boxcount < maxboxes )
                {
                    level = 255;
                    splitloc = -1;

                    for ( int k = 0; k < boxcount; k++ )
                    {
                        if ( list[k].lower == list[k].upper )
                            continue;
                        else if ( list[k].level < level )
                        {
                            level = list[k].level;
                            splitloc = k;
                        }
                    }
                    if ( splitloc == -1 )
                        break;

                    box = list[splitloc];

                    median = FindMedianLocation(box, ref count);

                    //Create box A which is the first half of the split of box
                    boxA = new RGBBox();
                    boxA.count = count;
                    boxA.lower = box.lower;
                    boxA.upper = median - 1;
                    boxA.level = box.level + 1;
                    Shrink(ref boxA);
                    list[splitloc] = boxA; //Replace old box with box A

                    //Box b which will contain the upper points in box
                    boxB = new RGBBox();
                    boxB.count = box.count - count;
                    boxB.lower = median;
                    boxB.upper = box.upper;
                    boxB.level = box.level + 1;
                    Shrink(ref boxB);
                    list[boxcount++] = boxB; //Add b to the end of the list
                }
            }

            origHis = (int[]) histogram.Clone();
        }

        /// <summary>Converts the Color Palette to use as the color map
        /// </summary>
        private void ColorPaletteToColorMap()
        {
            colorMap = new int[colorPalette.Count, 3];

            for ( int i = 0; i < colorPalette.Count; i++ )
            {
                colorMap[i, 0] = colorPalette[i].R;
                colorMap[i, 1] = colorPalette[i].G;
                colorMap[i, 2] = colorPalette[i].B;
            }

            int r, g, b, color;

            for ( int i = 0; i < histogram.Length; i++ )
            {
                histogram[i] = -1;
            }

            for ( int i = 0; i < histPtr.Count; i++ )
            {
                color = histPtr[i];
                r = Red(color);
                g = Green(color);
                b = Blue(color);

                histogram[color] = GetClosestColor(r, g, b);
            }
        }

        /// <summary>Updates histogram to function as a lookup table from oldcolor -> newcolor
        /// </summary>
        private void GenerateColorMap()
        {
            colorMap = new int[list.Length, 3];

            for ( int i = 0; i < list.Length; i++ )
            {
                GetBoxAverageColor(list[i], out colorMap[i, 0], out colorMap[i, 1], out colorMap[i, 2]);
            }

            int r, g, b, color;

            for ( int i = 0; i < histogram.Length; i++ )
            {
                histogram[i] = -1;
            }

            foreach ( RGBBox box in list )
            {
                for ( int i = box.lower; i <= box.upper; i++ )
                {
                    color = histPtr[i];
                    r = Red(color);
                    g = Green(color);
                    b = Blue(color);

                    histogram[color] = GetClosestColor(r, g, b);
                }
            }
        }

        /// <summary>Returns the output color given the rgb values as well as 
        /// updating r, g, and b
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Color GetOutputColor( ref int r, ref int g, ref int b )
        {
            int color = ToRGBFrom8Bit(r, g, b);
            int colorIndex = histogram[color];

            if ( colorIndex == -1 )
            {
                histogram[color] = GetClosestColor(ref r, ref g, ref b);
            }
            else
            {
                r = colorMap[colorIndex, 0];
                g = colorMap[colorIndex, 1];
                b = colorMap[colorIndex, 2];
            }

            return Color.FromArgb(r, g, b);
        }

        /// <summary>Returns the output color given the rgb values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private Color GetOutputColor( int r, int g, int b )
        {
            int color = ToRGBFrom8Bit(r, g, b);
            int colorIndex = histogram[color];

            if ( colorIndex == -1 )
            {
                histogram[color] = GetClosestColor(ref r, ref g, ref b);
            }
            else
            {
                r = colorMap[colorIndex, 0];
                g = colorMap[colorIndex, 1];
                b = colorMap[colorIndex, 2];
            }

            return Color.FromArgb(r, g, b);
        }

        /// <summary>Returns the index in the colorMap of the closest color 
        /// to the given r g b values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetClosestColor( int r, int g, int b )
        {
            double minDistance = double.MaxValue, temp;
            int index = 0;

            for ( int i = 0; i < colorMap.GetLength(0); i++ )
            {
                temp = GetEuclideanDistance(r, colorMap[i, 0], g, colorMap[i, 1], b, colorMap[i, 2]);

                if ( temp < minDistance )
                {
                    minDistance = temp;
                    index = i;
                }

                if ( minDistance == 0 )
                    break;
            }

            return index;
        }

        /// <summary>Returns the index in the colorMap of the closest color 
        /// to the given r g b values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int GetClosestColor( ref int r, ref int g, ref int b )
        {
            int index = GetClosestColor(r, g, b);

            r = colorMap[index, 0];
            g = colorMap[index, 1];
            b = colorMap[index, 2];

            return index;
        }

        /// <summary>Get the Euclidean distance between two points in the RGB color space
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        private double GetEuclideanDistance( int r1, int r2, int g1, int g2, int b1, int b2 )
        {
            return Math.Sqrt(Square(r1 - r2) + Square(g1 - g2) + Square(b1 - b2));
        }

        /// <summary>Square function for integers, since Math is doubles
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int Square( int x )
        {
            return x * x;
        }

        /// <summary>Returns the median location of pixels in box, also sets count to be the # of pixels
        /// </summary>
        /// <param name="box"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int FindMedianLocation( RGBBox box, ref int count )
        {
            int lenr, leng, lenb;

            LongAxis longdim;

            lenr = box.rmax - box.rmin;
            leng = box.gmax - box.gmin;
            lenb = box.bmax - box.bmin;

            if ( lenr >= leng && lenr >= lenb )
                longdim = LongAxis.Red;
            else if ( leng >= lenr && leng >= lenb )
                longdim = LongAxis.Green;
            else
                longdim = LongAxis.Blue;

            //Uses a delegate to correctly sort histPointer along the longest dimension
            histPtr.Sort(box.lower, box.upper - box.lower + 1, Comparer<int>.Create(( int x, int y ) =>
            {
                int n1, n2;
                switch ( longdim )
                {
                    case LongAxis.Red:
                        n1 = Red(x);
                        n2 = Red(y);
                        break;
                    case LongAxis.Green:
                        n1 = Green(x);
                        n2 = Green(y);
                        break;
                    default: // Blue case
                        n1 = Blue(x);
                        n2 = Blue(y);
                        break;
                }
                return n1 - n2;
            }));

            count = 0;
            int i;
            for ( i = box.lower; i < box.upper; i++ )
            {
                if ( count >= box.count / 2 )
                    break;
                count += histogram[histPtr[i]];
            }

            return i;
        }

        /// <summary>Shrinks the RGBBox to be the tighest fitted box given the points inside.
        /// Also updates the volume of box
        /// </summary>
        /// <param name="box"></param>
        private void Shrink( ref RGBBox box )
        {
            int rmin = 255, rmax = 0;
            int gmin = 255, gmax = 0;
            int bmin = 255, bmax = 0;

            int color, red, green, blue;

            for ( int i = box.lower; i <= box.upper; i++ )
            {
                color = histPtr[i];
                red = Red(color);
                green = Green(color);
                blue = Blue(color);

                //Update the min and max values for each color
                rmax = (red > rmax) ? red : rmax;
                rmin = (red < rmin) ? red : rmin;

                gmax = (green > gmax) ? green : gmax;
                gmin = (green < gmin) ? green : gmin;

                bmax = (blue > bmax) ? blue : bmax;
                bmin = (blue < bmin) ? blue : bmin;
            }

            box.rmax = rmax;
            box.rmin = rmin;
            box.gmax = gmax;
            box.gmin = gmin;
            box.bmax = bmax;
            box.bmin = bmin;

            box.volume = ((rmax - rmin) + 1) * ((gmax - gmin) + 1) * ((bmax - bmin) + 1);
        }

        /// <summary>Returns the average color for all points in the box
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        private void GetBoxAverageColor( RGBBox box, out int r, out int g, out int b )
        {
            int rtot = 0, gtot = 0, btot = 0;

            if (box==null)
            {
                MessageBoxResult result1 = MessageBox.Show("Ten obraz został już skwantowany. Nie możesz już powtórzyć procesu na tym pliku!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result1 == MessageBoxResult.OK)
                {
                    goto stop;
                }

            }


            //Sum the red, green, and blue components of all points in the box
            for ( int i = box.lower; i <= box.upper; i++ )
            {
                rtot += Red(histPtr[i]) * histogram[histPtr[i]];
                gtot += Green(histPtr[i]) * histogram[histPtr[i]];
                btot += Blue(histPtr[i]) * histogram[histPtr[i]];
            }

            r = rtot / box.count;
            g = gtot / box.count;
            b = btot / box.count;

            if ( grayscale ) //Use grayscale if we're quantizing in grayscale
            {
                int color = (int) (0.212 * r) + (int) (0.7512 * g) + (int) (0.0722 * b);
                r = g = b = color;
            }
        }

        /// <summary>Returns a single integer representing the color, given the r g and b values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ToRGBFrom5Bit( int r, int g, int b )
        {
            return r << 10 | g << 5 | b;
        }

        /// <summary>Converts 8 bit rgb values to an integer representation of 5 bit rgb values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ToRGBFrom8Bit( int r, int g, int b )
        {
            return ToRGBFrom5Bit((r & 255) >> 3, (g & 255) >> 3, (b & 255) >> 3);
        }

        /// <summary>Get the Red component of a 15 bit color
        /// </summary>
        /// <param name="color">15 Bit color value</param>
        /// <returns>8 bit r component</returns>
        private int Red( int color )
        {
            return ((color >> 10) << 3);
        }

        /// <summary>Get the Green component of a 15 bit color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private int Green( int color )
        {
            return (((color >> 5) & 31) << 3);
        }

        /// <summary>Get the Blue component of a 15 bit color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private int Blue( int color )
        {
            return ((color & 31) << 3);
        }

        /// <summary>Converts the given image to an array of integers
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private void ConvertRGBToInt( Bitmap image )
        {
            int histindex;

            Color pixel;

            for ( int i = 0; i < image.Width * image.Height; i++ )
            {
                int x = i % image.Width, y = i / image.Width;

                pixel = image.GetPixel(x, y);

                histindex = ToRGBFrom8Bit(pixel.R, pixel.G, pixel.B);

                histogram[histindex]++;
            }
        }

        /// <summary>Class used to store an RGBBox
        /// </summary>
        private class RGBBox
        {
            public int upper = 0, lower = 0;
            public int count = 0;
            public int level = 0;
            public int volume = 0; //Used to determine which box to subdivide. Divide based on highest count:volume ratio

            public int rmin, rmax;
            public int gmin, gmax;
            public int bmin, bmax;

            public RGBBox()
            { }

            public RGBBox( int rmin, int rmax, int gmin, int gmax, int bmin, int bmax )
            {
                this.bmax = bmax;
                this.bmin = bmin;
                this.rmax = rmax;
                this.rmin = rmin;
                this.gmax = gmax;
                this.gmin = gmin;
                this.level = 0;
            }
        }

        /// <summary>Generates the color quantized image and the dithered image
        /// </summary>
        public void LoadImage()
        {
            ditheredImage = new Bitmap(imageWidth, imageHeight);
            generatedImage = new Bitmap(imageWidth, imageHeight);
            Color c, temp;
            int r, g, b;
            int dr, dg, db; //Values to use for dithering
            int diffr, diffg, diffb;
            float mult = (1.0f / 16);
            int color;

            float[, ,] ditherMatrix = new float[imageWidth, imageHeight, 3];

            for ( int y = 0; y < imageHeight; y++ )
            {
                for ( int x = 0; x < imageWidth; x++ )
                {
                    temp = originalImage.GetPixel(x, y);

                    dr = r = diffr = temp.R;
                    dg = g = diffg = temp.G;
                    db = b = diffb = temp.B;

                    GetOutputColor(ref r, ref g, ref b);

                    c = Color.FromArgb(r, g, b);

                    generatedImage.SetPixel(x, y, c);

                    dr += (int) ditherMatrix[x, y, 0];
                    dg += (int) ditherMatrix[x, y, 1];
                    db += (int) ditherMatrix[x, y, 2];

                    //Enusure dr, dg, and db are within the 0 - 255 range after adding the dither error
                    EnsureInBounds(ref dr);
                    EnsureInBounds(ref dg);
                    EnsureInBounds(ref db);

                    //Map dr, dg, and db to one of the available colors
                    GetOutputColor(ref dr, ref dg, ref db);

                    if ( grayscale ) //Convert rgb values to greyscale
                    {
                        color = (int) (0.212 * dr) + (int) (0.7512 * dg) + (int) (0.0722 * db);
                        c = Color.FromArgb(color, color, color);
                    }
                    else
                        c = Color.FromArgb(dr, dg, db);

                    //Calculate the difference and use it in the dither matrix
                    diffr -= dr;
                    diffg -= dg;
                    diffb -= db;


                    /// Floyd-Steinberg dithering (all multiplied by a factor of 1/16)
                    ///     X   7
                    /// 3   5   1
                    if ( IsInBounds(x + 1, y) )
                    {
                        ditherMatrix[x + 1, y, 0] += mult * diffr * 7;
                        ditherMatrix[x + 1, y, 1] += mult * diffg * 7;
                        ditherMatrix[x + 1, y, 2] += mult * diffb * 7;
                    }
                    if ( IsInBounds(x - 1, y + 1) )
                    {
                        ditherMatrix[x - 1, y + 1, 0] += mult * diffr * 3;
                        ditherMatrix[x - 1, y + 1, 1] += mult * diffg * 3;
                        ditherMatrix[x - 1, y + 1, 2] += mult * diffb * 3;
                    }
                    if ( IsInBounds(x, y + 1) )
                    {
                        ditherMatrix[x, y + 1, 0] += mult * diffr * 5;
                        ditherMatrix[x, y + 1, 1] += mult * diffg * 5;
                        ditherMatrix[x, y + 1, 2] += mult * diffb * 5;
                    }
                    if ( IsInBounds(x + 1, y + 1) )
                    {
                        ditherMatrix[x + 1, y + 1, 0] += mult * diffr;
                        ditherMatrix[x + 1, y + 1, 1] += mult * diffg;
                        ditherMatrix[x + 1, y + 1, 2] += mult * diffb;
                    }

                    ditheredImage.SetPixel(x, y, c);
                }
            }
        }

        /// <summary>Returns whether or not the x and y values are in the bounds of the array
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsInBounds( int x, int y )
        {
            return (x < imageWidth && y < imageHeight && x >= 0 && y >= 0);
        }

        /// <summary>Ensures that the color component is in the range 0 to 255 inclusive
        /// </summary>
        /// <param name="colorComponent"></param>
        private void EnsureInBounds( ref int colorComponent )
        {
            if ( colorComponent > 255 )
                colorComponent = 255;
            else if ( colorComponent < 0 )
                colorComponent = 0;
        }
    }
}
