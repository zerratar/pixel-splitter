using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using LiveSplit.PixelSplitter.Extensions;
using LiveSplit.PixelSplitter.Models;

namespace LiveSplit.PixelSplitter.Providers
{
    internal class CaptureDeviceGameImageProvider : IGameImageProvider
    {
        private readonly ConcurrentDictionary<string, CaptureDeviceGameImageSource> loadedSources
            = new ConcurrentDictionary<string, CaptureDeviceGameImageSource>();

        public static PixelSplitterSettingsProvider SettingsProvider { get; set; }

        public IGameImageSource Get(string key)
        {
            if (loadedSources.TryGetValue(key, out var device))
            {
                return device;
            }

            var devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            return loadedSources[key] = new CaptureDeviceGameImageSource(devices.First(x => x.Name == key));
        }

        class CaptureDeviceGameImageSource : IGameImageSource
        {
            private readonly VideoCaptureDevice device;
            private bool disposed;

            private bool newFrameAvailable;

            private readonly object frameLock = new object();

            private Bitmap frame;
            private Bitmap backframe;
            private bool frameSkip;

            public CaptureDeviceGameImageSource(FilterInfo filterInfo)
            {
                this.device = new VideoCaptureDevice(filterInfo.MonikerString);
                this.device.NewFrame += DeviceOnNewFrame;
                this.device.Start();
            }

            public bool HasNewFrame => this.newFrameAvailable;

            public Bitmap Frame
            {
                get
                {
                    //lock (frameLock)
                    lock (frameLock)
                    {
                        return this.frame;
                    }
                }
            }

            public void SaveFrame(string path)
            {
                lock (frameLock)
                {
                    var settings = SettingsProvider.Get();
                    if (settings.ImageMatchMask != null && settings.ImageMatchMask.Count > 0)
                    {
                        var mask = settings.GetImageBitMask(this.backframe.Width, this.backframe.Height);
                        var applyMask = new ApplyMask(mask);

                        using (var masked = applyMask.Apply(this.backframe))                        
                        {
                            masked.Save(path, ImageFormat.Png);                            
                        }
                    }
                    else
                    {
                        this.backframe.Save(path, ImageFormat.Png);
                    }
                }
            }

            public IMaskedGameImage GetMaskedFrame(PixelSplitterSettings settings)
            {
                lock (frameLock)
                {
                    var mask = settings.GetImageBitMask(this.frame.Width, this.frame.Height);
                    var applyMask = new ApplyMask(mask);
                    return new MaskedGameImage(applyMask.Apply(this.frame));
                }
            }

            private void DeviceOnNewFrame(object sender, NewFrameEventArgs eventArgs)
            {
                //try
                //{
                //    if (this.Frame != null)
                //    {
                //        this.Frame.Dispose();
                //    }
                //}
                //catch { }
                //this.frame = new BitmapLocker(eventArgs.Frame);

                //if (this.frameSkip)
                //{
                //    this.frameSkip = false;
                //    return;
                //}
                //else
                //{
                //    this.frameSkip = true;
                //}


                lock (frameLock)
                {
                    var settings = SettingsProvider.Get();
                    if (settings.ImageMatchMask != null && settings.ImageMatchMask.Count > 0)
                    {
                        this.frame = (Bitmap)eventArgs.Frame.Clone();

                        //var mask = settings.GetImageBitMask(this.frame.Width, this.frame.Height);
                        //var applyMask = new ApplyMask(mask);
                        //this.frame = applyMask.Apply(this.frame);

                        //var mask = settings.GetMaskBitmap(this.frame.Width, this.frame.Height, frame.PixelFormat);
                        //var applyMask = new Merge(mask);
                        //this.frame = applyMask.Apply(this.frame);

                        //using (var gfx = Graphics.FromImage(frame))
                        //{
                        //    var mask = settings.GetMaskBitmap(this.frame.Width, this.frame.Height, frame.PixelFormat);
                        //    gfx.DrawImage(mask, new Point(0, 0));
                        //}

                        //var resize = new ResizeBilinear(this.frame.Width / 2, this.frame.Height / 2);
                        //this.frame = resize.Apply(this.frame);

                        this.backframe = (Bitmap)this.frame.Clone();
                    }
                    else
                    {
                        this.frame = (Bitmap)eventArgs.Frame.Clone();
                        this.backframe = (Bitmap)eventArgs.Frame.Clone();
                    }

                    this.newFrameAvailable = true;
                }
            }

            public void Dispose()
            {
                if (this.disposed) return;

                this.disposed = true;
                this.device.NewFrame -= DeviceOnNewFrame;
                this.device.SignalToStop();
            }

            public bool Available => this.device != null && this.device.IsRunning;

            public void Next()
            {
                this.newFrameAvailable = false;
            }

            //public Color GetPixel(int x, int y)
            //{
            //    if (!this.Available)
            //    {
            //        return Color.Empty;
            //    }

            //    return this.frame.GetPixel(x, y);
            //}

        }
    }
}