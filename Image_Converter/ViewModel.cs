using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Image_Converter
{
    internal class ViewModel : INotifyPropertyChanged
    {
        private const string DEFAULT_DEST_DIRECTORY = "pictures";

        private string sourcePath;
        public string SourcePath
        {
            get { return sourcePath; }
            set { sourcePath = value; FirePropertyChanged("SourcePath"); }
        }


        private string destinationPath;
        public string DestinationPath
        {
            get { return destinationPath; }
            set { destinationPath = value; FirePropertyChanged("DestinationPath"); }
        }

        private string[] listAllPictures;
        public string[] ListAllPictures
        {
            get { return listAllPictures; }
            set { listAllPictures = value; FirePropertyChanged("ListAllPictures"); }
        }

        private string pictureSelected;
        public string PictureSelected
        {
            get { return pictureSelected; }
            set
            {
                pictureSelected = value;
                DisplayedImage = value;
                FirePropertyChanged("PictureSelected");
            }
        }

        private string displayedImage;
        public string DisplayedImage
        {
            get { return displayedImage; }
            set { displayedImage = value; FirePropertyChanged("DisplayedImage"); }
        }

        //public WriteableBitmap ConvertedPicture { get; set; }


        private FrameType enmFrameType;
        public FrameType EnmFrameType
        {
            get { return enmFrameType; }
            set { enmFrameType = value; FirePropertyChanged("EnmFrameType"); }
        }

        private int progressBarValue;
        public int ProgressBarValue
        {
            get { return progressBarValue; }
            set { progressBarValue = value; FirePropertyChanged("ProgressBarValue"); }
        }

        private int progressBarMaxValue;
        public int ProgressBarMaxValue
        {
            get { return progressBarMaxValue; }
            set { progressBarMaxValue = value; FirePropertyChanged("ProgressBarMaxValue"); }
        }


        #region Constructor
        public ViewModel()
        {
            OpenSourcePath = new RelayCommand(btnOpenSourcePath_Click);
            OpenDestinationPath = new RelayCommand(btnOpenDestinationPath_Click);
            ConvertPictures = new RelayCommand(btnConvertPictures_Click);

            SourcePath = Directory.GetCurrentDirectory();
            DestinationPath = Directory.GetCurrentDirectory() + "\\" + DEFAULT_DEST_DIRECTORY + "\\";
            findAllPicturesIn(SourcePath);
            ProgressBarValue = 0;
            ProgressBarMaxValue = 100;
        }

        #endregion

        #region Methods
        private String openDirectory()
        {
            String dirSelected = "";
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    dirSelected = fbd.SelectedPath;
            }
            return dirSelected;
        }
        private void findAllPicturesIn(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] files = directory.GetFiles();

            ListAllPictures = files.Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden) && 
                                            (f.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                                             f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || 
                                             f.Extension.Equals(".png", StringComparison.OrdinalIgnoreCase)))
                                            .Select(f => f.FullName).ToArray();
            
        }

        #endregion

        #region Command
        public RelayCommand OpenSourcePath { get; set; }
        private void btnOpenSourcePath_Click(object obj)
        {
            var s = openDirectory();
            if (s != "")
            {
                SourcePath = s;
                findAllPicturesIn(SourcePath);
            }
        }
        public RelayCommand OpenDestinationPath { get; set; }
        private void btnOpenDestinationPath_Click(object obj)
        {
            var s = openDirectory();
            if (s != "")
            {
                DestinationPath = s + "\\" + DEFAULT_DEST_DIRECTORY + "\\";
            }
        }
        public RelayCommand ConvertPictures { get; set; }
        private void btnConvertPictures_Click(object obj)
        {
            // Check if directory exist
            if (!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);

            new Thread(convertAllPictures).Start();
        }

        private void convertAllPictures()
        {
            SixLabors.ImageSharp.Size size = new SixLabors.ImageSharp.Size(0, 0);
            switch (enmFrameType)
            {
                case FrameType.Frame_1200x820:
                    size = new SixLabors.ImageSharp.Size(1200, 820);
                    break;
                case FrameType.Frame_600x488:
                    size = new SixLabors.ImageSharp.Size(600, 488);
                    break;
            }
            // Define the resizing options
            ResizeOptions options = new ResizeOptions()
            {
                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Crop,
                PadColor = SixLabors.ImageSharp.Color.Red,
                Size = size
            };
            ProgressBarValue = 0;
            ProgressBarMaxValue = listAllPictures.Length;
            foreach (var picPath in listAllPictures)
            {
                if (!File.Exists(picPath))
                    continue;
                try
                {
                    using (Image image = Image.Load(picPath))
                    {
                        image.Mutate(x => x.Resize(options));

                        image.Save(destinationPath + Path.GetFileName(picPath));
                    }
                    ProgressBarValue++;
                }
                catch (Exception)
                {
                }
            }
        }

        //private void setConvertedPicture(SixLabors.ImageSharp.Image image)
        //{
        //    var bmp = new WriteableBitmap(image.Width, image.Height, image.Metadata.HorizontalResolution, image.Metadata.VerticalResolution, PixelFormats.Bgra32, null);
            
        //    bmp.Lock();
        //    image.ProcessPixelRows(static pixelAccessor =>
        //    {
        //        for (int y = 0; y < pixelAccessor.Height; y++)
        //        {
        //            Span<Rgb24> row = pixelAccessor.GetRowSpan(y);

        //            // Using row.Length helps JIT to eliminate bounds checks when accessing row[x].
        //            for (int x = 0; x < row.Length; x++)
        //            {
        //                row[x] = new Rgb24((byte)x, (byte)y, 0);
        //            }
        //        }
        //    });
        //    try
        //    {
        //        IntPtr backBuffer = bmp.BackBuffer;
        //        SixLabors.ImageSharp.Image.
        //        for (var y = 0; y < image.Height; y++)
        //        {
                    
        //            var buffer = image.GetPixelRowSpan(y);
        //            for (var x = 0; x < image.Width; x++)
        //            {
        //                var backBufferPos = backBuffer + (y * image.Width + x) * 4;
        //                var rgba = buffer[x];
        //                var color = rgba.A << 24 | rgba.R << 16 | rgba.G << 8 | rgba.B;

        //                System.Runtime.InteropServices.Marshal.WriteInt32(backBufferPos, color);
        //            }
        //        }

        //        bmp.AddDirtyRect(new Int32Rect(0, 0, image.Width, image.Height));
        //    }
        //    finally
        //    {
        //        bmp.Unlock();
        //        ConvertedPicture = bmp;
        //    }
        //}

        #endregion


        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;
        // Check the attribute in the following line :
        public void FirePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
