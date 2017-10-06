using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Accord.Video;
using Accord.Video.DirectShow;

namespace WpfApp1
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VideoCaptureDevice _videoDevice;

        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var maxVideoWidth = 0;
            var maxSnapWidth = 0;
            VideoCapabilities bestSnapshotCapabilities = null;
            FilterInfo bestDevice = null;
            VideoCaptureDevice bestVideoCaptureDevice = null;
            var bestDeviceSupportsSnapshots = true;

            // Find best device based on best snapshot resolution.
            foreach (FilterInfo device in new FilterInfoCollection(FilterCategory.VideoInputDevice))
            {
                var videoCaptureDevice = new VideoCaptureDevice(device.MonikerString);
                bestDeviceSupportsSnapshots = bestDeviceSupportsSnapshots &&
                                              videoCaptureDevice.SnapshotCapabilities.Length > 0;
                foreach (var capability in videoCaptureDevice.SnapshotCapabilities)
                {
                    // Just base this on width for now.
                    if (maxSnapWidth >= capability.FrameSize.Width) continue;
                    bestVideoCaptureDevice = videoCaptureDevice;
                    maxSnapWidth = capability.FrameSize.Width;
                    bestSnapshotCapabilities = capability;
                    bestDevice = device;
                }
            }

            if (bestDevice == null)
                foreach (FilterInfo device in new FilterInfoCollection(FilterCategory.VideoInputDevice))
                {
                    var videoCaptureDevice = new VideoCaptureDevice(device.MonikerString);
                    foreach (var capability in videoCaptureDevice.VideoCapabilities)
                    {
                        // Just base this on width for now.
                        if (maxVideoWidth >= capability.FrameSize.Width) continue;
                        bestVideoCaptureDevice = videoCaptureDevice;
                        bestDevice = device;
                    }
                }

            if (bestDevice == null) return;

            _videoDevice = bestVideoCaptureDevice;
            _videoDevice.VideoResolution =
                bestVideoCaptureDevice.VideoCapabilities
                    .First(); // Seems to just select the lowest resolution which is fine for video
            _videoDevice.ProvideSnapshots = bestDeviceSupportsSnapshots;
            _videoDevice.SnapshotResolution = bestSnapshotCapabilities;
            _videoDevice.SnapshotFrame += VideoDeviceOnSnapshotFrame;
            _videoDevice.NewFrame += NewFrame;
            _videoDevice.Start();

            Resolution.Content = $"Video Resolution: {_videoDevice.VideoResolution}";
            SupportsResolution.Content = $"Snapshots Enabled: {_videoDevice.ProvideSnapshots}";
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bitmap = (Bitmap) eventArgs.Frame.Clone();
            UpdateCurrentImageWithBitmap(bitmap);
        }

        private void VideoDeviceOnSnapshotFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bitmap = (Bitmap) eventArgs.Frame.Clone();
            UpdateCurrentImageWithBitmap(bitmap);
            _videoDevice.SignalToStop();
        }

        private void UpdateCurrentImageWithBitmap(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Bmp);
                ms.Close();
                var bytes = ms.ToArray();
                Dispatcher.Invoke(() => CurrentImage.Source =
                    (ImageSource) new ImageSourceConverter().ConvertFrom(bytes));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_videoDevice == null) return;
            if (_videoDevice.IsRunning)
            {
                if (_videoDevice.ProvideSnapshots) _videoDevice.SimulateTrigger();
                else _videoDevice.SignalToStop();
            }
            else
            {
                _videoDevice.Start();
            }
        }
    }
}