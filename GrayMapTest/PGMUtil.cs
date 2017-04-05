using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GrayMapTest
{
    public enum PGMType { P2, P5, UNKNOWN }

    public class UnknowPGMTypeException : Exception
    {
    }

    public class PGMInformation
    {
        private PGMType type = PGMType.UNKNOWN;

        public PGMType Type
        {
            get { return type; }
            set { type = value; }
        }

        public uint Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
                pixelCount = Convert.ToInt32(width * height);
            }
        }

        public uint Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
                pixelCount = Convert.ToInt32(width * height);
            }
        }

        public int PixelCount
        {
            get
            {
                return pixelCount;
            }

            set
            {
                pixelCount = value;
            }
        }

        public byte GaryMax
        {
            get
            {
                return garyMax;
            }

            set
            {
                garyMax = value;
            }
        }

        private UInt32 width;

        private UInt32 height;

        private int pixelCount;

        private byte garyMax;

        private byte[] pgmByteData = null;

        public byte[] PgmByteData
        {
            get { return pgmByteData; }
            set { pgmByteData = value; }
        }

        private string[,] pgmStringData = null;

        public string[,] PgmStringData
        {
            get { return pgmStringData; }
            set { pgmStringData = value; }
        }

        static private PGMInformation pgmInformation;

        static public PGMInformation PGMinformationInstance
        {
            get
            {
                if (pgmInformation == null)
                    pgmInformation = new PGMInformation();

                return pgmInformation;
            }
        }

        public SoftwareBitmapSource BitmapImageSource
        {
            get
            {
                return bitmapImageSource;
            }

            set
            {
                bitmapImageSource = value;
            }
        }

        private SoftwareBitmapSource bitmapImageSource;

        public async void ToWriteableBitmap()
        {
            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Gray8, (int)width, (int)height);
            softwareBitmap.CopyFromBuffer(pgmByteData.AsBuffer());

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
      softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            bitmapImageSource = new SoftwareBitmapSource();
            await bitmapImageSource.SetBitmapAsync(softwareBitmap);

            //Save softwareBitmap to file

            //using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    // Create an encoder with the desired format
            //    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

            //    // Set the software bitmap
            //    encoder.SetSoftwareBitmap(softwareBitmap);

            //    await encoder.FlushAsync();
            //}
        }

        //public async Task ConvertToGrayScale()
        //{
        //    WriteableBitmap writeableBitmap = new WriteableBitmap((int)width, (int)height);
        //    using (Stream stream = writeableBitmap.PixelBuffer.AsStream())
        //    {
        //        await stream.WriteAsync(pgmByteData, 0, pixelCount);
        //    }
        //    bitmapImageSource = writeableBitmap;

        //    byte[] srcPixels = new byte[4 * bitmapImageSource.PixelWidth * bitmapImageSource.PixelHeight];

        //    using (Stream pixelStream = bitmapImageSource.PixelBuffer.AsStream())
        //    {
        //        await pixelStream.ReadAsync(srcPixels, 0, srcPixels.Length);
        //    }

        //    WriteableBitmap dstBitmap =
        //            new WriteableBitmap(bitmapImageSource.PixelWidth, bitmapImageSource.PixelHeight);
        //    byte[] dstPixels = new byte[4 * dstBitmap.PixelWidth * dstBitmap.PixelHeight];

        //    for (int i = 0; i < srcPixels.Length; i += 4)
        //    {
        //        double b = (double)srcPixels[i] / 255.0;
        //        double g = (double)srcPixels[i + 1] / 255.0;
        //        double r = (double)srcPixels[i + 2] / 255.0;

        //        byte a = srcPixels[i + 3];

        //        double e = (0.21 * r + 0.71 * g + 0.07 * b) * 255;
        //        byte f = Convert.ToByte(e);

        //        dstPixels[i] = f;
        //        dstPixels[i + 1] = f;
        //        dstPixels[i + 2] = f;
        //        dstPixels[i + 3] = a;
        //    }
        //    using (Stream pixelStream = dstBitmap.PixelBuffer.AsStream())
        //    {
        //        await pixelStream.WriteAsync(dstPixels, 0, dstPixels.Length);
        //    }
        //    dstBitmap.Invalidate();

        //    bitmapImageSource = dstBitmap;
        //}
    }

    public class PgmFileUtil
    {
        public PgmFileUtil()
        {
            data = PGMInformation.PGMinformationInstance;
        }

        private Stream fileStream = null;
        private BinaryReader binaryReader = null;
        private StreamReader stringReader = null;

        private BinaryWriter binaryWriter = null;
        private StreamWriter stringWriter = null;

        private PGMInformation data;

        public PGMInformation Data
        {
            get { return data; }
            set { data = value; }
        }

        static private PgmFileUtil instance = null;

        static public PgmFileUtil Instance
        {
            get
            {
                if (instance == null)
                    instance = new PgmFileUtil();
                return instance;
            }
        }

        public async void LoadPgmFile(StreamReader stringReader)
        {
            fileStream = stringReader.BaseStream;
            string typeHead = stringReader.ReadLine();
            string sizeHead = stringReader.ReadLine();
            string grayMax = stringReader.ReadLine();
            if (typeHead[1] == '5')
            {
                data.Type = PGMType.P5;
            }
            else if (typeHead[1] == '2')
            {
                data.Type = PGMType.P5;
            }
            else
            {
                data.Type = PGMType.UNKNOWN;
                throw new UnknowPGMTypeException();
            }
            string[] tokens = sizeHead.Split(' ');
            data.Width = UInt32.Parse(tokens[0]);
            data.Height = UInt32.Parse(tokens[1]);

            data.GaryMax = Convert.ToByte(grayMax);

            if (data.Type == PGMType.P5)
            {
                binaryReader = new BinaryReader(fileStream, Encoding.ASCII);

                data.PgmByteData = new byte[data.PixelCount];

                binaryReader.Read(data.PgmByteData, 0, data.PixelCount);

                data.ToWriteableBitmap();
                stringReader.Dispose();
                binaryReader.Dispose();
            }
            if (data.Type == PGMType.P2)
            {
                data.PgmStringData = new string[data.Height, data.Width];
                data.PgmByteData = new byte[data.PixelCount];
                char[] temp0, temp;
                int lineLength = 0;

                for (int row = 0; row < data.Height; row++)
                {
                    for (int col = 0; col < data.Width; col++)
                    {
                        if (row == 3 && col == 0x12)
                            col = col;
                        temp0 = new char[4];
                        stringReader.Read(temp0, 0, 4);
                        lineLength += 4;

                        int posit = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (temp0[i] == ' ')
                            {
                                posit = i;
                                break;
                            }
                        }

                        temp = new char[posit];
                        for (int i = 0; i < posit; i++)
                        {
                            temp[i] = temp0[i];
                        }

                        data.PgmStringData[row, col] = new String(temp);
                        data.PgmByteData[row * data.Width + col] =
                            Convert.ToByte(data.PgmStringData[row, col]);

                        if (lineLength >= 70)
                        {
                            stringReader.Read();
                            lineLength = 0;
                            if (col == (data.Width - 1))
                                continue;
                        }
                        if (col == (data.Width - 1))
                        {
                            stringReader.Read();
                            lineLength = 0;
                        }
                    }
                }
            }
            fileStream.Dispose();
        }
    }
}