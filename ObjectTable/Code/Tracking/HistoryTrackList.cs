using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectTable.Code.Recognition.DataStructures;
using ObjectTable.Code.SettingManagement;

namespace ObjectTable.Code.Tracking
{
    /// <summary>
    /// Saves the tracked objects from the last frames
    /// </summary>
    class HistoryTrackList
    {
        private List<HistoryContainer> _containerList;
        private HistoryContainer _refLastContainer;
        private object _lockCointainers = "";

        public HistoryTrackList()
        {
            _containerList = new List<HistoryContainer>();
        }

        public void AddNew(List<TableObject> ObjectList)
        {
            lock (_lockCointainers)
            {
                HistoryContainer c = new HistoryContainer();
                c.ObjectList = ObjectList;
                AddAndIncreaseAge(c);
            }
        }

        public List<int> GetUsedIDs()
        {
            List<int> result = new List<int>();
            lock (_lockCointainers)
            {
                foreach (HistoryContainer hc in _containerList)
                {
                    foreach (TableObject obj in hc.ObjectList)
                    {
                        result.Add(obj.ObjectID);
                    }
                }
            }

            return result;
        }

        public HistoryContainer Get2ndNewContainer()
        {
            lock (_lockCointainers)
            {
                if (_containerList.Count < 2)
                {
                    return null;
                }
                else
                {
                    return _containerList[_containerList.Count - 2];
                }
            }
        }

        private void AddAndIncreaseAge(HistoryContainer c)
        {
            lock (_lockCointainers)
            {
                //add
                _containerList.Add(c);
                _refLastContainer = c;

                //Too many items? delete the oldest one
                if (_containerList.Count >= SettingsManager.TrackingSet.MaximumHistoryCount)
                    _containerList.RemoveAt(0);

                //Increase the age
                foreach (HistoryContainer hc in _containerList)
                {
                    hc.FrameAge++;
                }
            }
        }

        public HistoryContainer GetLastFrame()
        {
            lock (_lockCointainers)
            {
                return _refLastContainer;
            }
        }

        public int GetGuessedAge(int ObjectID)
        {
            lock (_lockCointainers)
            {
                //Get all containers that contain this ID
                List<HistoryContainer> hcList = _containerList.Where(
                    container => container.ObjectList.Where(obj => obj.ObjectID == ObjectID).Count() > 0).ToList();

                //Now check for how long this ID has been guessed
                int guessedAge = 0;
                int index;
                bool onceseen = false;

                //Inverse check, because we count into the past
                for (index = hcList.Count - 1; index >= 0; index--)
                {
                    //Check whether this object has been guesed


                    List<TableObject> objList = hcList[index].ObjectList;
                    TableObject checkObj = objList.Where(obj => obj.ObjectID == ObjectID).ToList()[0];
                    if (checkObj.TrackingStatus == TableObject.ETrackingStatus.LongTermGuessed)
                    {
                        //Increase age
                        guessedAge++;
                    }
                    else
                    {
                        //The object is seen, so the age counted so far is the result
                        if (onceseen)
                        {
                            return guessedAge;
                        }

                        onceseen = true;
                        guessedAge++;
                    }
                }

                return guessedAge;
            }
        }

        public bool HasFrames()
        {
            if (_containerList.Count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the age of the oldest historyContainer a ceratin object appears in
        /// </summary>
        /// <param name="ObjectID"></param>
        /// <returns></returns>
        public int GetObjectAge(int ObjectID)
        {
            lock (_lockCointainers)
            {
                IEnumerable<HistoryContainer> IObjList =
                    _containerList.Where(
                        container => container.ObjectList.Where(obj => obj.ObjectID == ObjectID).Count() > 0);
                if (IObjList.Count() > 0)
                {
                    List<HistoryContainer> HistConList = IObjList.ToList();
                    return HistConList[0].FrameAge;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
