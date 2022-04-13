﻿using System;
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

namespace GenerativeImages
{
    internal class Program
    {
        public List<List<string>> imagesPaths;
        public List<string> imageNames;
        public Random random;
        public string CurrentDir;
        private SemaphoreSlim semaphore;
        int imageName = 0;

        public Program()
        {
            imagesPaths = new List<List<string>>();
            imageNames = new List<string>();
            random = new Random();
            CurrentDir = Directory.GetCurrentDirectory();
            semaphore = new SemaphoreSlim(1);
        }

        static async Task Main()
        {
            var GenIm = new Program();

            Stopwatch stopwatch = Stopwatch.StartNew();

            GenIm.GenerateImagesPaths(100);
            await GenIm.GenerateImages();
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}");

            //GenIm.ArciveImages();

            stopwatch.Stop();

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
                tasks.Add(ImageSharpTest(l, imageName));
            }

            Task.WaitAll(tasks.ToArray());

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ResetColor();
        }
    
        private async Task ImageSharpTest(List<string> l,int imageName)
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

        private async Task GenerateImage(List<string> l, int imageName)
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
    }



}
