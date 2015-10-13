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

namespace KeyLoggerGui {
    /// <summary>
    /// Interaction logic for TitleLine.xaml
    /// </summary>
    public partial class TitleLine : UserControl {

        public TitleLine() {
            InitializeComponent();
        }

        /// <summary>
        /// Typically you set some text to title, but can be any WPF widget as well
        /// </summary>
        public object Title {
            get { return this.title.Content; }
            set { this.title.Content = value; }
        }
    }
}
