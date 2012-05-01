using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using ObjectTable.Code.Display.GUI.ScreenElements.ScreenLine;
using ObjectTable.Code.Kinect;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;
using ObjectTable.Code.Tracking.DataStructures;

namespace ObjectTable.Code.Tracking
{
    /// <summary>
    /// Tracks the various objects supplied by the RecognitionManager
    /// </summary>
    class ObjectTracker
    {
        private HistoryTrackList _historyTrackList;
        private double MaxDistance = 0.0;

        /// <summary>
        /// The Time [ms] how long tracking takes
        /// </summary>
        public int TrackingDuration;

        public ObjectTracker()
        {
            _historyTrackList = new HistoryTrackList();

            //calculate max possible object distance
            int DepthWidth = 0, DepthHeight = 0;
            SettingsManager.KinectSet.GetDepthResolution(out DepthWidth, out DepthHeight);
            MaxDistance = Math.Sqrt(DepthWidth*DepthWidth + DepthHeight*DepthHeight);
        }

        public List<TableObject> TrackObjects(List<TableObject> inputObjects)
        {
            //Save the time for performance calculations
            DateTime dtBegin = DateTime.Now;

            //First convert TableObjects to TrackObjects
            List<TrackObject> TrackObjects = TrackObject.ConvertToTrackObjects(inputObjects);

            //Special case: first frame ever
            if (!_historyTrackList.HasFrames())
            {
                //Assign ids, add to HistoryList and return
                AssignIDs(ref TrackObjects);
                List<TableObject> returnList = TrackObject.ConvertToTableObjects(TrackObjects);
                _historyTrackList.AddNew(returnList);
                return returnList;
            }

            //Match the non-moving long term objects
            MatchObjects(ref TrackObjects,TableObject.ETrackingStatus.LongTermTracked,SettingsManager.TrackingSet.LtNonMovingCertainity);

            //if guessing is enabled, match guessed objects to non-moved long term objects
            if (SettingsManager.TrackingSet.GuessLongTermObjects)
            {
                MatchObjects(ref TrackObjects, TableObject.ETrackingStatus.LongTermGuessed,
                             SettingsManager.TrackingSet.LtMovingCertainity,TableObject.ETrackingStatus.LongTermTracked);
            }

            //Are there any unmatched objects?
            if (TrackObjects.Where(obj => obj.ObjReference.TrackingStatus == TableObject.ETrackingStatus.NotTracked).Count() > 0)
            {
                //Track long term objects that might be moved (use a lower certainity threshold)
                MatchObjects(ref TrackObjects,TableObject.ETrackingStatus.LongTermTracked,SettingsManager.TrackingSet.LtMovingCertainity);

                //Match objects that are newly added
                MatchObjects(ref TrackObjects, TableObject.ETrackingStatus.ShortlyTracked,
                             SettingsManager.TrackingSet.ShortlyAddedCertainity);

                //Any untracked objects? assign new ids
                AssignIDs(ref TrackObjects);
            }
            
            //now eventually increase the status (shortly tracked -> long term tracked) of an object, based on its age 
            UpdateTrackObjectStatusAndAge(ref TrackObjects);

            //If enabled, guess long term objects that arent visible right now
            if (SettingsManager.TrackingSet.GuessLongTermObjects)
            {
                GuessLongTermObjects(ref TrackObjects);
            }

            //Finally, convert back to a List<TableObject>, add it to the history container and return
            List<TableObject> resultList = TrackObject.ConvertToTableObjects(TrackObjects);

            //If desired, smoothObjects
            if (SettingsManager.TrackingSet.SmoothObjects)
            {
                List<TableObject> smoothList = SmoothObjects(resultList);
                _historyTrackList.AddNew(resultList);
                resultList = smoothList;
            }

            //Calculate Performance
            TimeSpan ts = DateTime.Now - dtBegin;
            TrackingDuration = (int)Math.Round(ts.TotalMilliseconds);

            return resultList;
        }

        private List<TableObject> SmoothObjects(List<TableObject> targets)
        {
            List<TableObject> result = new List<TableObject>();

            if (_historyTrackList.GetLastFrame() == null || _historyTrackList.Get2ndNewContainer() == null)
                return targets;

            List<TableObject> old1 = _historyTrackList.GetLastFrame().ObjectList;
            List<TableObject> old2 = _historyTrackList.Get2ndNewContainer().ObjectList;

            foreach (TableObject srcObj in targets)
            {
                //get older instances
                TableObject oldInstance1 = null, oldInstance2 = null;
                if (old1.Where(o => o.ObjectID == srcObj.ObjectID).Count() > 0)
                    oldInstance1 = old1.Where(o => o.ObjectID == srcObj.ObjectID).ToArray()[0];
                if (old2.Where(o => o.ObjectID == srcObj.ObjectID).Count() > 0)
                    oldInstance2 = old2.Where(o => o.ObjectID == srcObj.ObjectID).ToArray()[0];

                //Only if both instances are not null
                if (oldInstance1 == null || oldInstance2 == null)
                    continue;

                //Calculate average
                TableObject avrgObj = (TableObject) srcObj.Clone();
                avrgObj.Center.DepthX = (int)Math.Round((srcObj.Center.DepthX + oldInstance1.Center.DepthX + oldInstance2.Center.DepthX) / 3.0);
                avrgObj.Center.DepthY = (int)Math.Round((srcObj.Center.DepthY + oldInstance1.Center.DepthY + oldInstance2.Center.DepthY) / 3.0);
                
                //Check whether all direction vectors aren't (0,0)
                if (!(oldInstance1.DirectionVector.Equals(new Vector(0, 0)) || oldInstance2.DirectionVector.Equals(new Vector(0, 0)) || srcObj.DirectionVector.Equals(new Vector(0, 0))))
                {
                    avrgObj.DirectionVector =
                        ScreenMathHelper.Average(new List<Vector>
                                                     {
                                                         srcObj.DirectionVector,
                                                         oldInstance1.DirectionVector,
                                                         oldInstance2.DirectionVector
                                                     });
                }

                //add to result list
                result.Add(avrgObj);
            }
            return result;
        }

        /// <summary>
        /// Returns a list of objects that are new since the last frame
        /// </summary>
        /// <returns></returns>
        public List<int> GetNewObjects()
        {
            List<int> NewObjects = new List<int>();
            HistoryContainer Objn = _historyTrackList.GetLastFrame();

            foreach (TableObject o in Objn.ObjectList)
            {
                if (_historyTrackList.GetObjectAge(o.ObjectID) < 2)
                {
                    NewObjects.Add(o.ObjectID);
                }
            }

            return NewObjects;
        }

        /// <summary>
        /// Gets a list of ObjectID's that are new longTermObjects 
        /// </summary>
        /// <returns></returns>
        public List<int> GetNewLongTermObjects()
        {
            HistoryContainer Hcn = _historyTrackList.GetLastFrame();
            HistoryContainer H2n = _historyTrackList.Get2ndNewContainer();

            List<int> newLTO = new List<int>();

            //If there is no second objectContainer, return a empty list
            if (H2n == null)
                return newLTO;

            //get all long term objects
            foreach (TableObject obj in Hcn.ObjectList.Where(o => o.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked))
            {
                //Check whether these objects were shortly tracked objects in the frame before
                if (H2n.ObjectList.Where(o => o.ObjectID == obj.ObjectID).ToList()[0].TrackingStatus == TableObject.ETrackingStatus.ShortlyTracked)
                {
                    newLTO.Add(obj.ObjectID);
                }
            }

            return newLTO;
        }

        /// <summary>
        /// Gets a list of objects that were deleted since last frame
        /// </summary>
        /// <returns></returns>
        public List<int> GetDeletedObjects()
        {
            HistoryContainer Hcn = _historyTrackList.GetLastFrame();
            HistoryContainer H2n = _historyTrackList.Get2ndNewContainer();

            List<int> deletedObjects = new List<int>();

            //If there is no second objectContainer, return a empty list
            if (H2n == null)
                return deletedObjects;

            foreach (TableObject o in H2n.ObjectList)
            {
                //is it in the newest frame, too?
                if (Hcn.ObjectList.Where(obj => obj.ObjectID == o.ObjectID).Count() == 0)
                    deletedObjects.Add(o.ObjectID);
            }

            return deletedObjects;
        }

        /// <summary>
        /// Gets the ID of Moved objects (Threshold: SettingsManager.TrackingSet.ObjectMoveListMinDistance)
        /// </summary>
        /// <returns></returns>
        public List<int> GetMovedObjects()
        {
            HistoryContainer Hcn = _historyTrackList.GetLastFrame();
            HistoryContainer H2n = _historyTrackList.Get2ndNewContainer();

            List<int> MovedObjects = new List<int>();

            //If there is no second container, return empty list
            if (H2n == null)
                return MovedObjects;

            foreach (TableObject obj in Hcn.ObjectList)
            {
                //Did this object exist one frame ago, and does it have a center??
                if ((obj.CenterDefined) && (H2n.ObjectList.Where(o => o.ObjectID == obj.ObjectID).Count() > 0))
                {
                    TableObject objAlt = H2n.ObjectList.Where(o => o.ObjectID == obj.ObjectID).ToList()[0];
                    int movedDistance = (int)Math.Round(obj.Center.DistanceTo(objAlt.Center, TPoint.PointCreationType.depth));
                    if (movedDistance >= SettingsManager.TrackingSet.ObjectMoveListMinDistance)
                        MovedObjects.Add(obj.ObjectID);
                }
            }

            return MovedObjects;
        }

        public List<int> GetRotationChangedObjects()
        {
            HistoryContainer Hcn = _historyTrackList.GetLastFrame();
            HistoryContainer H2n = _historyTrackList.Get2ndNewContainer();

            List<int> RotatedObjects = new List<int>();

            //If there is no second container, return empty list
            if (H2n == null)
                return RotatedObjects;

            foreach (TableObject obj in Hcn.ObjectList)
            {
                //Did this object exist one frame ago, and does it have a center??
                if ((obj.RotationDefined) && (H2n.ObjectList.Where(o => o.ObjectID == obj.ObjectID).Count() > 0))
                {
                    TableObject objAlt = H2n.ObjectList.Where(o => o.ObjectID == obj.ObjectID).ToList()[0];
                    if (obj.DirectionVector != objAlt.DirectionVector)
                    {
                        RotatedObjects.Add(obj.ObjectID);
                    }
                }
            }

            return RotatedObjects; 
        }

        private void GuessLongTermObjects(ref List<TrackObject> ObjectList)
        {
            //Get a list of the former long term objects
            List<TableObject> OldLongTermObjects =
                _historyTrackList.GetLastFrame().ObjectList.Where(
                    obj => obj.TrackingStatus == TableObject.ETrackingStatus.LongTermTracked).ToList();

            List<TableObject> OldLongTermGuessedObjects =
                _historyTrackList.GetLastFrame().ObjectList.Where(
                    obj => obj.TrackingStatus == TableObject.ETrackingStatus.LongTermGuessed).ToList();

            //Now add them if they aren't too old
            foreach (TableObject to in OldLongTermObjects)
            {
                //Only if not in list
                if (ObjectList.Where(obj => obj.ObjReference.ObjectID == to.ObjectID).Count() == 0)
                {
                    //Check age
                    int age = _historyTrackList.GetGuessedAge(to.ObjectID);
                    if (age < SettingsManager.TrackingSet.GuessObjectMaxAge)
                    {
                        TableObject t2 = (TableObject) to.Clone();
                        TrackObject tob = new TrackObject();
                        t2.TrackingStatus = TableObject.ETrackingStatus.LongTermGuessed;
                        tob.ObjReference = t2;

                        
                        ObjectList.Add(tob);
                    }
                }
            }

            //Do the same for the Already guessed objects
            foreach (TableObject to in OldLongTermGuessedObjects)
            {
                //Only if not in list
                if (ObjectList.Where(obj => obj.ObjReference.ObjectID == to.ObjectID).Count() == 0)
                {
                    //Check age
                    int age = _historyTrackList.GetGuessedAge(to.ObjectID);
                    if (age < SettingsManager.TrackingSet.GuessObjectMaxAge)
                    {
                        TrackObject tob = new TrackObject();
                        tob.ObjReference = to;
                        ObjectList.Add(tob);
                    }
                }
            }
        }

        private void UpdateTrackObjectStatusAndAge(ref List<TrackObject> ObjectList)
        {
            foreach (TrackObject obj in ObjectList)
            {
                //Get age
                int age = _historyTrackList.GetObjectAge(obj.ObjReference.ObjectID);
                //set age in properties
                obj.ObjReference.TrackingFrameExistence = age;
                
                //Is ist an shortlyTracked (==new) object that is old enought to become long term tracked?
                if (obj.ObjReference.TrackingStatus == TableObject.ETrackingStatus.ShortlyTracked && age >= SettingsManager.TrackingSet.MinimalLongTermAge)
                {
                    obj.ObjReference.TrackingStatus = TableObject.ETrackingStatus.LongTermTracked;
                }
            }
        }

        private void AssignIDs(ref List<TrackObject> ObjectList)
        {
            //get a list with assingned IDs
            List<int> AssignedIDs = _historyTrackList.GetUsedIDs();
            int TrackIDCounter = 0;

            //Select all untracked objects (ID == 0)
            foreach(TrackObject to in ObjectList.Where(obj => obj.ObjReference.ObjectID == 0))
            {
                //Get the next free ID
                bool work = true;
                while (work)
                {
                    TrackIDCounter++;
                    if (!AssignedIDs.Contains(TrackIDCounter))
                        work = false;
                }

                //Assign ID
                to.ObjReference.ObjectID = TrackIDCounter;
                to.ObjReference.TrackingStatus = TableObject.ETrackingStatus.ShortlyTracked;
            }
        }

        
        private void MatchObjects(ref List<TrackObject> TrackObjList, TableObject.ETrackingStatus TrackStatusFilter, double CertainityThreshold, TableObject.ETrackingStatus AssignedStatus = TableObject.ETrackingStatus.NotTracked)
        {
            //Calculate the certainity for the selected objects
            List<int> UsedIDs = CalculateCertainity(ref TrackObjList, TrackStatusFilter);

            
            /*//For objects with a very good certainity value, assign objectid
            foreach (TrackObject to in TrackObjList.Where(obj => obj.TrackCertainity >= CertainityThreshold))
            {
                to.ObjReference.ObjectID = to.BestCertainityWithID;
                to.ObjReference.TrackingStatus = TrackStatusFilter;
                //If the assigned TrackStatus is defined, use it
                if (AssignedStatus != TableObject.ETrackingStatus.NotTracked)
                    to.ObjReference.TrackingStatus = AssignedStatus;
            }*/

            //There might be several objects with the same proposed ID. For each ObjectID, take the object with the best Certainity
            foreach (int checkID in UsedIDs)
            {
                TrackObject maxCertainityRef = null;
                double maxCertainity = 0.0;
                
                //Gett all proposed objs for this ID, and with the minimum required threshold
                foreach(TrackObject tobj in TrackObjList.Where(obj => obj.BestCertainityWithID == checkID).Where(obj => obj.TrackCertainity >= CertainityThreshold))
                {
                    //Take the object with the best Certainity
                    if (tobj.TrackCertainity > maxCertainity)
                    {
                        maxCertainity = tobj.TrackCertainity;
                        maxCertainityRef = tobj;
                    }
                }

                //Assign ID to the "best" object
                if (maxCertainityRef != null)
                {
                    maxCertainityRef.ObjReference.ObjectID = maxCertainityRef.BestCertainityWithID;
                    maxCertainityRef.ObjReference.TrackingStatus = TrackStatusFilter;            
                    //If the assigned TrackStatus is defined, use it
                    if (AssignedStatus != TableObject.ETrackingStatus.NotTracked)
                        maxCertainityRef.ObjReference.TrackingStatus = AssignedStatus;
                }
            }
        }

        private List<int> CalculateCertainity(ref List<TrackObject> newFrameObjects, TableObject.ETrackingStatus filter)
        {
            //Get only the desired objects
            List<TableObject> recentObjects =
                _historyTrackList.GetLastFrame().ObjectList.Where(obj => obj.TrackingStatus == filter).ToList();

            //Dont track old objects that are already assigned to new ones (ID)
            List<int> AlreadyAssignedIDs = new List<int>();
            foreach (TrackObject obj in newFrameObjects)
            {
                if (obj.ObjReference.ObjectID != 0)
                    AlreadyAssignedIDs.Add(obj.ObjReference.ObjectID);
            }

            //Create a List with each ID that will be used
            List<int> UsedIDs = new List<int>();

            //Calculate certainity for each object that isn't tracked
            foreach (TrackObject to in newFrameObjects.Where(obj =>obj.ObjReference.TrackingStatus == TableObject.ETrackingStatus.NotTracked))
            {
                foreach(TableObject proposedObject in recentObjects)
                {
                    //Compare each object from the last frame 
                    //and calculate the Certainity in % (value from 0 to 1)
                    
                    //distance between objects
                    double distance = proposedObject.Center.DistanceTo(to.ObjReference.Center,TPoint.PointCreationType.depth);
                    
                    //The negative percentage (distance / max distance) is the certainity
                    double certainity = 1 - (distance/MaxDistance);

                    //if this certainity value is better than the saved one, store the new values an the object id for later
                    //but only if the stored object isn't assigned to another object
                    if ((to.TrackCertainity < certainity) && (!AlreadyAssignedIDs.Contains(proposedObject.ObjectID)))
                    {
                        to.TrackCertainity = certainity;
                        to.BestCertainityWithID = proposedObject.ObjectID;
                        if (!UsedIDs.Contains(proposedObject.ObjectID))
                            UsedIDs.Add(proposedObject.ObjectID);
                    }
                }
            }

            return UsedIDs;
        }
    }
}
