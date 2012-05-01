using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ObjectTable.Code;
using ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTableForms.Forms.Debug;
using ObjectTableForms.Forms.Screen;
using ObjectTable.Code.Display.GUI;

namespace RotationTestApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TableManager tableManager;
        public BeamerScreenWindow bsw = new BeamerScreenWindow();

        public List<ObjectTestItem> TestItems = new List<ObjectTestItem>();

        public MainWindow()
        {
            InitializeComponent();

            bsw = new BeamerScreenWindow();
            bsw.Show();

            tableManager = new TableManager(bsw.GetBeamerUC());

            tableManager.Start();
            tableManager.ToggleObjectRecognition = true;
            tableManager.ToggleObjectTracking = true;
            tableManager.ToggleObjectRotationAnalysation = true;

            tableManager.OnNewLongTermObject += new TableManager.TableManagerObjectChange(tableManager_OnNewLongTermObject);
            tableManager.OnObjectMove += new TableManager.TableManagerObjectChange(tableManager_OnObjectMove);
            tableManager.OnObjectRemove += new TableManager.TableManagerObjectChange(tableManager_OnObjectRemove);
            tableManager.OnObjectRotate += new TableManager.TableManagerObjectChange(tableManager_OnObjectRotate);
            ScreenCalibrationWindow scw = new ScreenCalibrationWindow(tableManager,true);
            scw.Show();
            DepthMapViewer dv = new DepthMapViewer(ref tableManager);
            dv.Show();
        }

        void tableManager_OnObjectRotate(List<int> ObjectIDList)
        {
            tableManager_OnObjectMove(ObjectIDList);
        }

        void tableManager_OnObjectRemove(List<int> ObjectIDList)
        {
            foreach (int objid in ObjectIDList)
            {
                List<ObjectTestItem> oti = TestItems.Where(obj => obj.objectID == objid).ToList();

                if (oti.Count > 0)
                {
                    ObjectTestItem ot = oti[0];
                    tableManager.DisplayManager.DeleteElement(ot.circle);
                    if (ot.screenline != null)
                        tableManager.DisplayManager.DeleteElement(ot.screenline);
                    TestItems.Remove(ot);
                }
            }
        }

        void tableManager_OnObjectMove(List<int> ObjectIDList)
        {
            foreach (int objid in ObjectIDList)
            {
                List<ObjectTestItem> oti = TestItems.Where(obj => obj.objectID == objid).ToList();

                if (oti.Count > 0)
                {
                    ObjectTestItem ot = oti[0];
                    TableObject tobj = tableManager.TableObjects.Where(o => o.ObjectID == ot.objectID).ToList()[0];

                    tableManager.DisplayManager.WorkThreadSafe(
                        (Action) (() =>
                                      {
                                          ot.circle.Center = tobj.Center;
                                          if (tobj.RotationDefined)
                                          {
                                              tableManager.DisplayManager.DeleteElement(ot.screenline);
                                              tobj.DirectionVector = ScreenMathHelper.RescaleVector(
                                                  tobj.DirectionVector, 350.0);
                                              ot.screenline = new ObjectTable.ScreenLine(tobj.Center,
                                                                                         tobj.DirectionVector);
                                              tableManager.DisplayManager.AddElement(ot.screenline,
                                                                                     ObjectTable.Code.Display.
                                                                                         DisplayManager.DisplayLayer.
                                                                                         middle);
                                              ot.circle.SetText(4,
                                                                ScreenMathHelper.VectorToDegree(tobj.DirectionVector).
                                                                    ToString());
                                          }
                                          else if (ot.screenline != null)
                                          {
                                              tableManager.DisplayManager.DeleteElement(ot.screenline);
                                              ot.screenline = null;
                                          }
                                      }), null);
                }
            }
        }

        void tableManager_OnNewLongTermObject(List<int> ObjectIDList)
        {
            foreach (int objID in ObjectIDList)
            {
                ObjectTestItem oti = new ObjectTestItem();
                oti.objectID = objID;

                TableObject tobj = tableManager.TableObjects.Where(o => o.ObjectID == oti.objectID).ToList()[0];

                tableManager.DisplayManager.WorkThreadSafe(
                    (Action) (() =>
                                  {
                                      oti.circle = new ObjectTable.ScreenCircle(tobj.Center);
                                      oti.circle.SetText(1, oti.objectID.ToString());
                                      oti.circle.SetText(4, oti.objectID.ToString());
                                      oti.circle.RotationSpeed = 20;

                                      if (tobj.RotationDefined)
                                      {
                                          tobj.DirectionVector = ScreenMathHelper.RescaleVector(tobj.DirectionVector,
                                                                                                350.0);
                                          oti.screenline = new ObjectTable.ScreenLine(tobj.Center, tobj.DirectionVector);
                                          tableManager.DisplayManager.AddElement(oti.screenline,
                                                                                 ObjectTable.Code.Display.DisplayManager
                                                                                     .
                                                                                     DisplayLayer.middle);
                                      }
                                      tableManager.DisplayManager.AddElement(oti.circle,
                                                                             ObjectTable.Code.Display.DisplayManager.
                                                                                 DisplayLayer.top);
                                  }), null);

                TestItems.Add(oti);
            }
        }
    }
}
