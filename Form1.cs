using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;

namespace PlasticIndentificationEngine
{
    public partial class Form1 : Form
    {
        OpenFileDialog fileOpener = new OpenFileDialog();

        string emptyImagePath = String.Empty;

        List<string> imagePath = new List<string>();

        Image<Bgr, byte> RGBimage;

        Image<Bgr, byte> heatMapImage;

        int count;

        int imagesLength = 0;

        private int boundingBoxLength = 0;

        public Form1()
        {
            InitializeComponent();

            displayRGBImage.SizeMode = PictureBoxSizeMode.Zoom;
            displayHeatmapImage.SizeMode = PictureBoxSizeMode.Zoom;

            fileOpener.Multiselect = true;

            fileOpener.Filter = ("Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|" +
                                "All files (*.*)|*.*");

        }

        private void button1_Click(object sender, EventArgs e) //Loads in all of the images into the imagePath list
        {
            if (fileOpener.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < fileOpener.FileNames.Length; i = i + 1)
                {
                    imagePath.Add(fileOpener.FileNames[i]);

                    imagesLength = fileOpener.FileNames.Length;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void ApplyHeatmapAndShowImage(string pathImage)
        {

            RGBimage = new Image<Bgr, byte>(pathImage); //Gets the RGB image from the image path which is retrieved from the file opener
            
            heatMapImage = new Image<Bgr, byte>(pathImage); //Makes a copy of the image which 

            CvInvoke.ApplyColorMap(RGBimage, heatMapImage, ColorMapType.Jet); //Applies the heatmap colormap on the image, (overriding the original image)

            displayRGBImage.Image = RGBimage.ToBitmap();

            displayHeatmapImage.Image = heatMapImage.ToBitmap();

            
            Image<Gray, byte> boundingBoxImage = heatMapImage.SmoothBlur(10,10).Convert<Gray, byte>().ThresholdBinary(new Gray(150), new Gray(255));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            Mat hierarchy = new Mat();

            CvInvoke.FindContours(boundingBoxImage, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Ccomp, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            Dictionary<int, double> dictionary = new Dictionary<int, double>();

            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i = i + 1)
                {
                    double area = CvInvoke.ContourArea(contours[i]);
                    dictionary.Add(i, area);
                }
            }

            var item = dictionary.OrderByDescending(v => v.Value);
            int key = 0;

            foreach (var Item in item)
            {
                key = int.Parse(Item.Key.ToString());
                Rectangle rectangle = CvInvoke.BoundingRectangle(contours[key]);
                CvInvoke.Rectangle(heatMapImage, rectangle, new MCvScalar(255, 0, 191), 5);

            }

            label2.Text = key.ToString();


            displayHeatmapImage.Image = heatMapImage.ToBitmap();

            
        }   

        private void button3_Click(object sender, EventArgs e)
        {

            if (count == imagePath.Count - 1)
            {
                MessageBox.Show("Exceeded Image Count");
            } else
            {
                count = count + 1;

                ApplyHeatmapAndShowImage(imagePath[count]);
            } 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (count == 0)
            {
                MessageBox.Show("No more images to show!");
            } else
            {
                count = count - 1;

                ApplyHeatmapAndShowImage(imagePath[count]);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            count = 0;

            ApplyHeatmapAndShowImage(imagePath[0]);

            
        }

    }
}
