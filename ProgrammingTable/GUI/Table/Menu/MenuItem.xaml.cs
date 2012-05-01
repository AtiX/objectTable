using System.Windows.Controls;
using ProgrammingTable.Code.Simulation.Menu;

namespace ProgrammingTable.GUI.Table.Menu
{
	/// <summary>
	/// Interaction logic for MenuItem.xaml
	/// </summary>
	public partial class MenuItem : UserControl
	{
		public MenuItem()
		{
			this.InitializeComponent();
		}

        public MenuItem(string name, string category, string shortSign)
        {
            this.InitializeComponent();
            SetInfo(name, category, shortSign);
        }

        public void SetInfo(string name, string category, string shortSign)
        {
            txt_name.Text = name;
            txt_short.Text = shortSign;
            txt_category.Text = category;

            //adjust size if too big
            if (shortSign.Length > 3)
            {
                txt_short.FontSize = 50.0;
            }
            else
            {
                txt_short.FontSize = 73.0;
            }
        }
	}
}