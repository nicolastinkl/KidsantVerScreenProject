//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BackgroundRemovalBasics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows; 
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.BackgroundRemoval;
    using SoundUtil;
    using System.Net;
    using HttpEngen;
    using System.Collections.Generic;
    using System.Text;
    using System.Configuration;
    using System.Windows.Threading;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using Microsoft.Samples.Kinect.BackgroundRemovalBasics.model;
    using Newtonsoft.Json.Linq;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using WpfApplication22;
    using System.Threading;
    using KinectBasicHandTrackingFramework; 

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        /// <summary>
        /// Cache Directory
        /// </summary>
        static string imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"] + @"\";

        /// <summary>
        /// Format we will use for the depth stream
        /// </summary>
        private const DepthImageFormat DepthFormat = DepthImageFormat.Resolution320x240Fps30;

        /// <summary>
        /// Format we will use for the color stream
        /// </summary>
        private const ColorImageFormat ColorFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap foregroundBitmap;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensorChooser sensorChooser;

        /// <summary>
        /// Our core library which does background 
        /// </summary>
        private BackgroundRemovedColorStream backgroundRemovedColorStream;

        /// <summary>
        /// Intermediate storage for the skeleton data received from the sensor
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        /// the skeleton that is currently tracked by the app
        /// </summary>
        private int currentlyTrackedSkeletonId;

        /// <summary>
        /// WaveGesture   Wave gesTure sure
        /// </summary>
        public WaveGesture _WaveGesture;

        /// <summary>
        ///  SendWaveTime
        /// </summary>
        //private DispatcherTimer SendWaveDisTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.8) };

        /// <summary>
        /// remove
        /// </summary>
        //private DispatcherTimer RemoveSendWaveDisTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        /// <summary>
        /// Success ID
        /// </summary>
        private int currentWaveID;
        /// <summary>
        /// INT32
        /// </summary>
        private Int32 mID = -1;

        private Dictionary<string, string> HasCheckDic;
        /// <summary>
        /// 
        /// </summary>
        private readonly DispatcherTimer disengagementTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1)};

        /// <summary>
        ///  Back ground worker to request network with add credit
        /// </summary>
        private BackgroundWorker _backgroundWorker = new BackgroundWorker();
        /// <summary>
        /// Boolean determining whether any user is currently engaged.
        /// </summary>
        private bool isUserEngaged;


        /// <summary>
        /// 
        /// </summary>
        private readonly DispatcherTimer ShowPauseAnimationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        /// <summary>
        ///  send wave code
        /// </summary>
        private readonly DispatcherTimer SendDispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };

        /// <summary>
        ///  请求手势签到动作
        /// </summary>
        private readonly DispatcherTimer WaveHandDispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        
        /// <summary>
        /// 当前窗口计时器
        /// </summary>
        private int CurrentTimerCount;
        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed;

        /// <summary>
        /// tracking id
        /// </summary>
        private int trackingId;
        private int InvalidTrackingId;
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            // initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.KinectChanged += this.SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
            
            this.disengagementTimer.Tick += disengagementTimer_Tick; 
            this.disengagementTimer.Start();

            this.SendDispatcherTimer.Tick += SendDispatcherTimer_Tick;
            CurrentTimerCount = 0;
//#mark  this is test
            isUserEngaged = true;
            currentWaveID = 0;

            this._backgroundWorker.DoWork += _backgroundWorker_DoWork;

            this._backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;


            //this.SendWaveDisTimer.Tick += SendWaveDisTimer_Tick;
            
            //this.RemoveSendWaveDisTimer.Tick += RemoveSendWaveDisTimer_Tick;
            //this.RemoveSendWaveDisTimer.Start();

            ShowPauseAnimationTimer.Tick += ShowPauseAnimationTimer_Tick;

            this._WaveGesture = new WaveGesture();
            this._WaveGesture.GestureDetected += new EventHandler(_WaveGesture_GestureDetected);

            this.WaveHandDispatcherTimer.Tick += WaveHandDispatcherTimer_Tick;

            HasCheckDic = new Dictionary<string, string>();
            RandomADImageView();
          
        }

        void WaveHandDispatcherTimer_Tick(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            //5秒之后处理再次请求然后关闭当前dispatchertimer
            // request networking from internet
            CheckinWithKinect();
            this.WaveHandDispatcherTimer.Stop();
        }

        void ShowPauseAnimationTimer_Tick(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_showGIf"]).Pause();
            ShowPauseAnimationTimer.Stop();
        }

        private void _WaveGesture_GestureDetected(object sender, EventArgs e)
        {
            //helpServiceTextOrImg.SendMessage("欢迎 您已经招手了");
            Console.WriteLine("欢迎 您已经招手了");
            if (!this.WaveHandDispatcherTimer.IsEnabled)
            {
                this.WaveHandDispatcherTimer.Start();
                //开始计时 然后处理第一次请求 让数据库记录我当前请求时间刻度
                //Disk Usage with fast image cache  @twitter
                CheckinWithKinect();

            }
        }

        private void CheckinWithKinect()
        {
            string json = HttpWebResponseUtility.GetModel("http://app.kidswant.com.cn/bigscreen/screen_handshake?sceneid=1&screenid=1");
            if (json != null)
            {
                json.Replace("\"", "");
            }
            Console.WriteLine(json);

            if (json.Length > 1)
            {
                ResponseEntity responseCode = JsonConvert.DeserializeObject<ResponseEntity>(json);
                if (responseCode.ResponseCode == 1)
                {
                    CheckInUserMain usermain = JsonConvert.DeserializeObject<CheckInUserMain>(json);
                    Dispatcher.Invoke((Action)delegate
                    {
                        try
                        {
                            string url = string.Format("{0}", usermain.User.Avatar);
                            if (url != "" && url.Length > 5)
                            {
                                string filename = this.DownloadImageWithUrl(url, (int)usermain.User.Id);
                                BitmapImage src = new BitmapImage();
                                src.BeginInit();
                                src.UriSource = new Uri(imageDirectory + filename);
                                src.CacheOption = BitmapCacheOption.OnLoad;
                                src.EndInit();
                                //new Uri(url);//
                                this.PanelUserImage.ImageSource = src;
                            }
                            else
                            {
                                BitmapImage src = new BitmapImage();
                                src.BeginInit();
                                src.UriSource = new Uri("pack://application:,,,/Images/avatar_fault.png");
                                src.CacheOption = BitmapCacheOption.OnLoad;
                                src.EndInit();
                                this.PanelUserImage.ImageSource = src;

                            }

                            int userid = (int)usermain.User.Id;
                            string value = "" + userid;
                            ///如果有识别到该用户就处理重复签到逻辑
                            if (this.HasCheckDic.ContainsKey(value))
                            {
                                string valueID = this.HasCheckDic[value];
                                int valueidint = int.Parse(valueID);
                                if (valueidint == 1)
                                {
                                    //第2次签到  +1
                                    valueidint++;
                                    this.HasCheckDic[value] = "" + valueidint;
                                    this.Dispatcher.Invoke((Action)delegate
                                    {

                                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                        RandomADImageView();
                                        this.TextCheckIn.Text = "今天第二次签到";
                                        this.TextBok_addCredit.Visibility = Visibility.Visible;
                                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                                    });
                                }
                                else if (valueidint >= 2)
                                {
                                    //第3......n次签到  =0
                                    //这次不签到 

                                    ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                    this.Dispatcher.Invoke((Action)delegate
                                    {
                                        this.TextCheckIn.Text = "亲 明天再来签到吧";
                                        this.TextBok_addCredit.Visibility = Visibility.Hidden;
                                    });
                                }
                            }
                            else
                            {
                                //第一次签到
                                this.HasCheckDic[value] = "1";
                                this.Dispatcher.Invoke((Action)delegate
                                {

                                    ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                    RandomADImageView();
                                    this.TextCheckIn.Text = "今天第一次签到";
                                    this.TextBok_addCredit.Visibility = Visibility.Visible;
                                    ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                                });
                            }


                        }
                        catch (Exception)
                        {

                        }
                        this.PanelUserName.Text = string.Format("{0}", usermain.User.Name);

                    });
                }
                else
                {
                  // response 0
                   // CheckinWithKinect();
                }
            }



        }
         

        void RemoveSendWaveDisTimer_Tick(object sender, EventArgs e)
        {
            int count = gridSendWave.Children.Count;
            if (count >= 5)
            {
                //Mark  remove Index at 0
                try
                {
                    gridSendWave.Children.RemoveRange(5, count - 5);
                }
                catch (Exception)
                {
                     
                }
               
                 
            }
        }

        void SendWaveDisTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UserControl1 ctl = new UserControl1();
                ctl.Margin = new Thickness(210, 30, 0, 0);
                gridSendWave.Children.Insert(0, ctl);
                ctl.BeginStoryboard((System.Windows.Media.Animation.Storyboard)ctl.Resources["Storyboard2"]);
            }
            catch (Exception)
            {
                 
            }
           
           //  Console.WriteLine("count : " + gridSendWave.Children.Count);
        }
         

        // Worker Method

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do something
            int i = Convert.ToInt32(e.Argument);
            if (i >0 )
            {   
                if (i == 10086)
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {

                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                        this.TextCheckIn.Text = "亲 明天再来签到吧";
                    });
                }
                
                
                try
                {
                      Addcredit(i); 
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
           
        }

        // Completed Method
        void _backgroundWorker_RunWorkerCompleted(  object sender,  RunWorkerCompletedEventArgs e)
        {
          
            // Console.WriteLine("_backgroundWorker_RunWorkerCompleted");
           
        }


        void disengagementTimer_Tick(object sender, EventArgs e)
        {
            CurrentTimerCount++;
            if (CurrentTimerCount > 100)
            {
                currentWaveID = 0;
            }

            //Console.Write("  CurrentTimerCount: " + CurrentTimerCount + "  isUserEngaged:" + isUserEngaged);
            switch (CurrentTimerCount)
            {
                case 1:
                    playMp3("1"); 
                    break;
                case 20:
                   playMp3("2"); 
                   break;
                case 30:
                    playMp3("3");
                    break;
                case 60:
                    playMp3("4");
                   break;
                default:
                    break;
            }
            
        }

        public void playMp3(string strMp3Name)
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_showGIf"]).Begin();
            audioMedia.Source = new Uri(Environment.CurrentDirectory + "\\audio\\" + strMp3Name + ".wav");
            audioMedia.Position = TimeSpan.Zero;
            // audioMedia.Volume = 1.0;
            // audioMedia.Balance = 1.0;
            audioMedia.Play();
        }

        /// <summary>
        /// 根据用户签到处理随机图片推送
        /// </summary>
        public void RandomADImageView()
        {   
            //3-16  
            Random rand = new Random();
            int  currentImageIndex = rand.Next(3, 16);
            this.ADImageView.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/kidswantMember" + currentImageIndex + ".jpg"));
             

        }
    
        /// <summary>
        /// Finalizes an instance of the MainWindow class.
        /// This destructor will run only if the Dispose method does not get called.
        /// </summary>
        ~MainWindow()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Dispose the allocated frame buffers and reconstruction.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all memory associated with the FusionImageFrame.
        /// </summary>
        /// <param name="disposing">Whether the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (null != this.backgroundRemovedColorStream)
                {
                    this.backgroundRemovedColorStream.Dispose();
                    this.backgroundRemovedColorStream = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
            this.sensorChooser = null;
        }

        /// <summary>
        /// Event handler for Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            // in the middle of shutting down, or lingering events from previous sensor, do nothing here.
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect || this.sensorChooser.Kinect != sender)
            {
                return;
            }

            try
            {
                using (var depthFrame = e.OpenDepthImageFrame())
                {
                    if (null != depthFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                    }
                }

                using (var colorFrame = e.OpenColorImageFrame())
                {
                    if (null != colorFrame)
                    {
                        this.backgroundRemovedColorStream.ProcessColor(colorFrame.GetRawPixelData(), colorFrame.Timestamp);
                    }
                }

                using (var skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (null != skeletonFrame)
                    {
                        skeletonFrame.CopySkeletonDataTo(this.skeletons);
                        this.backgroundRemovedColorStream.ProcessSkeleton(this.skeletons, skeletonFrame.Timestamp);
                        // Can shibie More people
                        //bool isUserPresent = UpdateTrackedSkeletonsArray(); 
                       // Console.WriteLine("skeletonFrame.SkeletonArrayLength: " + skeletonFrame.SkeletonArrayLength);
                       
                        ///
                        /// change by tinkl or ljh
                        ///
                        this._WaveGesture.Update(this.skeletons, skeletonFrame.Timestamp);

                    }
                }

                this.ChooseSkeleton();
            }
            catch (InvalidOperationException)
            {
                // Ignore the exception. 
            }
        }
       

        /// <summary>
        /// trackid
        /// </summary>
        public int TrackingId
        {
            get
            {
                return this.trackingId;
            }

            set
            {
                if (value != this.trackingId)
                {
                    if (null != this.backgroundRemovedColorStream)
                    {
                        if (InvalidTrackingId != value)
                        {
                            this.backgroundRemovedColorStream.SetTrackedPlayer(value);
                            this.Timestamp = DateTime.UtcNow;
                        }
                        else
                        {
                            // Hide the last frame that was received for this user. 
                            this.MaskedColor.Visibility = Visibility.Hidden;
                            this.Timestamp = DateTime.MinValue;
                        }
                    }

                    this.trackingId = value;
                }
            }
        }
         
        /// <summary>
        /// Handle the background removed color frame ready event. The frame obtained from the background removed
        /// color stream is in RGBA format.
        /// </summary>
        /// <param name="sender">object that sends the event</param>
        /// <param name="e">argument of the event</param>
        private void BackgroundRemovedFrameReadyHandler(object sender, BackgroundRemovedColorFrameReadyEventArgs e)
        {
            using (var backgroundRemovedFrame = e.OpenBackgroundRemovedColorFrame())
            {
                if (backgroundRemovedFrame != null)
                {
                    if (null == this.foregroundBitmap || this.foregroundBitmap.PixelWidth != backgroundRemovedFrame.Width
                        || this.foregroundBitmap.PixelHeight != backgroundRemovedFrame.Height)
                    {
                        this.foregroundBitmap = new WriteableBitmap(backgroundRemovedFrame.Width, backgroundRemovedFrame.Height, 96.0, 96.0, PixelFormats.Bgra32, null);

                        // Set the image we display to point to the bitmap where we'll put the image data
                        this.MaskedColor.Source = this.foregroundBitmap;
                        this.isUserEngaged = true;
                       // Console.WriteLine("Yes  for track id");
                    }
                    else {
                        this.isUserEngaged = false;
                      //  this.CurrentTimerCount = 0;
                        //Console.WriteLine("reset timer ........... ");
                    }

                    // Write the pixel data into our bitmap
                    this.foregroundBitmap.WritePixels(
                        new Int32Rect(0, 0, this.foregroundBitmap.PixelWidth, this.foregroundBitmap.PixelHeight),
                        backgroundRemovedFrame.GetRawPixelData(),
                        this.foregroundBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Use the sticky skeleton logic to choose a player that we want to set as foreground. This means if the app
        /// is tracking a player already, we keep tracking the player until it leaves the sight of the camera, 
        /// and then pick the closest player to be tracked as foreground.
        /// </summary>
        private void ChooseSkeleton()
        {
            var isTrackedSkeltonVisible = false;
            var nearestDistance = float.MaxValue;
            var nearestSkeleton = 0;

            foreach (var skel in this.skeletons)
            {
                if (null == skel)
                {
                    continue;
                }

                if (skel.TrackingState != SkeletonTrackingState.Tracked)
                {
                    continue;
                }

                if (skel.TrackingId == this.currentlyTrackedSkeletonId)
                {
                    isTrackedSkeltonVisible = true;
                    break;
                }

                if (skel.Position.Z < nearestDistance)
                {
                    nearestDistance = skel.Position.Z;
                    nearestSkeleton = skel.TrackingId;
                }
            }

            if (!isTrackedSkeltonVisible && nearestSkeleton != 0)
            {
                isUserEngaged = true;
                Console.WriteLine(" track id with now ");
                this.CurrentTimerCount = 0;
                this.backgroundRemovedColorStream.SetTrackedPlayer(nearestSkeleton);
                this.currentlyTrackedSkeletonId = nearestSkeleton;
            }
        }

        /// <summary>
        /// Called when the KinectSensorChooser gets a new sensor
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="args">event arguments</param>
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.AllFramesReady -= this.SensorAllFramesReady;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();

                    // Create the background removal stream to process the data and remove background, and initialize it.
                    if (null != this.backgroundRemovedColorStream)
                    {
                        this.backgroundRemovedColorStream.BackgroundRemovedFrameReady -= this.BackgroundRemovedFrameReadyHandler;
                        this.backgroundRemovedColorStream.Dispose();
                        this.backgroundRemovedColorStream = null;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthFormat);
                    args.NewSensor.ColorStream.Enable(ColorFormat);
                    args.NewSensor.SkeletonStream.Enable();

                    this.backgroundRemovedColorStream = new BackgroundRemovedColorStream(args.NewSensor);
                    this.backgroundRemovedColorStream.Enable(ColorFormat, DepthFormat);

                    // Allocate space to put the depth, color, and skeleton data we'll receive
                    if (null == this.skeletons)
                    {
                        this.skeletons = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];
                    }

                    // Add an event handler to be called when the background removed color frame is ready, so that we can
                    // composite the image and output to the app
                    this.backgroundRemovedColorStream.BackgroundRemovedFrameReady += this.BackgroundRemovedFrameReadyHandler;

                    // Add an event handler to be called whenever there is new depth frame data
                    args.NewSensor.AllFramesReady += this.SensorAllFramesReady;

                    try
                    {
                        args.NewSensor.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    this.statusBarText.Text = Properties.Resources.ReadyForScreenshot;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }


        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ButtonScreenshotClick(object sender, RoutedEventArgs e)
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                this.statusBarText.Text = Properties.Resources.ConnectDeviceFirst;
                return;
            }

            int colorWidth = this.foregroundBitmap.PixelWidth;
            int colorHeight = this.foregroundBitmap.PixelHeight;

            // create a render target that we'll render our controls to
            var renderBitmap = new RenderTargetBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                // render the backdrop
                var backdropBrush = new VisualBrush(Backdrop);
                dc.DrawRectangle(backdropBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));

                // render the color image masked out by players
                var colorBrush = new VisualBrush(MaskedColor);
                dc.DrawRectangle(colorBrush, null, new Rect(new Point(), new Size(colorWidth, colorHeight)));
            }

            renderBitmap.Render(dv);
    
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            var time = DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            var myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var path = Path.Combine(myPhotos, "KinectSnapshot-" + time + ".png");

            // write the new file to disk
            try
            {
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteSuccess, path);
            }
            catch (IOException)
            {
               this.statusBarText.Text = string.Format(CultureInfo.InvariantCulture, Properties.Resources.ScreenshotWriteFailed, path);
            }
        }
        
        /// <summary>
        /// Handles the checking or unchecking of the near mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxNearModeChanged(object sender, RoutedEventArgs e)
        {
            if (null == this.sensorChooser || null == this.sensorChooser.Kinect)
            {
                return;
            }

            // will not function on non-Kinect for Windows devices
            try
            {
                this.sensorChooser.Kinect.DepthStream.Range = this.checkBoxNearMode.IsChecked.GetValueOrDefault()
                                                    ? DepthRange.Near
                                                    : DepthRange.Default;
            }
            catch (InvalidOperationException)
            {
            }
        }

        private string DownloadImageWithUrl(string uri, int id)
        {
            string fileName = uri;
            if (String.IsNullOrEmpty(fileName))
                fileName = "";
            else
            {
                fileName = id.ToString();
                try
                {
                    using (WebClient downloadClient = new WebClient())
                    {
                        //downloadClient.DownloadFile(serviceImageRoot + uri, imageDirectory + fileName);
                        downloadClient.DownloadFile(uri, imageDirectory + fileName);
                    }
                }
                catch (Exception)
                {
                    fileName = "";
                }
            }
            return fileName;
        } 
         
        public void Addcredit(int userID)
        {
            string sceneId = ConfigurationManager.AppSettings["EventsWebSocketSceneID"].ToString();

           // //{"userid":1,"sceneid":1,"screenid":1,"credit":10}
            //Dictionary<string, string> parametersJson = HttpWebResponseUtility.jsonParse(@"{""userid"":""50"",""sceneid"":""11"",""screenid"":""1"",""credit"":""10""} ");
            //HttpWebResponse response = HttpWebResponseUtility.CreatePostHttpResponse(loginUrl, parametersJson, null, null, Encoding.UTF8, null);

            string loginUrl = "http://app.kidswant.com.cn/bigscreen/userjoin?sceneid=1&screenid=1&credit=10&userid="+userID;

            string json = HttpWebResponseUtility.GetModel(loginUrl);
            if (json != null)
            {
                json.Replace("\"", "");
            }
            Console.WriteLine(json);

            if (json.Length > 1)
            {
                ResponseEntity responseCode = JsonConvert.DeserializeObject<ResponseEntity>(json);
                if (responseCode.ResponseCode == 1)
                {
                    CheckInUserMain usermain = JsonConvert.DeserializeObject<CheckInUserMain>(json);
                    Dispatcher.Invoke((Action)delegate
                    {
                        try
                        {
                            string url = string.Format("{0}", usermain.User.Avatar);
                            if (url != "" && url.Length > 5)
                            {
                                string filename = this.DownloadImageWithUrl(url, (int)usermain.User.Id);
                                BitmapImage src = new BitmapImage();
                                src.BeginInit();
                                src.UriSource = new Uri(imageDirectory + filename);
                                src.CacheOption = BitmapCacheOption.OnLoad;
                                src.EndInit();
                                //new Uri(url);//
                                this.PanelUserImage.ImageSource = src;
                            }
                            else
                            {

                                BitmapImage src = new BitmapImage();
                                src.BeginInit();
                                src.UriSource = new Uri("pack://application:,,,/Images/avatar_fault.png");
                                src.CacheOption = BitmapCacheOption.OnLoad;
                                src.EndInit();

                                this.PanelUserImage.ImageSource = src;

                            }

                        }
                        catch (Exception)
                        {

                        }
                        this.PanelUserName.Text = string.Format("{0}", usermain.User.Name);

                    });


                    this.Dispatcher.Invoke((Action)delegate
                    {
                        if (currentWaveID > 0)
                        { 
                           
                            if (this.HasCheckDic.ContainsKey("" + currentWaveID))
                            {
                                string value = this.HasCheckDic["" + currentWaveID];
                                int valueidint = int.Parse(value);
                                if (valueidint == 1)
                                {
                                    //第2次签到  +1
                                    valueidint++;
                                    this.HasCheckDic["" + currentWaveID] = "" + valueidint;
                                    this.Dispatcher.Invoke((Action)delegate
                                    {
                                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                        RandomADImageView();
                                        this.TextCheckIn.Text = "今天第二次签到";
                                        this.TextBok_addCredit.Visibility = Visibility.Visible;
                                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                                    });
                                }
                                else if (valueidint >= 2)
                                {
                                    //第3......n次签到  =0
                                    //这次不签到 
                                    this.Dispatcher.Invoke((Action)delegate
                                    {
                                        ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                        this.TextCheckIn.Text = "亲 明天再来签到吧";
                                        this.TextBok_addCredit.Visibility = Visibility.Hidden;
                                    });
                                }
                            }
                            else
                            {
                                //第一次签到
                                this.HasCheckDic["" + currentWaveID] = "1";
                                this.Dispatcher.Invoke((Action)delegate
                                {
                                    ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                    RandomADImageView();
                                    this.TextCheckIn.Text = "今天第一次签到";
                                    this.TextBok_addCredit.Visibility = Visibility.Visible;
                                    ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                                });
                            }

                        }

                    });

                }
                else
                {
                     /// echeck error
                    this.Dispatcher.Invoke((Action)delegate
                   {
                       this.TextBok_addCredit.Visibility = Visibility.Hidden;
                       //this.TextCheckIn.Text = "签到失败";
                       ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                   });

                }
            }
          
            
        }

        /// <summary>
        /// 成功收到ID
        /// </summary>
        /// <param name="str"></param>
        public void ReceiveSuccessWaveCode(string str)
        {
            Console.WriteLine(" ReceiveSuccessWaveCode: " + str);
        }

        public void SendID()
        {
            //App currentApp = (App)Application.Current;
            //currentApp.waveView.sendWaveID(12);
        }

        private delegate void delOnSoundReceived(Int32 id);
        public void OnSoundReceived(Int32 id)
        {
            if (this.Dispatcher.CheckAccess())
            {
                delOnSoundReceived receive = new delOnSoundReceived(OnSoundReceived);
                this.Dispatcher.BeginInvoke(receive, id);
                return;
            }
           // Console.WriteLine("识别: " + id);
            if (id > 0)
            {
                String value = Convert.ToString(id);
                Console.WriteLine("识别成功: " + value);
                mID = id;
                if (id != currentWaveID)
                {

                    if (!this.WaveHandDispatcherTimer.IsEnabled)  //要么招手 要么声波 
                    {

                        Console.WriteLine("running ... ");
                        
                        ///如果有识别到该用户就处理重复签到逻辑
                        ///
                        if (this.HasCheckDic.ContainsKey(value))
                        {
                            string valueID = this.HasCheckDic[value];
                            int valueidint = int.Parse(valueID);
                            if (valueidint == 1)
                            {
                                //第2次签到  +1
                                //valueidint++;
                                //this.HasCheckDic[value] = "" + valueidint;
                                //this.SendDispatcherTimer.Start();
                                this._backgroundWorker.RunWorkerAsync(id); 
                            }
                            else if (valueidint >= 2)
                            {
                                //第3......n次签到  =0
                                //这次不签到
                                this._backgroundWorker.RunWorkerAsync("10086");
                            }
                        }
                        else
                        {
                            //第一次签到
                            //this.HasCheckDic[value] = "1";
                            //this.SendDispatcherTimer.Start();
                            this._backgroundWorker.RunWorkerAsync(id);
                        }

                        currentWaveID = id;
                    }
                    else
                    {

                        Console.WriteLine("wave hand running ... ");
                    }

                   
                  
                   
                }
            } 
        }

        private SoundUtil.SoundReceivedDelegate mOnReceivedDelegate;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             
            mOnReceivedDelegate = new SoundUtil.SoundReceivedDelegate(OnSoundReceived);
            SoundUtil.Recording(mOnReceivedDelegate);
             
        }

        void SendDispatcherTimer_Tick(object sender, EventArgs e)
        { 
            mID = -1;
            //SoundUtil.StopSounding();
            // SoundUtil.Recording(mOnReceivedDelegate);
            this.SendDispatcherTimer.Stop();
            // and start receive wave 
            Console.WriteLine("SendDispatcherTimer_Tick stop   and start receive.....");
            //this.SendWaveDisTimer.Stop();
            //this.RemoveSendWaveDisTimer.Stop();
            this.Dispatcher.Invoke((Action)delegate
            {
                if (currentWaveID > 0)
                {
                    
                    if (this.HasCheckDic.ContainsKey("" + currentWaveID))
                    {
                        string value = this.HasCheckDic["" + currentWaveID];
                        int valueidint = int.Parse(value);
                        if (valueidint == 1)
                        {
                            //第2次签到  +1
                            valueidint++;
                            this.HasCheckDic["" + currentWaveID] = "" + valueidint;
                            this.Dispatcher.Invoke((Action)delegate
                            {
                                RandomADImageView();
                                this.TextCheckIn.Text = "今天第二次签到";
                                this.TextBok_addCredit.Visibility = Visibility.Visible;
                                ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                                ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 

                            });
                        }
                        else if (valueidint >= 2)
                        {
                            //第3......n次签到  =0
                            //这次不签到 
                            this.Dispatcher.Invoke((Action)delegate
                            {
                                ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                                this.TextCheckIn.Text = "亲 明天再来签到吧";
                                this.TextBok_addCredit.Visibility = Visibility.Hidden;
                            });
                        }
                    }
                    else
                    {
                        //第一次签到
                        this.HasCheckDic["" + currentWaveID] = "1";
                        this.Dispatcher.Invoke((Action)delegate
                        {
                            RandomADImageView();
                            this.TextCheckIn.Text = "今天第一次签到";
                            this.TextBok_addCredit.Visibility = Visibility.Visible;
                            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_Vidate"]).Begin(); 
                        });
                    }


                   /* if (value != null)
                    {
                        int valueid = int.Parse(value);
                        if (valueid == 1)
                        {
                            RandomADImageView();
                            this.TextCheckIn.Text = "今天第一次签到";
                            this.TextBok_addCredit.Visibility = Visibility.Visible;
                            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();

                        }
                        else if (valueid == 2)
                        {
                            RandomADImageView();
                            this.TextCheckIn.Text = "今天第二次签到";
                            this.TextBok_addCredit.Visibility = Visibility.Visible;
                            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_addCredit"]).Begin();
                        }
                        else {
                            this.TextCheckIn.Text = "亲 明天再来签到吧";
                            this.TextBok_addCredit.Visibility = Visibility.Hidden;
                        }
                    }*/
                }
                
            });

        }

        private void mediaElement1_MediaEnded(object sender, RoutedEventArgs e)
        {
            ((System.Windows.Media.Animation.Storyboard)this.Resources["Storyboard_showGIf"]).Resume();
        }

        private void mediaElement1_MediaOpened(object sender, RoutedEventArgs e)
        {
            ShowPauseAnimationTimer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Test
            this.WaveHandDispatcherTimer.Start();
            CheckinWithKinect();
        }

   

    }
     
    [Obfuscation(Feature = "trigger", Exclude = false)]
    public class ResponseEntity
    {
        [JsonProperty("response_code")]
        public int ResponseCode { get; set; }
         
    }
} 
 