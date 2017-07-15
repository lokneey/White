using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Quantization process was inspired, by thi Github source. Some parts of code are the same.
//https://github.com/Int0blivion/Color-Quantization

namespace WhitePhoto
{
    class Compression
    {

        public static Bitmap doCompress(Bitmap src)
        {

            ColorQuantization cmp = new ColorQuantization(src, 255, false);

            cmp.BeginQuantization();

            Bitmap final = cmp.YouShallNotPass();

            return final;
            
            

            
        }

    }
}
