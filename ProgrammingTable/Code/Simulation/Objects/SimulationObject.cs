using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ObjectTable;
using ObjectTable.Code.Recognition.DataStructures;
using ProgrammingTable.Code.Graphics;

namespace ProgrammingTable.Code.Simulation.Objects
{
    /// <summary>
    /// A SimulationObject is equivalent to a certain "brick" on the table
    /// </summary>
    abstract class SimulationObject
    {
        /// <summary>
        /// The name of the Object Class (used for menue)
        /// </summary>
        public string Name;

        /// <summary>
        /// The category/pack of the object class (used for menue)
        /// </summary>
        public string Category;

        /// <summary>
        /// The short sign that is used for the object (used for menue)
        /// </summary>
        public string ShortSign;

        /// <summary>
        /// The last tableObject it was assigned to
        /// </summary>
        public TableObject LastTableObject;

        /// <summary>
        /// is set to true by the SimulationEngine if the TableObject changed since last Tick
        /// </summary>
        public bool TableObjectDidChange;

        /// <summary>
        /// The Layer of the Simulation Object. objects on different layers cant work with each other
        /// </summary>
        public int Layer = 0;

        public delegate void ScreenElementHandler(UIElement e);
        /// <summary>
        /// Raised by the SimulationObject if it wants to add a new ScreenElement to the middle layer on the screen
        /// </summary>
        public abstract event ScreenElementHandler OnNewMiddleScreenElement;
        /// <summary>
        /// Raised by the SimulationObject if it wants to add a new ScreenElement to the top layer on the screen
        /// </summary>
        public abstract event ScreenElementHandler OnNewBottomScreenElement;
        /// <summary>
        /// Raised by the simulationObject before it removes the screenElement
        /// </summary>
        public abstract event ScreenElementHandler OnScreenElementRemove;

        public delegate void ScreenElementModify(Delegate action, object[] args);
        /// <summary>
        /// This event is raised if the simulation object wants to modify one screen element. the delegate is executed in the screeen's thread
        /// </summary>
        public abstract event ScreenElementModify OnScreenElementModify;

        /// <summary>
        /// If set to false, the SimulationEngine wont calculate PossibleDestinationObjects
        /// </summary>
        public bool ShootBeam = true;

        /// <summary>
        /// The simulationObjects the current object may point to - is updated before simulationTick() is called
        /// null if the object doesn't point to any object.
        /// index increases with increasing distance
        /// </summary>
        public List<SimulationObject> PossibleDestinations;

        /// <summary>
        /// If the object has more than one Direction, the possible Destinations are calculated for each additional direction
        /// </summary>
        public List<List<SimulationObject>> PossibleAdditionalDestinations;

        /// <summary>
        /// A simulation object may point to more than one direction. these additional directions are stored in this List
        /// </summary>
        public List<Vector> AdditionalDirections;

        /// <summary>
        /// The destination objects the simulationObject points to. is set by the simulation object.
        /// </summary>
        public List<SimulationObject> Destinations;

        /// <summary>
        /// A list of objects that point to this object. the list is updated before simulationTick() is called
        /// </summary>
        public List<SimulationObject> SourceObjects;

        /// <summary>
        /// Called when the TableObject is assigned and the SimulationObject may initialize itself
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// This method is called by the simulationEngine when a tick occurs. Place ObjectLogic here
        /// </summary>
        public abstract void SimulationTick();

        /// <summary>
        /// This method is called by the SimulationEngine regularly so that GUI-Updates can be performed. Place GUI-Modification/Update-Logic here
        /// </summary>
        public abstract void GfxUpdateTick();

        /// <summary>
        /// This method is called by the destination object of this object to get the objects value
        /// </summary>
        /// <returns></returns>
        public abstract SimulationValue GetValue();

        /// <summary>
        /// The graphics settings of the object - used by the SimObjDrawer to draw circle and screen line(s)
        /// </summary>
        public SimObjGraphicsSettings GraphicsSettings = new SimObjGraphicsSettings();

        /// <summary>
        /// is called before the object is removed from the table
        /// </summary>
        public abstract void Remove();
    }
}
