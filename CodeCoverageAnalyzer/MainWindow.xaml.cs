using System.Windows;
using System.Windows.Controls;

namespace CoverageAnalyzer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window { 

   #region Constructor(s)
   public MainWindow () {
      InitializeComponent ();
      mViewModel = new (this);
      this.DataContext = mViewModel;
   }
   #endregion

   #region Members/Properties
   MainViewModel mViewModel;
   public TreeView TreeView { get { return treeView; } }
   #endregion
}