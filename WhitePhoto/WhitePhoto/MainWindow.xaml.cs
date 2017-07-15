using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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


namespace WhitePhoto
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string imagePath ="";
        public bool done = false;
        public bool sucess = false;


        public MainWindow()
        {
            InitializeComponent();
            checkBox.IsChecked = true;
            DataContext = this;
        }

        private Bitmap BitmapImage2Bitmap(Uri bitmapImage)
        {
            

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

       


        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog okienko = new OpenFileDialog();
            okienko.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
            if (okienko.ShowDialog() == true)
            {
                
                image.Source = new BitmapImage(new Uri(okienko.FileName));
                imagePath = okienko.FileName;
            }
            else
            {

                MessageBoxResult result1 = MessageBox.Show("Musisz wybrać obraz do wczytania!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result1 == MessageBoxResult.OK)
                {
                    return;
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            if (imagePath == "")
            { 
            MessageBoxResult result1 = MessageBox.Show("Załduj najpierw obraz!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (result1 == MessageBoxResult.OK)
            {
                goto escape;

            }
            }

            Bitmap bmpPic1 = BitmapImage2Bitmap(new Uri(imagePath));


            if (checkBox.IsChecked == true && checkBox1.IsChecked == true)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace(@"\", "/");
                Bitmap bmpPic2 = new Bitmap(path + "do.png");
                double width = 1.4 * bmpPic1.Width;
                double height = 1.4 * bmpPic1.Height;


                System.Drawing.Image white = new Bitmap((int)Math.Round(width), (int)Math.Round(height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics grp = Graphics.FromImage(white))
                {
                    grp.FillRectangle(
                        System.Drawing.Brushes.White, 0, 0, (int)Math.Round(width), (int)Math.Round(height));

                }
                using (Graphics g = Graphics.FromImage(white))
                {

                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.DrawImage(bmpPic1, new System.Drawing.Point((int)Math.Round(0.2 * white.Width / 1.4), (int)Math.Round(0.2 * white.Height / 1.4)));
                    g.DrawImage(bmpPic2, new System.Drawing.Point((int)Math.Round(0.7 * white.Width), (int)Math.Round(0.15 * white.Height)));


                }
                if (checkBox2.IsChecked == true)
                {
                    Bitmap toBitmap = new Bitmap(white);

                    string currentPath = imagePath;

                    Bitmap final = Compression.doCompress(toBitmap);

                    FileStream cmpSave = new FileStream(currentPath + "-white-compressed.png", FileMode.Create);
                    final.Save(cmpSave, ImageFormat.Png);
                    cmpSave.Close();
                }
                else
                {


                    SaveFileDialog okienko = new SaveFileDialog();
                    okienko.Filter = "Pliki PNG | *.png";
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                    if (okienko.ShowDialog() == true)
                    {


                        FileStream saveStream2 = new FileStream(okienko.FileName, FileMode.OpenOrCreate);
                        white.Save(saveStream2, ImageFormat.Png);
                        saveStream2.Close();
                    }

                }
            }
            else if (checkBox.IsChecked == true && checkBox1.IsChecked == false)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                path = path.Replace(@"\", "/");
                Bitmap bmpPic2 = new Bitmap(path + "do.png");

                using (Graphics gre = Graphics.FromImage(bmpPic1))
                {

                    gre.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                    gre.DrawImage(bmpPic2, new System.Drawing.Point((int)Math.Round(0.7 * bmpPic1.Width), (int)Math.Round(0.15 * bmpPic1.Height)));


                }

                if (checkBox2.IsChecked == true)
                {
                    Bitmap toBitmap = new Bitmap(bmpPic1);

                    string currentPath = imagePath;

                    Bitmap final = Compression.doCompress(toBitmap);

                    FileStream cmpSave = new FileStream(currentPath + "-white-compressed.png", FileMode.Create);
                    final.Save(cmpSave, ImageFormat.Png);
                    cmpSave.Close();
                }
                else
                {
                    

                    SaveFileDialog okienko = new SaveFileDialog();
                    okienko.Filter = "Pliki PNG | *.png";
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                    if (okienko.ShowDialog() == true)
                    {


                        FileStream saveStream2 = new FileStream(okienko.FileName, FileMode.OpenOrCreate);
                        bmpPic1.Save(saveStream2, ImageFormat.Png);
                        saveStream2.Close();
                    }
                    
                }
            }
            else if (checkBox1.IsChecked == true && checkBox.IsChecked == false)
            {

                double width = 1.4 * bmpPic1.Width;
                double height = 1.4 * bmpPic1.Height;


                System.Drawing.Image white = new Bitmap((int)Math.Round(width), (int)Math.Round(height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                using (Graphics grp = Graphics.FromImage(white))
                {
                    grp.FillRectangle(
                        System.Drawing.Brushes.White, 0, 0, (int)Math.Round(width), (int)Math.Round(height));

                }
                using (Graphics g = Graphics.FromImage(white))
                {

                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.DrawImage(bmpPic1, new System.Drawing.Point((int)Math.Round(0.2 * white.Width / 1.4), (int)Math.Round(0.2 * white.Height / 1.4)));
                    


                }
                if (checkBox2.IsChecked == true)
                {
                    Bitmap toBitmap = new Bitmap(white);

                    string currentPath = imagePath;

                    Bitmap final = Compression.doCompress(toBitmap);

                    FileStream cmpSave = new FileStream(currentPath + "-white-compressed.png", FileMode.Create);
                    final.Save(cmpSave, ImageFormat.Png);
                    cmpSave.Close();
                }
                else
                {


                    SaveFileDialog okienko = new SaveFileDialog();
                    okienko.Filter = "Pliki PNG | *.png";
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
                    if (okienko.ShowDialog() == true)
                    {


                        FileStream saveStream2 = new FileStream(okienko.FileName, FileMode.OpenOrCreate);
                        white.Save(saveStream2, ImageFormat.Png);
                        saveStream2.Close();
                    }

                }


            }
            else
                {
                MessageBoxResult result1 = MessageBox.Show("Błąd edycji obrazu! Skontaktuj się z wyrwórcą. Łukasz Granat tel. 785 077 010", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result1 == MessageBoxResult.OK)
                {
                    goto escape;
                 
                }
            }


            
            escape:
            Task.Delay(100);
        }
       

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            done = false;
             sucess = false;
        OpenFileDialog okienko = new OpenFileDialog();
            okienko.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
            if (okienko.ShowDialog() == true)
            {

                image.Source = new BitmapImage(new Uri(okienko.FileName));
                
            }
            else
            {
                MessageBoxResult result1 = MessageBox.Show("Musisz wybrać obraz do wczytania!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result1 == MessageBoxResult.OK)
                {
                    return;
                }
                goto escape;
            }
            Bitmap bmpPic1 = BitmapImage2Bitmap(new Uri(okienko.FileName));
            string path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace(@"\", "/");
            Bitmap bmpPic2 = new Bitmap(path +"do.png");
            
            
            double width =  1.4*bmpPic1.Width;
            double height = 1.4 * bmpPic1.Height;
           

            System.Drawing.Image white = new Bitmap((int)Math.Round(width), (int)Math.Round(height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics grp = Graphics.FromImage(white))
            {
                grp.FillRectangle(
                    System.Drawing.Brushes.White, 0, 0, (int)Math.Round(width), (int)Math.Round(height));
                
            }

            using (Graphics g = Graphics.FromImage(white))
            {
           
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;               
                g.DrawImage(bmpPic1, new System.Drawing.Point((int)Math.Round(0.2*white.Width/1.4), (int)Math.Round(0.2 * white.Height/1.4)));
                g.DrawImage(bmpPic2, new System.Drawing.Point((int)Math.Round(0.7 * white.Width ), (int)Math.Round(0.15 * white.Height )));


            }


           

            Bitmap toBitmap = new Bitmap(white);

            string currentPath = okienko.FileName;

            //Kompresja

            Bitmap final = Compression.doCompress(toBitmap);

            //Kompresja - koniec

            FileStream cmpSave = new FileStream(currentPath + "-white-compressed.png", FileMode.Create);
            final.Save(cmpSave, ImageFormat.Png);
            cmpSave.Close();

            escape:
            Task.Delay(100);

        }

        

    }
}
