using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Diagnostics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

using Cudafy;
using Cudafy.Translator;
using Cudafy.Host;
using Image = SixLabors.ImageSharp.Image;

namespace GenerativeImages
{
    internal class ImageGenerator
    {
        public List<List<string>> imagesPaths;
        public List<string> imageNames;
        public Random random;
        public string CurrentDir;
        int imageName = 0;

        public ImageGenerator()
        {
            imagesPaths = new List<List<string>>();
            imageNames = new List<string>();
            random = new Random();
            CurrentDir = Directory.GetCurrentDirectory();
        }

        // Usually generating images with ImageSharp is 5 times faster then System.Drawing.Bitmap

        static async Task Main()
        {
            var GenIm = new ImageGenerator();

            GenIm.GenerateImagesPaths(100);
            await GenIm.GenerateImages();

            GenIm.ArciveImages();

        }
        
        private void GenerateImagesPaths(int amount)
        {
            Console.Write("Generating Paths: ");

            int imagesCount = 0;
            int maxImages = 1;

            foreach (var d in Directory.EnumerateDirectories(CurrentDir + "\\layers"))
            {
                foreach (var f in Directory.EnumerateFiles(d))
                {
                    imagesCount++;
                }
                if (imagesCount != 0)
                {
                    maxImages *= imagesCount;
                    imagesCount = 0;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed");
                    Console.ResetColor();
                    throw new Exception("Please remove empty folders");
                }
            }

            if (amount > maxImages)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed");
                Console.ResetColor();

                throw new Exception($"It is unable to generate this much images. Max amount is {maxImages}");
            }

            do
            {
                List<string> imagePath = new List<string>();
                StringBuilder imageName = new StringBuilder();

                foreach (var d in Directory.EnumerateDirectories(Directory.GetCurrentDirectory() + "\\layers"))
                {
                    var files = Directory.EnumerateFiles(d);
                    var list = files.ToList();
                    var index = random.Next(0, list.Count);

                    imageName.Append(list[index]);
                    imagePath.Add(list[index]);


                }

                if (!imageNames.Contains(imageName.ToString()))
                {
                    imagesPaths.Add(imagePath);
                }



            } while (amount > imagesPaths.Count);


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ResetColor();

        }

        private async Task GenerateImages()
        {
            Console.Write("Generating images: ");

            List<Task> tasks = new List<Task>();

            foreach (var l in imagesPaths)
            {
                imageName++;
                //tasks.Add(GenerateImage(l, imageName));
                tasks.Add(ImageSharpGenerator(l, imageName));
            }

            Task.WaitAll(tasks.ToArray());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ResetColor();
        }
    
        private async Task BitmapImageGenerator(List<string> l, int imageName)
        {
            await Task.Run(() =>
            {
                Bitmap bmp = new Bitmap(l[0]);

                foreach (var s in l)
                {
                    var part = new Bitmap(s);

                    for (int x = 0; x < part.Width; x++)
                    {
                        for (int y = 0; y < part.Height; y++)
                        {
                            Color color = part.GetPixel(x, y);
                            if (color.A != 0)
                            {
                                bmp.SetPixel(x, y, color);
                            }
                        }
                    }

                }
                
                bmp.Save($"{CurrentDir}\\Output\\{imageName}.png");

            });
        }

        private async Task ImageSharpGenerator(List<string> l,int imageName)
        {
            await Task.Run(() =>
            {
                SixLabors.ImageSharp.Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(l[0]);
                foreach (var s in l)
                {
                    var part = SixLabors.ImageSharp.Image.Load<Rgba32>(s);

                    for (int x = 0; x < part.Width; x++)
                    {
                        for (int y = 0; y < part.Height; y++)
                        {
                            Rgba32 color = part[x, y];
                            if (color.A != 0)
                            {
                                image[x, y] = color;
                            }
                        }
                    }

                }

                image.SaveAsPng($"{CurrentDir}\\Output\\{imageName}.png");

            });
        }

        private void ArciveImages()
        {
            int prefix = 0;

            do
            {
                try
                {
                    ZipFile.CreateFromDirectory(CurrentDir + "\\Output", $"Output{prefix}.zip", CompressionLevel.Optimal, false);
                    break;
                }
                catch
                {
                    prefix++;
                }
            } while (true);
        }


        // To be continued ...
        private void CUDAfyTest(List<string> l, int imageName)
        {

            CudafyModule km = CudafyTranslator.Cudafy();

            GPGPU gpu = CudafyHost.GetDevice(CudafyModes.Target,CudafyModes.DeviceId);

            gpu.LoadModule(km);


            Image<Rgba32> image = Image.Load<Rgba32>(l[0]);



            foreach (var s in l)
            {
                var part = SixLabors.ImageSharp.Image.Load<Rgba32>(s);

                gpu.Allocate<Rgba32>(part.Width, part.Height);

                Rgba32[,] colorArray = new Rgba32[part.Width, part.Height];
                
                //for(int i = 0; i < part.Width; i++)
                //{
                //    colorArray[i]=new Rgba32[part.Height];
                //}

                
                for(int i = 0; i < part.Width; i++)
                {
                    for(int j = 0; j < part.Height; j++)
                    {
                        colorArray[i,j] = part[i, j];
                    }
                }
                
                Rgba32[,] devColor = gpu.Allocate<Rgba32>(part.Width, part.Height);

                gpu.CopyToDevice(colorArray, devColor);

                

                for (int x = 0; x < part.Width; x++)
                {
                    for (int y = 0; y < part.Height; y++)
                    {
                        Rgba32 color = part[x, y];
                        if (color.A != 0)
                        {
                            image[x, y] = color;
                        }
                    }
                }

            }

            //gpu.Allocate<SixLabors.ImageSharp.Image>()





        }



    }



}
