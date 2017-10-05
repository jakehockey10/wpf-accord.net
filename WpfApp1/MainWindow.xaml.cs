using System.Drawing;
using System.IO;
using System.Windows;
using Accord.Video;
using Accord.Video.DirectShow;

namespace WpfApp1
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private FilterInfoCollection m_videoDevices;
    private VideoCaptureDevice m_videoDevice;
    private VideoCapabilities[] m_videoCapabilities;
    private VideoCapabilities[] m_snapshotCapabilities;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var maxVideoWidth = 0;
      var maxSnapWidth = 0;
      VideoCapabilities bestVideoCapabilities = null;
      VideoCapabilities secondBestVideoCapabilities = null;
      VideoCapabilities bestSnapshotCapabilities = null;
      FilterInfo bestDevice = null;
      VideoCaptureDevice bestVideoCaptureDevice = null;

      // Find best device based on best snapshot resolution.
      foreach (FilterInfo device in new FilterInfoCollection(FilterCategory.VideoInputDevice))
      {
        var videoCaptureDevice = new VideoCaptureDevice(device.MonikerString);
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

      if (bestDevice == null) return;

      foreach (var capability in bestVideoCaptureDevice.VideoCapabilities)
      {
        // Just base this on width for now.
        if (maxVideoWidth >= capability.FrameSize.Width) continue;
        maxVideoWidth = capability.FrameSize.Width;
        secondBestVideoCapabilities = bestVideoCapabilities;
        bestVideoCapabilities = capability;
      }
      
      m_videoDevice = bestVideoCaptureDevice;
      // TODO: Rear-facing camera on Panasonic FZ-G1 won't work if video/snapshot resolutions are the same.  Why?
      // https://github.com/accord-net/framework/issues/881
      m_videoDevice.VideoResolution = secondBestVideoCapabilities;
      m_videoDevice.ProvideSnapshots = true;
      m_videoDevice.SnapshotResolution = bestSnapshotCapabilities;
      m_videoDevice.SnapshotFrame += VideoDeviceOnSnapshotFrame;
      
      videoSourcePlayer.VideoSource = m_videoDevice;
      videoSourcePlayer.Start();
    }

    private void VideoDeviceOnSnapshotFrame(object sender, NewFrameEventArgs eventArgs)
    {
      ShowSnapshot((Bitmap) eventArgs.Frame.Clone());
    }

    private void ShowSnapshot(Bitmap snapshot)
    {
      using (var ms = new MemoryStream())
      {
        snapshot.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //snapshot.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
      }
      
      //Dispatcher.Invoke(() =>
      //{
      //  // TODO: Show picture?
      //});
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      if (m_videoDevice != null && m_videoDevice.ProvideSnapshots)
      {
        m_videoDevice.SimulateTrigger();
      }
    }
  }
}
