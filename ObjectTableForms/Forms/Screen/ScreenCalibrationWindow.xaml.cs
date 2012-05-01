using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ObjectTable;
using ObjectTable.Code;
using ObjectTable.Code.PositionMapping;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTableForms.Forms.Screen
{
    /// <summary>
    /// Interaction logic for ScreenCalibrationWindow.xaml
    /// </summary>
    public partial class ScreenCalibrationWindow : Window
    {
        private TableManager _tableManager;
        private bool _drawCircles = true;
        private ScreenCircle[] _uiArray = new ScreenCircle[1000];
        private ScreenLine sl;

        public ScreenCalibrationWindow(TableManager tablemanager, bool CreateOwnObjects)
        {
            InitializeComponent();
            _tableManager = tablemanager;
            _drawCircles = CreateOwnObjects;
            if (_drawCircles)
                cb_crosshair.IsChecked = true;

            if (CreateOwnObjects)
            {
                //bind to the new object events
                //_tableManager.OnNewObjectList += new TableManager.TableManagerObjectHandler(_tableManager_OnNewObjectList);
                _tableManager.OnNewLongTermObject +=
                    new TableManager.TableManagerObjectChange(_tableManager_OnNewLongTermObject);
                _tableManager.OnObjectMove += new TableManager.TableManagerObjectChange(_tableManager_OnObjectMove);
                _tableManager.OnObjectRemove += new TableManager.TableManagerObjectChange(_tableManager_OnObjectRemove);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Load the data from the settings and update UI
            UpdateUI();
        }

        void _tableManager_OnObjectRemove(List<int> ObjectIDList)
        {
            foreach (int ID in ObjectIDList)
            {
                if (_uiArray[ID] != null)
                {
                    _tableManager.DisplayManager.DeleteElement(_uiArray[ID]);
                }
            }
        }

        void _tableManager_OnObjectMove(List<int> ObjectIDList)
        {
            if (!_drawCircles)
                return;

            //we are only interested in LongTermObjects
            foreach (TableObject obj in _tableManager.TableObjects.Where(o => o.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked))
            {
                //only moved objects that are stored in our array
                if ((ObjectIDList.Contains(obj.ObjectID))&&(_uiArray[obj.ObjectID] != null))
                {
                    //Update center - threadsafe update is handled by the Ui object itself
                    TableObject obj1 = obj;
                    _tableManager.BeamerScreen.Dispatcher.BeginInvoke((Action) (() => _uiArray[obj1.ObjectID].Center = obj1.Center));
                }
            }
        }

        void _tableManager_OnNewLongTermObject(List<int> ObjectIDList)
        {
            if (!_drawCircles)
                return;
            foreach (int i in ObjectIDList)
            {
                //Create a new UI Element
                TableObject obj = _tableManager.TableObjects.Where(o => o.ObjectID == i).ToList()[0];

                ScreenCircle sc = null;
                _tableManager.DisplayManager.WorkThreadSafe((Action) (() =>
                                                                          {
                                                                              sc = new ScreenCircle(obj.Center);
                                                                              sc.SetColor(ScreenCircle.EColor.blue);
                                                                              sc.SetText(1, obj.ObjectID.ToString());
                                                                              sc.SetText(2, obj.ObjectID.ToString());
                                                                              sc.SetText(3, obj.ObjectID.ToString());
                                                                              sc.SetText(4, obj.ObjectID.ToString());
                                                                              sc.RotationSpeed = 40;
                                                                          }), null,false);

                _uiArray[obj.ObjectID] = sc;
                _tableManager.DisplayManager.AddElement(sc, ObjectTable.Code.Display.DisplayManager.DisplayLayer.top);
            }

            //For enhanced fun, check whether there are exactly 2 lto. if so, create a line between them
            /*if (_tableManager.TableObjects.Where(o2 => o2.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked).Count() == 2)
            {
                CreateScreenline();
            }*/
        }

        private void CreateScreenline()
        {
            TPoint center1 =
                _tableManager.TableObjects.Where(
                    o2 => o2.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked).ToList()[0].Center;

            TPoint center2 =
                _tableManager.TableObjects.Where(
                    o2 => o2.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked).ToList()[1].Center;

            if (sl == null)
            {
                _tableManager.DisplayManager.WorkThreadSafe((Action)(() => { sl = new ScreenLine(center1, center2); }), null);
                _tableManager.DisplayManager.AddElement(sl, ObjectTable.Code.Display.DisplayManager.DisplayLayer.middle);
            }

            sl.Source = center1;
            sl.Destination = center2;

            _tableManager.DisplayManager.WorkThreadSafe((Action) (() => sl.CalculateLine()), null);

        }

        void _tableManager_OnNewObjectList()
        {
            //clear the surface
            _tableManager.DisplayManager.ClearScreen();
            //Create a crosshair for each new longTerm object on the surface
            foreach (TableObject obj in _tableManager.TableObjects.Where(obj => obj.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked))
            {
                if (_drawCircles)
                {
                    ScreenCircle sc = null;
                    _tableManager.DisplayManager.WorkThreadSafe((Action)(() =>
                                                                              {
                                                                                  sc = new ScreenCircle(obj.Center);
                                                                                  sc.SetColor(ScreenCircle.EColor.blue);
                                                                                  sc.SetText(1, obj.ObjectID.ToString());
                                                                                  sc.SetText(2, obj.ObjectID.ToString());
                                                                                  sc.SetText(3, obj.ObjectID.ToString());
                                                                                  sc.SetText(4, obj.ObjectID.ToString());
                                                                              }), null);

                    _tableManager.DisplayManager.AddElement(sc, ObjectTable.Code.Display.DisplayManager.DisplayLayer.top);
                }
                else
                {
                    ScreenCrosshair sch = null;
                    _tableManager.DisplayManager.WorkThreadSafe((Action)(() => sch = new ScreenCrosshair(obj.Center)),null);
                    _tableManager.DisplayManager.AddElement(sch, ObjectTable.Code.Display.DisplayManager.DisplayLayer.top);
                }
            }
        }

        private void UpdateUI()
        {
            txt_mx.Text = SettingsManager.ScreenMappingSet.MoveX.ToString();
            txt_my.Text = SettingsManager.ScreenMappingSet.MoveY.ToString();

            if (txt_xscale != null)
                txt_xscale.Text = SettingsManager.ScreenMappingSet.ScaleX.ToString();
            if (SettingsManager.ScreenMappingSet.ScaleX > s_scalex.Maximum)
            {
                s_scalex.Value = s_scalex.Maximum;
            }
            else if (SettingsManager.ScreenMappingSet.ScaleX < s_scalex.Minimum)
            {
                s_scalex.Value = s_scalex.Minimum;
            }
            else
            {
                s_scalex.Value = SettingsManager.ScreenMappingSet.ScaleX;
            }

            if (txt_yscale != null)
                txt_yscale.Text = SettingsManager.ScreenMappingSet.ScaleY.ToString();
            if (SettingsManager.ScreenMappingSet.ScaleY > s_scaley.Maximum)
            {
                s_scaley.Value = s_scaley.Maximum;
            }
            else if (SettingsManager.ScreenMappingSet.ScaleY < s_scaley.Minimum)
            {
                s_scaley.Value = s_scaley.Minimum;
            }
            else
            {
                s_scaley.Value = SettingsManager.ScreenMappingSet.ScaleY;
            }

            txt_colormoveX.Text = SettingsManager.ScreenMappingSet.ColorMoveX.ToString();
            txt_colormoveY.Text = SettingsManager.ScreenMappingSet.ColorMoveY.ToString();

            //Raise event so that objects will adjust
            SettingsManager.ScreenMappingSet.CauseUpdateEvent();
        }

        private void b_mym_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveY--;
            UpdateUI();
        }

        private void txt_mymm_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveY -= 10;
            UpdateUI();
        }

        private void b_mxp_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveX++;
            UpdateUI();
        }

        private void b_m_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveX+= 10;
            UpdateUI();
        }

        private void b_myp_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveY++;
            UpdateUI();
        }

        private void b_mypp_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveY+= 10;
            UpdateUI();
        }

        private void b_mxm_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveX--;
            UpdateUI();
        }

        private void b_mxmm_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ScreenMappingSet.MoveX -= 10;
            UpdateUI();
        }

        private void s_scalex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void s_scaley_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void txt_xscale_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                double scale = Convert.ToDouble(txt_xscale.Text);
                SettingsManager.ScreenMappingSet.ScaleX = scale;
                UpdateUI();
            }
            catch (Exception)
            {
                txt_xscale.Text = SettingsManager.ScreenMappingSet.ScaleX.ToString();
            }
        }

        private void txt_yscale_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                double scale = Convert.ToDouble(txt_yscale.Text);
                SettingsManager.ScreenMappingSet.ScaleY = scale;
                UpdateUI();
            }
            catch (Exception)
            {
                txt_yscale.Text = SettingsManager.ScreenMappingSet.ScaleY.ToString();
            }
        }

        private void txt_mx_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int value = Convert.ToInt32(txt_mx.Text);
                SettingsManager.ScreenMappingSet.MoveX = value;
                UpdateUI();
            }
            catch (Exception)
            {
                //txt_mx.Text = SettingsManager.ScreenMappingSet.MoveX.ToString();
            }
        }

        private void txt_my_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int value = Convert.ToInt32(txt_my.Text);
                SettingsManager.ScreenMappingSet.MoveY = value;
                UpdateUI();
            }
            catch (Exception)
            {
                //txt_my.Text = SettingsManager.ScreenMappingSet.MoveY.ToString();
            }
        }

        private void txt_xscale_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void s_scaley_MouseMove(object sender, MouseEventArgs e)
        {
            SettingsManager.ScreenMappingSet.ScaleY = s_scaley.Value;
            if (txt_yscale != null)
                txt_yscale.Text = s_scaley.Value.ToString();
            UpdateUI();
        }

        private void s_scalex_MouseMove(object sender, MouseEventArgs e)
        {
            SettingsManager.ScreenMappingSet.ScaleX = s_scalex.Value;
            if (txt_xscale != null)
                txt_xscale.Text = s_scalex.Value.ToString();
            UpdateUI();
        }

        private void b_ok_Click(object sender, RoutedEventArgs e)
        {
            //Save values
            SettingsManager.SaveSettings();
            this.Close();
        }

        private void cb_kreis_Checked(object sender, RoutedEventArgs e)
        {
            _drawCircles = false;
        }

        private void cb_crosshair_Checked(object sender, RoutedEventArgs e)
        {
            _drawCircles = true;
        }

        private void txt_colormoveX_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                SettingsManager.ScreenMappingSet.ColorMoveX = Convert.ToInt32(txt_colormoveX.Text);
                UpdateUI();
            }
            catch (Exception)
            {
                
            }
        }

        private void txt_colormoveY_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                SettingsManager.ScreenMappingSet.ColorMoveY = Convert.ToInt32(txt_colormoveY.Text);
                UpdateUI();
            }
            catch (Exception)
            {

            }
        }

    }
}
