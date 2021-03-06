﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Emgu;
using Emgu.CV.Face;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.Drawing;
using System.Diagnostics;

namespace CV
{
    class Program
    {
        ~Program()
        {
            Directory.Delete(Images, true);
            Directory.Delete("UserFace", true);
        }

        static FaceRecognizer f = new EigenFaceRecognizer(80, Double.PositiveInfinity);

        static private string Images = "SampleImages";
        static private string Faces = @"SampleImages\Faces";
        static private string Video = @"vid.mp4";
        static private string sample = @"id.jpg";
        static private string ImageData = "TrainedImages.txt";
        static private readonly CascadeClassifier _cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_default.xml");

        static void Main(string[] args)
        {
            Console.WriteLine("INTO MAIN");
            //if (args.Length == 2)
            //{
            //    if (File.Exists(args[0]) && File.Exists(args[1]))
            //    {
            //if (Path.GetExtension(args[0]) == "mp4" && ((Path.GetExtension(args[1])) == "JPG" || (Path.GetExtension(args[1])) == "png" || (Path.GetExtension(args[1])) == "PNG" || (Path.GetExtension(args[1])) == "png"))
            //{
            Console.WriteLine("PROCESSING...");
            string images = Process(Video);
            Console.Write("DETECTING ...");
            Detection();
            //            return;
            Console.WriteLine("RECOGNIZING...");
            bool recog = TrainRecognizer();
            if (!recog)
            {
                Console.WriteLine("INVALID INPUT");
                return;
            }
            if (File.Exists(sample))
            {
                var fileLength = new FileInfo(sample).Length;
                var Filesample = new FileInfo(sample);
                if (fileLength > 1024 * 1024)
                {
                    Console.WriteLine(fileLength);
                    Console.WriteLine("The file Size exceed 1MB");
                }
                else
                {
                    if (RecognizeUser(new Image<Bgr, Byte>(sample)))
                        Console.WriteLine("WELCOME");
                    else
                        Console.WriteLine("You are not welcome");
                }

            }
            else
            {
                Console.WriteLine("The ID (sample) image is not availible or errorneous");
            }
            //                    }
            //else
            //                      Console.WriteLine("Format Error.");
            //    }
            //    else
            //        Console.WriteLine("One or Both File(s) missing.");
            //}
            //else
            //{
            //    Console.WriteLine("ARG Length Wrong");
            //}
            Console.Read();

        }
        static public bool TrainRecognizer(int UserId = 0)
        {
            DirectoryInfo SampleFaces = new DirectoryInfo(Faces);
            int count = 0;
            foreach (FileInfo f in SampleFaces.GetFiles("*.jpg"))
            {
                count++;
            }
            Console.WriteLine("NUMBER OF FACE = " + count);



            if (count > 0)
            {
                Console.Write("working to resize ...");
                var faceImages = new Image<Gray, byte>[count];
                var faceLabels = new int[count];
                for (int i = 0; i < count; i++)
                {
                    Console.Write("...");
                    var faceImage = new Image<Gray, Byte>(new Bitmap(Faces + @"\face" + i + ".jpg"));
                    faceImages[i] = faceImage.Resize(100, 100, Emgu.CV.CvEnum.Inter.Linear);
                }

                f.Train(faceImages, faceLabels);
                f.Save(ImageData);
            }
            else
            {
                Console.WriteLine("NO Face to match with");
                return false;
            }
            return true;
        }



        static public bool RecognizeUser(Image<Bgr, byte> userImage)
        {
            try
            {
                f.Load(ImageData);
                using (var imageFrame = userImage)
                {

                    if (imageFrame != null)
                    {
                        var grayframe = imageFrame.Convert<Gray, byte>();

                        var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty); //the actual face detection happens here
                        if (faces.Length > 0)
                        {
                            Console.WriteLine("\nFACE IS FOUND IN SAMPLE IMAGE");
                            Bitmap inputbmp = grayframe.ToBitmap(); ;
                            Bitmap Exface;
                            Graphics canvas;
                            foreach (var face in faces)
                            {
                                Exface = new Bitmap(face.Width, face.Height);
                                canvas = Graphics.FromImage(Exface);
                                canvas.DrawImage(inputbmp, 0, 0, face, GraphicsUnit.Pixel);

                                var result = f.Predict(new Image<Gray, Byte>(Exface).Resize(100, 100, Inter.Linear));

                                //FaceRecognizer model = new EigenFaceRecognizer();
                                //model.Predict(new Image<Gray, Byte>(Exface).Resize(100, 100, Inter.Linear))
                                if (result.Distance < 2500)
                                {
                                    Console.WriteLine(result.Distance);
                                    return true;
                                }
                                Console.WriteLine(result.Distance);
                                return false;
                            }
                        }
                        Console.WriteLine("ERROR! NO Face Found on ID");
                        return false;
                    }
                }
                Console.WriteLine("ID Image is Invalid");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex thrown");
                Console.Read();
                return false;
            }

        }


        static string Process(string Path)
        {

            // Create a directory
            DirectoryInfo InDir = Directory.CreateDirectory(Images);
            try
            {
                using (Process p = new Process())
                {
                    p.StartInfo.FileName = "ffmpeg.exe";
                    p.StartInfo.Arguments = "-i " + Video + " -r 3 " + "\"" + InDir.FullName + "\"" + @"\img%3d.jpg";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    p.WaitForExit();
                }
                return InDir.FullName;
            }
            catch (Exception p1)
            {
                Console.WriteLine(p1.Message);
                Console.Read();
                return "Error";
            }
        }


        ///////////
        static string Detection()
        {

            DirectoryInfo imageDir = new DirectoryInfo(Images);
            DirectoryInfo FaceDir = Directory.CreateDirectory(Faces);
            int i = 0;
            foreach (FileInfo image in imageDir.GetFiles("*.jpg"))
            {
                using (var imageFrame = new Image<Bgr, Byte>(image.FullName))
                {
                    if (imageFrame != null)
                    {
                        //                      var grayframe = imageFrame.Convert<Gray, Byte>();
                        var grayframe = imageFrame.Convert<Gray, byte>();

                        var faces = _cascadeClassifier.DetectMultiScale(grayframe, 3.5, 10, Size.Empty); //the actual face detection happens here
                        Console.Write("...");

                        if (faces.Length > 0)
                        {
                            Bitmap inputbmp = grayframe.ToBitmap(); ;
                            Bitmap Exface;
                            Graphics canvas;


                            foreach (var face in faces)
                            {

                                Exface = new Bitmap(face.Width, face.Height);
                                canvas = Graphics.FromImage(Exface);
                                canvas.DrawImage(inputbmp, 0, 0, face, GraphicsUnit.Pixel);
                                Exface.Save(FaceDir.FullName + @"\face" + i + ".jpg");//face pattern face(i).jpg
                                i++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine(".");
            Console.WriteLine("Done!");
            return FaceDir.FullName;
        }


        ////////////

    }
}