using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public string imagePath = "";
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
            //okienko.Multiselect = true;
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

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "Przetwarzam. To może zająć chwilę. Proszę czekać.";

            progressBar.IsIndeterminate = true;

            await StandardEvent ((bool)checkBox.IsChecked, (bool)checkBox1.IsChecked, (bool)checkBox2.IsChecked);
            
            progressBar.IsIndeterminate = false;
            textBlock.Text = "Zakończono! Możesz załadować kolejny obraz.";
        }


        private async void button2_Click(object sender, RoutedEventArgs e)
        {
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

            textBlock.Text = "Przetwarzam. To może zająć chwilę. Proszę czekać.";

            progressBar.IsIndeterminate = true;

            await ExpressEvent(okienko);
                        
            progressBar.IsIndeterminate = false;

            textBlock.Text = "Zakończono! Możesz załadować kolejny obraz.";

            escape:
            Task.Delay(100);
        }


        private async Task ExpressEvent(OpenFileDialog okienko)
        {
            await Task.Run(() =>
            {
                
                Bitmap bmpPic1 = BitmapImage2Bitmap(new Uri(okienko.FileName));
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


                double logoWidth;
                double logoHeight;
                if (width <= height)
                {
                    logoWidth = 0.25 * white.Width;
                    logoHeight = 0.25 * white.Width;
                }
                else
                {
                    logoWidth = 0.25 * white.Height;
                    logoHeight = 0.25 * white.Height;
                }

                Bitmap scaledbmpPic2 = new Bitmap(bmpPic2, (int)Math.Round(logoWidth), (int)Math.Round(logoHeight));

                using (Graphics g = Graphics.FromImage(white))
                {

                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.DrawImage(bmpPic1, new System.Drawing.Point((int)Math.Round(0.2 * white.Width / 1.4), (int)Math.Round(0.2 * white.Height / 1.4)));
                    g.DrawImage(scaledbmpPic2, new System.Drawing.Point((int)Math.Round(0.72 * white.Width), (int)Math.Round(0.1 * white.Height)));


                }




                Bitmap toBitmap = new Bitmap(white);

                string currentPath = okienko.FileName;

                //Kompresja

                Bitmap final = Compression.doCompress(toBitmap);

                //Kompresja - koniec

                FileStream cmpSave = new FileStream(currentPath + "-white-compressed.png", FileMode.Create);
                final.Save(cmpSave, ImageFormat.Png);
                cmpSave.Close();

                
            });
        }

        private async Task StandardEvent(bool checkBoxIsChecked, bool checkBox1IsChecked, bool checkBox2IsChecked )
        {
            await Task.Run(() =>
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


                if (checkBoxIsChecked == true && checkBox1IsChecked == true)
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

                    double logoWidth;
                    double logoHeight;
                    if (width <= height)
                    {
                        logoWidth = 0.25 * white.Width;
                        logoHeight = 0.25 * white.Width;
                    }
                    else
                    {
                        logoWidth = 0.25 * white.Height;
                        logoHeight = 0.25 * white.Height;
                    }

                    Bitmap scaledbmpPic2 = new Bitmap(bmpPic2, (int)Math.Round(logoWidth), (int)Math.Round(logoHeight));

                    using (Graphics g = Graphics.FromImage(white))
                    {

                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                        g.DrawImage(bmpPic1, new System.Drawing.Point((int)Math.Round(0.2 * white.Width / 1.4), (int)Math.Round(0.2 * white.Height / 1.4)));
                        g.DrawImage(scaledbmpPic2, new System.Drawing.Point((int)Math.Round(0.72 * white.Width), (int)Math.Round(0.1 * white.Height)));


                    }
                    if (checkBox2IsChecked == true)
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
                else if (checkBoxIsChecked == true && checkBox1IsChecked == false)
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    path = path.Replace(@"\", "/");
                    Bitmap bmpPic2 = new Bitmap(path + "do.png");

                    double logoWidth;
                    double logoHeight;
                    if (bmpPic1.Width <= bmpPic1.Height)
                    {
                        logoWidth = 0.25 * bmpPic1.Width;
                        logoHeight = 0.25 * bmpPic1.Width;
                    }
                    else
                    {
                        logoWidth = 0.25 * bmpPic1.Height;
                        logoHeight = 0.25 * bmpPic1.Height;
                    }

                    Bitmap scaledbmpPic2 = new Bitmap(bmpPic2, (int)Math.Round(logoWidth), (int)Math.Round(logoHeight));

                    using (Graphics gre = Graphics.FromImage(bmpPic1))
                    {

                        gre.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                        gre.DrawImage(scaledbmpPic2, new System.Drawing.Point((int)Math.Round(0.8 * bmpPic1.Width), (int)Math.Round(0.1 * bmpPic1.Height)));


                    }

                    if (checkBox2IsChecked == true)
                    {
                        Bitmap toBitmap = new Bitmap(bmpPic1);

                        string currentPath = imagePath;

                        Bitmap final = Compression.doCompress(toBitmap);

                        FileStream cmpSave = new FileStream(currentPath + "-compressed.png", FileMode.Create);
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
                else if (checkBox1IsChecked == true && checkBoxIsChecked == false)
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
                    if (checkBox2IsChecked == true)
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
                else if (checkBox1IsChecked == false && checkBoxIsChecked == false)
                {

                    if (checkBox2IsChecked == true)
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


                        MessageBoxResult result1 = MessageBox.Show("Musisz wykonać zmiany w obrazie! Zaznacz którąś z dostępnych funkcji!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (result1 == MessageBoxResult.OK)
                        {
                            goto escape;

                        }

                    }
                }
                else
                {
                    MessageBoxResult result1 = MessageBox.Show("Błąd edycji obrazu! Skontaktuj się z twórcą. Łukasz Granat tel. 785 077 010", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result1 == MessageBoxResult.OK)
                    {
                        goto escape;

                    }
                }



                escape:
                Task.Delay(100);
            });
        }

    }
}
