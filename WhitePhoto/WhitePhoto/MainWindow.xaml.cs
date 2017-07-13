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
            if(checkBox.IsChecked == true && checkBox.IsChecked == true)
            {

            }
            else if (checkBox.IsChecked==true)
            {


            }
            else if (checkBox1.IsChecked==true)
            {


            }
            else
                {
                MessageBoxResult result1 = MessageBox.Show("Błąd edycji obrazu! Skontaktuj się z wyrwórcą. Łukasz Granat tel. 785 077 010", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result1 == MessageBoxResult.OK)
                {
                    goto escape;
                 
                }
            }

            SaveFileDialog okienko = new SaveFileDialog();
            okienko.Filter = "Pliki PNG | *.png";
            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
            if (okienko.ShowDialog() == true)
            {
  

                FileStream saveStream = new FileStream(okienko.FileName, FileMode.OpenOrCreate);
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));
                encoder.Save(saveStream);
                saveStream.Close();
            }
            escape:
            Task.Delay(100);
        }
       

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog okienko = new OpenFileDialog();
            okienko.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" + "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" + "Portable Network Graphic (*.png)|*.png";
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

            SaveFileDialog okienko1 = new SaveFileDialog();
            okienko1.Filter = "Pliki PNG | *.png";
            System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;
            if (okienko1.ShowDialog() == true)
            {
                FileStream saveStream = new FileStream(okienko1.FileName, FileMode.OpenOrCreate);
                white.Save(saveStream, ImageFormat.Png);
                saveStream.Close();
               
            }

            escape:
            Task.Delay(100);

        }
    }
}
