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
using System.Windows.Shapes;

namespace ProgrammingTable.Code.Simulation.Objects.SimulationObjects
{
    /// <summary>
    /// Interaction logic for ConstantValueObjectWindow.xaml
    /// </summary>
    public partial class ConstantValueObjectWindow : Window
    {
        public object Value = 1;
        public SimulationValue.ESimulationValueType ValueType = SimulationValue.ESimulationValueType.Int;

        public ConstantValueObjectWindow()
        {
            InitializeComponent();
        }

        private void txt_value_KeyDown(object sender, KeyEventArgs e)
        {
            //Check whether conversion is okay
            bool problem = false;

            switch(comboBox1.SelectedIndex)
            {
                case 1: //INteger
                    try
                    {
                        Convert.ToInt32(txt_value.Text);
                    }
                    catch (Exception) { problem = true; }
                    break;
                case 2: //Double
                    try
                    {
                        Convert.ToDouble(txt_value.Text);
                    }
                    catch (Exception) { problem = true; }
                    break;
                case 3:
                    try
                    {
                        Convert.ToBoolean(txt_value.Text);
                    }
                    catch (Exception) { problem = true; }
                    break;
            }

            b_set.IsEnabled = !problem;
        }

        private void b_set_Click(object sender, RoutedEventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: //String
                    Value = txt_value.Text;
                    ValueType = SimulationValue.ESimulationValueType.String;
                    break;
                case 1: //INteger
                    Value = Convert.ToInt32(txt_value.Text);
                    ValueType = SimulationValue.ESimulationValueType.Int;
                    break;
                case 2: //Double
                    Value = Convert.ToDouble(txt_value.Text);
                    ValueType = SimulationValue.ESimulationValueType.Double;
                    break;
                case 3: //Bool
                    Value = Convert.ToBoolean(txt_value.Text);
                    ValueType = SimulationValue.ESimulationValueType.Bool;
                    break;
            }
            this.Close();
        }
    }
}
