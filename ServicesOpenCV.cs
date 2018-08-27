using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OldWebAPi
{
    static public class ServicesOpenCV
    {
        static public object Preprocess(string inputPath)
        {
            object retObj = null;

            Image<Bgr, Byte> imRaw = LoadImage<Bgr, byte>(inputPath);
            {
                double factor = 1.0;
                double heightOrig = imRaw.Height;
                double widthOrig = imRaw.Width;
                if (widthOrig > 600)
                    factor = 600 / widthOrig;

                int resizedHeight = (int)(heightOrig * factor);
                int resizedWidth = (int)(widthOrig * factor);

                Image<Bgr, Byte> img = imRaw.Resize(factor, Inter.Linear);
                Image<Bgr, Byte> imgAllCountor = imRaw.Resize(factor, Inter.Linear);
                Image<Bgr, Byte> imgFinalCountor = imRaw.Resize(factor, Inter.Linear);

                //# Remove Noise from image (this helps in filtering-out small contours)
                Image<Bgr, Byte> deNoisedImg = new Image<Bgr, byte>(img.Size);
                //Image<Bgr, Byte> deNoisedImg = CvInvoke.FastNlMeansDenoisingColored(img, deNoisedImg, 10, 10, 7, 21);
                CvInvoke.FastNlMeansDenoisingColored(img, deNoisedImg, 10f, 10f, 7, 21);

                //# Convert the image to HSV (We convert image to HSV as we want to extract object that has a contrasting HUE-color than the background HUE-color
                //HsvImg = cv2.cvtColor(deNoisedImg, cv2.COLOR_BGR2HSV)
                Image<Hsv, Byte> HsvImg = new Image<Hsv, byte>(deNoisedImg.Bitmap);

                //# Extract the HUE channel because this is the channel which gives best result
                Image<Gray, byte>[] HsvChannels = HsvImg.Split();

                Image<Bgr, Byte> imgRGB = new Image<Bgr, byte>(HsvImg.Size);
                CvInvoke.Merge(new VectorOfMat(HsvChannels[0].Mat, HsvChannels[1].Mat, HsvChannels[2].Mat), imgRGB);

                //# Convert to BW image
                Image<Gray, Byte> threshed_img = new Image<Gray, byte>(img.Size);
                CvInvoke.Threshold(HsvChannels[0], threshed_img, 125, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);

                //image, contours, hier = CvInvoke.FindContours(threshed_img, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

                var contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(threshed_img, contours, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                Rectangle maxRect = new Rectangle(0, 0, 0, 0);

                for (int idx = 0; idx < contours.Size; idx++)
                {
                    var c = contours[idx];
                    Rectangle boundingRect = CvInvoke.BoundingRectangle(c);

                    //cv2.rectangle(imgAllCountor, (x, y), (x + w, y + h), (0, 255, 0), 2)
                    CvInvoke.Rectangle(imgAllCountor, boundingRect, new MCvScalar(0, 255, 0), 2);

                    //# get the min area rect
                    //rect = cv2.minAreaRect(c)
                    //box = cv2.boxPoints(rect)
                    RotatedRect rect = CvInvoke.MinAreaRect(c);

                    Rectangle box = rect.MinAreaRect();

                    int hh = box.Height;
                    int ww = box.Width;

                    int hhh = 0;
                    int www = 0;
                    if (hh > ww)
                    {
                        hhh = ww;
                        www = hh;
                    }
                    else
                    {
                        hhh = hh;
                        www = ww;
                    }

                    if (www > resizedWidth * 0.3) {

                        if ((hhh * 1.8 > www) && (hhh < www * 1.4))
                        {
                            CvInvoke.Rectangle(imgFinalCountor, box, new MCvScalar(0, 255, 255), 3);
                            if(maxRect.Left == 0)
                            {
                                maxRect = box;
                            }
                        }
                    }

                }

                Image<Bgr, Byte> imageCrop = img.Clone();

                if (maxRect.Left > 0)
                {
                    imageCrop = img.GetSubRect(maxRect);
                }

                //ImageViewer.Show(deNoisedImg, "deNoisedImg");
                //ImageViewer.Show(imgRGB, "HsvImg");
                //ImageViewer.Show(HsvChannels[1], "HsvChannels");
                //ImageViewer.Show(threshed_img, "threshed_img");
                //ImageViewer.Show(imgAllCountor, "imgAllCountor");
                //ImageViewer.Show(imgFinalCountor, "imgFinalCountor");
                //ImageViewer.Show(imageCrop, "imageCrop");

                deNoisedImg.Save(@"F:\Z\tempout_1.jpg");
                imgRGB.Save(@"F:\Z\tempout_2.jpg");
                threshed_img.Save(@"F:\Z\tempout_3.jpg");
                imgAllCountor.Save(@"F:\Z\tempout_4.jpg");

                imgFinalCountor.Save(@"F:\Z\tempout_5.jpg");
                imageCrop.Save(@"F:\Z\tempout.jpg");
                
                retObj = imageCrop;
                retObj = File.OpenRead(@"F:\Z\tempout.jpg");
            }

            return retObj;
        }


        static public object Test(string inputPath)
        {
            object retObj = null;

            inputPath = @"F:\Z\Binarization\1.jpg";

            Image<Rgb, Byte> image = LoadImage<Rgb, Byte>(inputPath);


            Image<Gray, byte>[] channels = image.Split();

            Image<Hsv, Byte> imageHSV = LoadImage<Hsv, Byte>(inputPath);
            Image<Gray, byte>[] HSVchannels = imageHSV.Split();

            Image<Gray, byte> result = channels[0].ConcateHorizontal(channels[1]).ConcateHorizontal(channels[2]);
            Image<Gray, byte> hsvResult = HSVchannels[0].ConcateHorizontal(HSVchannels[1]).ConcateHorizontal(HSVchannels[2]);

            Image<Gray, byte> fResult = result.ConcateVertical(hsvResult);

            Image<Gray, byte> nResult = new Image<Gray, byte>(image.Size);
            CvInvoke.GaussianBlur(HSVchannels[1], nResult, new Size(5, 5), 0);
            //CvInvoke.AdaptiveThreshold(nResult, nResult, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary , 8, 1);
            CvInvoke.Threshold(nResult, nResult, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);

            Image<Gray, Byte> edges = new Image<Gray, byte>(image.Size);

            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(9, 9), new Point(-1, -1));
            CvInvoke.MorphologyEx(nResult, nResult, MorphOp.Close, kernel, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(2));

            CvInvoke.Canny(nResult, edges, 51, 150, 3);
            LineSegment2D[][] lines = edges.HoughLinesBinary(0.1, Math.PI / 180.0, 1, 30, 0);
            foreach (LineSegment2D[] line in lines)
            {

                foreach (LineSegment2D l in line)
                {

                    CvInvoke.Line(image, l.P1, l.P2, new MCvScalar(255, 0, 0), 5);

                }
            }

            ImageViewer.Show(edges);

            ImageViewer.Show(image);

            //result.Save(@"F:\Z\tempout.jpg");
            //retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;
        }

        static public object ConvertToBinaryImage(string inputPath)
        {
            object retObj = null;

            Image<Gray, Byte> image = LoadImage<Gray, byte>(inputPath);
            {
                Image<Gray, Byte> thresh1 = new Image<Gray, byte>(image.Size);
                Image<Gray, Byte> thresh2 = new Image<Gray, byte>(image.Size);
                CvInvoke.Threshold(image, thresh1, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);
                CvInvoke.Threshold(image, thresh2, 255, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);

                ImageViewer.Show(thresh2);
                retObj = thresh1;
                thresh1.Save(@"F:\Z\tempout.jpg");
                retObj = File.OpenRead(@"F:\Z\tempout.jpg");
            }

            return retObj;
        }

        static public object ConvertToDenoiseImage(string inputPath)
        {
            object retObj = null;

            Image<Gray, Byte> image1 = LoadImage<Gray, Byte>(inputPath);
            Image<Gray, Byte> image = new Image<Gray, byte>(image1.Size);
            CvInvoke.Threshold(image1, image, 255, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);
            Image<Gray, Byte> result = new Image<Gray, byte>(image.Size);
            CvInvoke.FastNlMeansDenoising(image, result, 3f, 7, 21);

            ImageViewer.Show(image.ConcateHorizontal(result));
            result.Save(@"F:\Z\tempout.jpg");
            retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;
        }

        static public object ConvertToRGBSeparateChannelsImage(string inputPath)
        {
            object retObj = null;

            Image<Rgb, Byte> image = LoadImage<Rgb, Byte>(inputPath);
            Image<Gray, byte>[] channels = image.Split();

            Image<Gray, byte> result = channels[0].ConcateHorizontal(channels[1]).ConcateHorizontal(channels[2]);
            ImageViewer.Show(result);

            result.Save(@"F:\Z\tempout.jpg");
            retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;
        }

        public static Image<TColor, TDepth> LoadImage<TColor, TDepth>(String name)
         where TColor : struct, IColor
         where TDepth : new()
        {
            return new Image<TColor, TDepth>(name);
        }

        public static void CropImage(string inputPath, int x, int y, int width, int height)
        {
            object retObj = null;

            Image<Gray, Byte> image = LoadImage<Gray, Byte>(inputPath);
            Rectangle roi = new Rectangle(x, y, width, height);
            var result = image.GetSubRect(roi);

            result.Save(@"F:\Z\tempout.jpg");
            retObj = File.OpenRead(@"F:\Z\tempout.jpg");

        }

        internal static object DetectFace(string inputPath)
        {
            //inputPath = @"C:\Users\Yogaashi\Downloads\Face-Detection-and-Recognition-master\Face-Detection-and-Recognition-master\FaceDetection\bin\Debug\example_080.jpg";

            CascadeClassifier _cascadeClassifier;
            _cascadeClassifier = new CascadeClassifier(@".\haarcascade_frontalface_alt_tree.xml");
            using (Image<Bgr, Byte> imageOrig = LoadImage<Bgr, Byte>(inputPath))
            {
                double resizeFactor = 1;
                if (imageOrig.Height > 600)
                {
                    resizeFactor = 600 / (double)imageOrig.Height;
                }

                Image<Bgr, Byte> imageFrame = imageOrig.Resize(resizeFactor, Inter.Linear);

                if (imageFrame != null)
                {
                    var grayframe = imageFrame.Convert<Bgr, byte>();
                    var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.5, 3, Size.Empty); //the actual face detection happens here

                    foreach (var face in faces)
                    {
                        imageFrame.Draw(face, new Bgr(0, 255, 0), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them
                        imageFrame.Draw(new Rectangle(face.X - (int)(face.Height * 0.40), face.Y - (int)(face.Height * 1.20), 
                                        (int)(face.Height * 4.60),
                                        (int)(face.Height * 3.0) ), 
                                        new Bgr(155, 155, 0), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them
                    }
                }
                ImageViewer.Show(imageFrame);
                imageFrame.Save(@"F:\Z\tempout.jpg");
            }

            object retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;
        }

        internal static object DetectLogo(string inputPath)
        {
            inputPath = @"C:\Users\Yogaashi\PycharmProjects\HaarLogoDetection-master\images\original.jpg";

            CascadeClassifier _cascadeClassifier;
            _cascadeClassifier = new CascadeClassifier(@".\chase_new.xml");
            using (Image<Bgr, Byte> imageOrig = LoadImage<Bgr, Byte>(inputPath))
            {
                double resizeFactor = 1;
                if (imageOrig.Height > 800)
                {
                    resizeFactor = 800 / (double)imageOrig.Height;
                }

                Image<Bgr, Byte> imageFrame = imageOrig.Resize(resizeFactor, Inter.Linear);

                if (imageFrame != null)
                {
                    var grayframe = imageFrame.Convert<Bgr, byte>();
                    var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 7, Size.Empty);
                    int a = 255;
                    foreach (var face in faces)
                    {
                        imageFrame.Draw(face, new Bgr(a, 255, 0), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them
                        //ImageViewer.Show(imageFrame);
                        a = (a + 255) % 255;
                    }
                }
                ImageViewer.Show(imageFrame);
                imageFrame.Save(@"F:\Z\tempout.jpg");
            }

            object retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;
        }

        internal static object DetectCommonObject(string inputPath)
        {
            byte[] pt = File.ReadAllBytes("MobileNetSSD_deploy.prototxt");
            byte[] cf = File.ReadAllBytes("MobileNetSSD_deploy.caffemodel");


            Emgu.CV.Dnn.Net net = Emgu.CV.Dnn.DnnInvoke.ReadNetFromCaffe(pt, cf);

            int pizsize = 300;
            Image<Rgb, Byte> imageOrig = LoadImage<Rgb, Byte>(inputPath);

            double resizeFactor = 1;
            if (imageOrig.Height > 800)
            {
                resizeFactor = 800 / (double)imageOrig.Height;
            }


            Image<Rgb, Byte> image = imageOrig.Resize(resizeFactor, Inter.Linear);
            Image<Rgb, Byte> resizedImage = image.Resize((int)pizsize, (int)pizsize, Inter.Linear);

            //Image<Gray, Byte> tImg = new Image<Gray, byte>(resizedImage.Size);
            //CvInvoke.Threshold(resizedImage, tImg, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);

            MCvScalar mm = new MCvScalar(127.5);

            var blob = Emgu.CV.Dnn.DnnInvoke.BlobFromImages(new Mat[] { resizedImage.Mat }, 0.007843, new Size(pizsize, pizsize), mm);

            //ImageViewer.Show(resizedImage);

            net.SetInput(blob, "data");
            var prob = net.Forward("detection_out");

            byte[] data = prob.GetData();
            string[] Labels = new string[]
                {"background", "aeroplane", "bicycle", "bird", "boat",
                    "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
                    "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
                    "sofa", "train", "tvmonitor" };


            //draw result
            for (int i = 0; i < prob.SizeOfDimemsion[2]; i++)
            {
                var d = BitConverter.ToSingle(data, i * 28 + 8);
                if (d > 0.2)
                {
                    var idx = (int)BitConverter.ToSingle(data, i * 28 + 4);
                    var w1 = (int)(BitConverter.ToSingle(data, i * 28 + 12) * image.Width);
                    var h1 = (int)(BitConverter.ToSingle(data, i * 28 + 16) * image.Height);
                    var w2 = (int)(BitConverter.ToSingle(data, i * 28 + 20) * image.Width);
                    var h2 = (int)(BitConverter.ToSingle(data, i * 28 + 24) * image.Height);

                    string[] output_last = new string[] { Labels[idx], w1.ToString(), w2.ToString(), (h1).ToString(), (h2).ToString() };

                    //output_coordinates.Add(output_last);

                    var label = $"{Labels[idx]} {d * 100:0.00}%";

                    //output.Add(Labels[idx]);

                    CvInvoke.Rectangle(image, new Rectangle(w1, h1, w2 - w1, h2 - h1), new MCvScalar(0, 255, 0), 2);
                    int baseline = 0;
                    var textSize = CvInvoke.GetTextSize(label, FontFace.HersheyTriplex, 0.5, 1, ref baseline);
                    var y = h1 - textSize.Height < 0 ? h1 + textSize.Height : h1;
                    CvInvoke.Rectangle(image, new Rectangle(w1, y - textSize.Height, textSize.Width, textSize.Height), new MCvScalar(0, 255, 0), -1);
                    CvInvoke.PutText(image, label, new Point(w1, y), FontFace.HersheyTriplex, 0.5, new Bgr(0, 0, 0).MCvScalar);
                }
            }

            //ImageViewer.Show(image);

            image.Save(@"F:\Z\tempout.jpg");
            object retObj = File.OpenRead(@"F:\Z\tempout.jpg");

            return retObj;

        }

        //public void TestDnn1()
        //{
        //    Emgu.CV.Dnn.Net net = new Emgu.CV.Dnn.Net();
        //    using (Emgu.CV.Dnn.Net. Importer importer = Emgu.CV.Dnn.Importer.CreateCaffeImporter("bvlc_googlenet.prototxt", "bvlc_googlenet.caffemodel"))
        //        importer.PopulateNet(net);

        //    Mat img = EmguAssert.LoadMat("space_shuttle.jpg");
        //    CvInvoke.Resize(img, img, new Size(224, 224));
        //    Dnn.Blob inputBlob = new Dnn.Blob(img);
        //    net.SetBlob(".data", inputBlob);
        //    net.Forward();
        //    Dnn.Blob probBlob = net.GetBlob("prob");
        //    int classId;
        //    double classProb;
        //    GetMaxClass(probBlob, out classId, out classProb);
        //    String[] classNames = ReadClassNames("synset_words.txt");
        //}

        internal static void MyTest(string v)
        {
            string inputPath = @"F:\z\sig2.jpg";
            object retObj = null;


            Image<Gray, Byte> image = LoadImage<Gray, Byte>(inputPath);

            Image<Gray, Byte> oimage = new Image<Gray, byte>(image.Size);

            CvInvoke.AdaptiveThreshold(image, oimage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 7, 7);

            Image<Gray, Byte> tImg = new Image<Gray, byte>(image.Size);
            CvInvoke.Threshold(image, tImg, 255, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);
            //ImageViewer.Show(tImg);

            Image<Gray, Byte> cannyImg = new Image<Gray, byte>(image.Size);
            CvInvoke.Canny(tImg, cannyImg, 0, 255);
            //ImageViewer.Show(cannyImg);

            ElementShape shape = new ElementShape();
            Mat ia = CvInvoke.GetStructuringElement(shape, new Size(10, 1), new Point(-1, -1));

            Image<Gray, Byte> shapeImg = new Image<Gray, byte>(image.Size);

            CvInvoke.MorphologyEx(cannyImg, shapeImg, MorphOp.Close, ia, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(1));
            ImageViewer.Show(shapeImg);

            var coutour = new VectorOfVectorOfPoint();

            var coutour1 = new VectorOfVectorOfPoint();

            CvInvoke.FindContours(shapeImg, coutour, null, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            Rectangle maxRect = new Rectangle(0, 0, 0, 0);
            for (int idx = 0; idx < coutour.Size; idx++)
            {
                var currentArea = CvInvoke.ContourArea(coutour[idx]);
                Rectangle boundingRect = CvInvoke.BoundingRectangle(coutour[idx]);

                CvInvoke.Rectangle(oimage, boundingRect, new MCvScalar(2));
                var maxRectArea = (double)(maxRect.Width * maxRect.Height);
                if (boundingRect.Height > maxRect.Height)
                {
                    if (boundingRect.Height < image.Height * 0.5)
                        maxRect = boundingRect;
                }
            }

            ImageViewer.Show(oimage);
            CvInvoke.Rectangle(image, maxRect, new MCvScalar(0));
            ImageViewer.Show(image);
        }


        internal static void MyTestSplit(string inputPath)
        {
            Image<Gray, Byte> image = LoadImage<Gray, Byte>(inputPath);

            int w = image.Width;
            int h = image.Height;

            int rows = 16;
            int cols = 8;

            int a = w / cols;
            int b = h / rows;

            for (int i = 0; i < cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Rectangle roi = new Rectangle(a * i + 12, b * j + 15, a - 15, b - 18);
                    var res = image.GetSubRect(roi);
                    //res = res.Resize(1, Inter.Linear);
                    using (var contours = new VectorOfVectorOfPoint())
                    {
                        //CvInvoke.FindNonZero(res, contours); //, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                        //Rectangle re = CvInvoke.BoundingRectangle(res);
                        //var res1 = res.GetSubRect(re);
                        //ImageViewer.Show(res1);
                    }

                    Image<Gray, Byte> binRes = new Image<Gray, byte>(res.Size);
                    CvInvoke.Threshold(res, binRes, 0, 255, Emgu.CV.CvEnum.ThresholdType.Otsu | Emgu.CV.CvEnum.ThresholdType.Binary);
                    binRes.Save("F:\\Z\\SN_" + i + "_" + j + ".jpg");
                }
            }


            //ImageViewer.Show(res);

        }
    }

}
