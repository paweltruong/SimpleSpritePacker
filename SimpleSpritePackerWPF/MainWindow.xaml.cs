using SimpleSpritePacker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleSpritePackerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //// Create the interop host control.
            //WindowsFormsHost host = new WindowsFormsHost();

            //// Create the MaskedTextBox control.
            //MaskedTextBox mtbDate = new MaskedTextBox("00/00/0000");

            //// Assign the MaskedTextBox control as the host control's child.
            //host.Child = mtbDate;

            //// Add the interop host control to the Grid
            //// control's collection of child controls.
            //this.grid1.Children.Add(host);
            MainForm form = new MainForm();
            WindowInteropHelper wih = new WindowInteropHelper(this);
            wih.Owner = form.Handle;
            form.ShowDialog();
        }
    }
}
