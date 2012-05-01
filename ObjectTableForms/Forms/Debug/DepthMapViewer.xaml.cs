using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ObjectTable.Code;
using ObjectTable.Code.Debug;
using ObjectTable.Code.Display;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Kinect.Structures;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Drawing.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Timer = System.Timers.Timer;

namespace ObjectTableForms.Forms.Debug
{
    /// <summary>
    /// Interaction logic for DepthMapViewer.xaml
    /// </summary>
    public partial class DepthMapViewer : Window
    {
        private Timer _actualisationtimer;
        private TableManager _tableManager;

        private BitmapSource _src, _rsrc, _vsrc;
        private List<TableObject> _objectlist;

        private delegate void ImageUpdater();

        private List<Color> _colorList;
        private bool _busyUpdate = false;

        public DepthMapViewer(ref TableManager tmgr)
        {
            InitializeComponent();
            _tableManager = tmgr;

            _tableManager.OnNewObjectList += new TableManager.TableManagerObjectHandler(_tableManager_OnNewObjectList);

            //Create a list full of 100 random colors
            Random rnd = new Random();
            _colorList = new List<Color>();
            for (int i = 0; i < 100;i++)
            {
                _colorList.Add(Color.FromArgb(255,(byte)rnd.Next(255), (byte)rnd.Next(255), (byte)rnd.Next(255)));
            }
            this.Show();
        }

        void _tableManager_OnNewObjectList()
        {
            if (!_busyUpdate)
            {
                _busyUpdate = true;
                this.Dispatcher.Invoke((Action) (() => _actualisationtimer_Elapsed(null, null)), null);
                _busyUpdate = false;
            }
        }

        void _actualisationtimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RecognitionDataPacket rdp = _tableManager.LastRecognitionDataPacket;

            if ((rdp.rawDepthImage != null)&&(rb_rawimage.IsChecked == true))
            {
                Bitmap bmp = MapVisualizer.VisualizeDepthImage(rdp.rawDepthImage);
                _rsrc = bmp.ToWpfBitmap();
            }

            if ((rdp.correctedDepthImage != null)&&(rb_correctedimage.IsChecked == true))
            {
                Bitmap src = MapVisualizer.VisualizeDepthImage(rdp.correctedDepthImage);
                _src = src.ToWpfBitmap();
            }

            if ((_tableManager.LastKinectColorFrame != null)&&(rb_video.IsChecked == true))
            {
                Bitmap vbmp = new Bitmap(_tableManager.LastKinectColorFrame);
                _vsrc = vbmp.ToWpfBitmap();
            }

            //Get objects
            _objectlist = _tableManager.TableObjects;

            //Thread safe update
            rb_correctedimage.Dispatcher.Invoke(new ImageUpdater(ThreadsafeUpdate), null);
        }

        private void rb_rawimage_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void rb_correctedimage_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void UpdateObjectIndicators()
        {
            canvas.Children.Clear();

            //if the image is originally only 320x240, the indicator-coordinates have to be scaled up
            int scale = 1;
            if (_tableManager.LastRecognitionDataPacket.correctedDepthImage.Width == 320)
                scale = 2;

            foreach(TableObject obj in _objectlist.Where(o => o.Center != null))
            {
                Rectangle r = new Rectangle();

                //Place the rectangle so that the object is in its center
                int x, y;
                if (((bool)rb_video.IsChecked || (bool)rb_indicator.IsChecked))
                {
                    x = obj.Center.ColorX - (int) Math.Round((double) obj.Radius/2);
                    y = obj.Center.ColorY - (int) Math.Round((double) obj.Radius/2);
                }
                else
                {
                    x = obj.Center.DepthX * scale - (int)Math.Round((double)obj.Radius / 2);
                    y = obj.Center.DepthY * scale - (int)Math.Round((double)obj.Radius / 2);
                }
                if (x < 0)
                    x = 0;
                if (y < 0)
                    y = 0;

                r.Margin = new Thickness(x, y, 0, 0);
                r.Width = obj.Radius;
                r.Height = obj.Radius;
                int id = obj.ObjectID;
                if (id > 99)
                    id = 99;

                r.Stroke = new SolidColorBrush(Colors.Red);

                TextBlock blck = new TextBlock();
                blck.Text = obj.ObjectID.ToString();

                if (obj.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked)
                {
                    blck.Text += "L";
                }
                else if (obj.TrackingStatus == TableObject.ETrackingStatus.ShortlyTracked)
                {
                    blck.Text += "S";
                }
                else if (obj.TrackingStatus == TableObject.ETrackingStatus.LongTermGuessed)
                {
                    blck.Text += "G";
                }
                else
                {
                    blck.Text += "N";
                }
                blck.Foreground = new SolidColorBrush(Colors.Red);
                blck.Margin = new Thickness(x, y, 0, 0);

                //If rotation is defined, add rotation indicator
                if (obj.RotationDefined)
                {
                    Point pdest;
                    Vector dest = obj.DirectionVector*20;
                    if (!((bool)rb_video.IsChecked||(bool)rb_indicator.IsChecked))
                    {
                        pdest = new System.Drawing.Point((int) Math.Round(obj.Center.DepthX*scale + dest.X),
                                                               (int) Math.Round(obj.Center.DepthY*scale + dest.Y));
                    }
                    else
                    {
                        pdest = new System.Drawing.Point((int)Math.Round(obj.Center.ColorX + dest.X),
                                                               (int)Math.Round(obj.Center.ColorY + dest.Y));
                    }
                    Line l = new Line();

                    if (!((bool)rb_video.IsChecked || (bool)rb_indicator.IsChecked))
                    {
                        l.X1 = obj.Center.DepthX*scale;
                        l.Y1 = obj.Center.DepthY*scale;
                    }
                    else
                    {
                        l.X1 = obj.Center.ColorX;
                        l.Y1 = obj.Center.ColorY;
                    }

                    l.X2 = pdest.X;
                    l.Y2 = pdest.Y;
                    l.Stroke = new SolidColorBrush(Colors.Pink);
                    canvas.Children.Add(l);
                }
                canvas.Children.Add(r);
                canvas.Children.Add(blck);
            }

            //Hand objects+
            if (_tableManager.LastRecognitionDataPacket.HandObj != null)
            {
                HandObject ho = _tableManager.LastRecognitionDataPacket.HandObj;
                ho.PointsAt = PositionMapper.GetColorCoordinatesfromDepth(ho.PointsAt);

                Ellipse e = new Ellipse();
                e.Height = 5;
                e.Width = 5;
                e.Stroke = new SolidColorBrush(Colors.Cyan);
                canvas.Children.Add(e);
                if (((bool)rb_video.IsChecked || (bool)rb_indicator.IsChecked))
                {
                    Canvas.SetLeft(e, ho.PointsAt.ColorX);
                    Canvas.SetTop(e, ho.PointsAt.ColorY);
                }
                else
                {
                    Canvas.SetLeft(e, ho.PointsAt.DepthX * scale);
                    Canvas.SetTop(e, ho.PointsAt.DepthY * scale);
                }
            }
        }
        private void ThreadsafeUpdate()
        {
            try
            {
                if (rb_correctedimage.IsChecked == true)
                {
                    if (_src != null)
                        img_depth.Source = _src;
                    UpdateObjectIndicators();
                }
                else if (rb_rawimage.IsChecked == true)
                {
                    if (_rsrc != null)
                        img_depth.Source = _rsrc;
                    UpdateObjectIndicators();
                }
                else
                {
                    if (_vsrc != null)
                        img_depth.Source = _vsrc;
                    UpdateObjectIndicators();
                }
            }
            catch (Exception) { }
        }

        private void rb_video_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
