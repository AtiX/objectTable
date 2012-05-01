using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ObjectTable.Code.Display.GUI;

namespace ObjectTable.Code.Display
{
    /// <summary>
    /// Manages the Visualisation and Object Management of the VisualObjects on the Screen
    /// </summary>
    public class DisplayManager
    {
        private BeamerDisplayUC _beamerDisplay;

        private object _lockvarUilist = "";
        private List<UIElement> _bottomElements;
        private List<UIElement> _midElements;
        private List<UIElement> _topElements;

        private ImageBrush _oldBackgroundBrush = null;

        public enum DisplayLayer
        {
            bottom,
            middle,
            top
        };

        public DisplayManager(BeamerDisplayUC beamerScreen)
        {
            _beamerDisplay = beamerScreen;

            _bottomElements = new List<UIElement>();
            _midElements = new List<UIElement>();
            _topElements = new List<UIElement>();
        }

        public BeamerDisplayUC GetBeamerScreen()
        {
            return _beamerDisplay;
        }

        public void UpdateBeamerScreen(BeamerDisplayUC beamerScreen)
        {
            _beamerDisplay = beamerScreen;

            //TODO: bug: elements don't reappear!
            //Add all elements to the new screen
            lock (_lockvarUilist)
            {
                foreach (UIElement e in _bottomElements)
                {
                    _beamerDisplay.Dispatcher.Invoke((Action)(() => { _beamerDisplay.canvas_bottom.Children.Add(e); }), null);
                }
                foreach(UIElement e in _midElements)
                {
                    _beamerDisplay.Dispatcher.Invoke((Action)(() => { _beamerDisplay.canvas_mid.Children.Add(e); }), null);
                }
                foreach (UIElement e in _topElements)
                {
                    _beamerDisplay.Dispatcher.Invoke((Action)(() => { _beamerDisplay.canvas_top.Children.Add(e); }), null);
                }
            }
        }

        /// <summary>
        /// Adds an element to the selected Layer
        /// </summary>
        /// <param name="element"></param>
        /// <param name="layer"></param>
        public void AddElement(UIElement element, DisplayLayer layer)
        {
            //Check whether invokation is required
            if (!_beamerDisplay.CheckAccess())
            {
                _beamerDisplay.Dispatcher.BeginInvoke((Action) (() =>
                                                                    {
                                                                        if ((element == null) ||
                                                                            (_beamerDisplay == null))
                                                                            return;
                                                                        switch (layer)
                                                                        {
                                                                            case DisplayLayer.bottom:
                                                                                _beamerDisplay.canvas_bottom.Children.
                                                                                    Add(element);
                                                                                break;
                                                                            case DisplayLayer.middle:
                                                                                _beamerDisplay.canvas_mid.Children.Add(
                                                                                    element);
                                                                                break;
                                                                            case DisplayLayer.top:
                                                                                _beamerDisplay.canvas_top.Children.Add(
                                                                                    element);
                                                                                break;
                                                                        }
                                                                    }), null);
            }
            else
            {
                if ((element == null) || (_beamerDisplay == null))
                    return;
                switch (layer)
                {
                    case DisplayLayer.bottom:
                    _beamerDisplay.canvas_bottom.Children.Add(element);
                        break;
                    case DisplayLayer.middle:
                        _beamerDisplay.canvas_mid.Children.Add(element);
                        break;
                    case DisplayLayer.top:
                        _beamerDisplay.canvas_top.Children.Add(element);
                        break;
                }
            }

            lock (_lockvarUilist)
            {
                switch (layer)
                {
                    case DisplayLayer.bottom:
                        _bottomElements.Add(element);
                        break;
                    case DisplayLayer.middle:
                        _midElements.Add(element);
                        break;
                    case DisplayLayer.top:
                        _topElements.Add(element);
                        break;
                } 
            }

            return;
        }

        /// <summary>
        /// Executes code in the Display thread - ui elements can be modified
        /// </summary>
        /// <param name="modificationMethod"></param>
        /// <param name="methodArgs"></param>
        /// <param name="beginInvoke">if set to false, Invoke instead of BeginInvoke will be used. Use if the action has to be done before continuing with the code</param>
        public void WorkThreadSafe(Delegate modificationMethod, object[] methodArgs, bool beginInvoke = true)
        {
            if (!_beamerDisplay.Dispatcher.CheckAccess())
                if (beginInvoke)
                    _beamerDisplay.Dispatcher.BeginInvoke(modificationMethod, methodArgs);
                else
                    _beamerDisplay.Dispatcher.Invoke(modificationMethod, methodArgs);
            else
                modificationMethod.DynamicInvoke(methodArgs);
        }

        /// <summary>
        /// Safely removes an element from the display
        /// </summary>
        /// <param name="element"></param>
        public void DeleteElement(UIElement element)
        {
            //on which layer? (and delete from lists)
            DisplayLayer layer = DisplayLayer.middle;
            lock (_lockvarUilist)
            {
                if (_bottomElements.Contains(element))
                {
                    layer = DisplayLayer.bottom;
                    _bottomElements.Remove(element);
                }
                else if (_midElements.Contains(element))
                {
                    layer = DisplayLayer.middle;
                    _midElements.Remove(element);
                }
                else if (_topElements.Contains(element))
                {
                    layer = DisplayLayer.top;
                    _topElements.Remove(element);
                }
            }

            bool access = _beamerDisplay.Dispatcher.CheckAccess();

            switch (layer)
            {
                case DisplayLayer.bottom:
                    if (!access) _beamerDisplay.Dispatcher.BeginInvoke((Action)(() => _beamerDisplay.canvas_bottom.Children.Remove(element)), null);
                    else _beamerDisplay.canvas_bottom.Children.Remove(element);
                    break;
                case DisplayLayer.middle:
                    if (!access) _beamerDisplay.Dispatcher.BeginInvoke((Action)(() => _beamerDisplay.canvas_mid.Children.Remove(element)), null);
                    else _beamerDisplay.canvas_mid.Children.Remove(element);
                    break;
                case DisplayLayer.top:
                    if (!access) _beamerDisplay.Dispatcher.BeginInvoke((Action)(() => _beamerDisplay.canvas_top.Children.Remove(element)), null);
                    else _beamerDisplay.canvas_top.Children.Remove(element);
                    break;
            }
        }

        /// <summary>
        /// Removes all elements from the 3 canvas
        /// </summary>
        public void ClearScreen()
        {
            _beamerDisplay.Dispatcher.Invoke((Action)(() => _beamerDisplay.canvas_bottom.Children.Clear()), null);
            _beamerDisplay.Dispatcher.Invoke((Action)(() => _beamerDisplay.canvas_mid.Children.Clear()), null);
            _beamerDisplay.Dispatcher.Invoke((Action)(() => _beamerDisplay.canvas_top.Children.Clear()), null);

            lock (_lockvarUilist)
            {
                _bottomElements.Clear();
                _midElements.Clear();
                _topElements.Clear();
            }
        }

        /// <summary>
        /// Sets the background image. ThreadSafe.
        /// </summary>
        /// <param name="src"></param>
        public void SetBackgroundImage(Bitmap src)
        {
            if (_beamerDisplay.Dispatcher.CheckAccess())
            {
                ImageBrush br = new ImageBrush(src.ToWpfBitmapVar2()); br.Stretch = Stretch.Fill;
                _oldBackgroundBrush = (ImageBrush)_beamerDisplay.grid_bottom.Background;
                _beamerDisplay.grid_bottom.Background = br;
                if (_oldBackgroundBrush != null)
                {
                    _oldBackgroundBrush.ImageSource = null;
                    _oldBackgroundBrush = null;
                }
                return;
            }

            _beamerDisplay.Dispatcher.Invoke((Action)(() =>
                                                               {
                                                                   ImageBrush br = new ImageBrush(src.ToWpfBitmapVar2()); br.Stretch = Stretch.Fill;
                                                                   _oldBackgroundBrush = (ImageBrush)_beamerDisplay.grid_bottom.Background;
                                                                   _beamerDisplay.grid_bottom.Background = br;
                                                                   if (_oldBackgroundBrush != null)
                                                                   {
                                                                       _oldBackgroundBrush.ImageSource = null;
                                                                       _oldBackgroundBrush = null;
                                                                   }
                                                               }));
        }
    }
}
