    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    namespace SysProjekat
    {
        public class ConvertClass
        {
            public static async Task<string> ConvertToGifAsync(string imagePath, int index)
            {
                using var image = Image.Load<Rgba32>(imagePath);
                
                int width = image.Width;
                int height = image.Height;

                using var gif = new Image<Rgba32>(width, height);

                object lockObject = new object();

                List<Task> tasks = new List<Task>();

                for (int i = 0; i < 6; i++)
                {
                    int indexCopy = i;
                    tasks.Add(Task.Run(() =>
                    {
                        EditImage(image.Clone(), indexCopy, gif.Frames, lockObject);
                    }));
                }

                await Task.WhenAll(tasks);

                string gifpath = $"../../../GifFile{index}.gif";
                gif.Save(gifpath);
            
                return gifpath;
            }
            public static void EditImage(Image<Rgba32> image, int index, ImageFrameCollection<Rgba32> gifFrames, object lockObject)
            {
                byte red = 0, green = 0, blue = 0;

                switch (index)
                {
                    case 0:
                        red = 0;
                        green = 50;
                        blue = 0;
                        break;
                    case 1:
                        red = 50;
                        green = 0;
                        blue = 0;
                        break;
                    case 2:
                        red = 0;
                        green = 0;
                        blue = 50;
                        break;
                    case 3:
                        red = 50;
                        green = 50;
                        blue = 0;
                        break;
                    case 4:
                        red = 0;
                        green = 50;
                        blue = 50;
                        break;
                    case 5:
                        red = 50;
                        green = 0;
                        blue = 50;
                        break;
                }

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Rgba32 pixel = image[x, y];
                        if (red != 0)
                            pixel.R = red;
                        if (green != 0)
                            pixel.G = green;
                        if (blue != 0)
                            pixel.B = blue;
                        image[x, y] = pixel;
                    }
                }

                lock (gifFrames)
                {
                    gifFrames.AddFrame(image.Frames.RootFrame);
                }
            }
        }
    }