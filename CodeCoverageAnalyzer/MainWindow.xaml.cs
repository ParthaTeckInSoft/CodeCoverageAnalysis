using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;

namespace CoverageAnalyzer {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged {

      #region Constructor(s)
      public MainWindow () {
         InitializeComponent ();
         CodeCover = new CodeCoverage ();
         this.DataContext = this;
         mAppTitle = mTitle;
         AppTitle = mAppTitle;
      }
      #endregion

      #region Events
      public event PropertyChangedEventHandler? PropertyChanged;
      #endregion

      #region Properties/Variables
      readonly string mTitle = "Code Coverage Analyzer";
      CodeCoverage? mCodeCover;
      public CodeCoverage? CodeCover {
         get => mCodeCover;
         set {
            mCodeCover = value;
            OnPropertyChanged (nameof (CodeCover));
         }
      }
      string mAppTitle;
      public string AppTitle {
         get => mAppTitle;
         set {
            if (mAppTitle != value)
               mAppTitle = value;
            OnPropertyChanged (nameof (AppTitle));
         }
      }
      List<Range> mSrcDetails = new List<Range> ();

      string? mLoadedSrcFullFilePath;
      public string? LoadedSrcFullFilePath {
         get => mLoadedSrcFullFilePath;
         set {
            mLoadedSrcFullFilePath = value;
            OnPropertyChanged (nameof (LoadedSrcFullFilePath));
         }
      }
      #endregion

      #region Attached Properties
      public static readonly DependencyProperty IsFileProperty =
            DependencyProperty.RegisterAttached (
                "IsFile",
                typeof (bool),
                typeof (MainWindow),
                new PropertyMetadata (false)
            );

      /// <summary>
      /// Method to query if the object has an attached property that 
      /// contains the file path (string)
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public static bool GetIsFile (DependencyObject obj) {
         return (bool)obj.GetValue (IsFileProperty);
      }

      /// <summary>
      /// Method to set the object with attached property with 
      /// boolean true is this node is a file (leaf) (string)
      /// </summary>
      /// <param name="obj"></param>
      /// <param name="value"></param>
      public static void SetIsFile (DependencyObject obj, bool value) {
         obj.SetValue (IsFileProperty, value);
      }

      // Attached property for FilePath
      public static readonly DependencyProperty FilePathProperty =
          DependencyProperty.RegisterAttached (
              "FilePath",
              typeof (string),
              typeof (MainWindow),
              new PropertyMetadata (string.Empty)
          );

      /// <summary>
      /// Method to get the treeview item with the full file path as
      /// attached property (string)
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public static string GetFilePath (DependencyObject obj) {
         return (string)obj.GetValue (FilePathProperty);
      }

      /// <summary>
      /// Method to set the treeview item with the full file path as
      /// attached property (string)</summary>
      /// <param name="obj"></param>
      /// <param name="value"></param>
      public static void SetFilePath (DependencyObject obj, string value) {
         obj.SetValue (FilePathProperty, value);
      }
      #endregion

      #region Callbacks
      protected virtual void OnPropertyChanged (string propertyName) {
         PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
      }

      /// <summary>
      /// This call back method, loads the code coverage XML document and creates the 
      /// treeview. 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnFileOpenClick (object sender, RoutedEventArgs e) {
         // Create an instance of OpenFileDialog
         OpenFileDialog openFileDialog = new OpenFileDialog {
            Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
            Title = "Open XML File",
         };

         // Show the dialog and get the result
         bool? result = openFileDialog.ShowDialog ();

         // Process the selected file
         if (result.HasValue && result == true) {
            string xmlFilename = openFileDialog.FileName;
            if (string.IsNullOrEmpty (xmlFilename))
               return;
            try {
               if (CodeCover != null) CodeCover.LoadXMLDocument (xmlFilename);
               ClearView ();
               CreateTreeView ();
               double percent = 0.0;
               int? blocksCovered = 0;
               int? totalBlocks = 0;
               if (CodeCover != null) {
                  if (CodeCover?.BlocksCovered != null && CodeCover?.TotalBlocks != null) {
                     blocksCovered = CodeCover?.BlocksCovered;
                     totalBlocks = CodeCover?.TotalBlocks;
                     if (blocksCovered != null && totalBlocks != null) {
                        percent = ((double)blocksCovered.Value / totalBlocks.Value) * 100.0;
                        percent = Math.Round (percent, 2);
                     }
                  }
               }

               // Set the title with appropriate info
               string title = "Code Coverage Analyzer: ";
               string appTitle = string.Format ("{0} : {1} / {2} blocks covered : {3} %", title,
                  CodeCover?.BlocksCovered, CodeCover?.TotalBlocks, percent);
               AppTitle = appTitle;
            } catch (Exception ex) {
               MessageBox.Show ($"An error occurred while loading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      /// <summary>
      /// This method creates the tree view on the left side of app window
      /// by reading from the data from CodeCoverage data structure.
      /// </summary>
      /// <exception cref="Exception"></exception>
      
      /// <summary>
      /// This callback method loads the source file corresponding to the coverage data
      /// into the FlowDocumentScrollViewer by calling CodeCoverage.LoadFileIntoDocumentViewer()
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseDoubleClickOnTreeViewNode (object sender, MouseButtonEventArgs e) {
         if (e.OriginalSource is DependencyObject originalSource) {
            TreeViewItem? item = FindAncestor<TreeViewItem> (originalSource);

            if (item == null) return;
            bool isFile = GetIsFile (item);
            if (!isFile) return;
            string filePath = GetFilePath (item);
            if (!string.IsNullOrEmpty (filePath) && File.Exists (filePath))
               CodeCover?.LoadFileIntoDocumentViewer (filePath);

            int blocksCoveredThisFile = 0, blocksNotCoveredThisFile = 0;
            if (CodeCover != null) (blocksCoveredThisFile, blocksNotCoveredThisFile) = CodeCover.GetBlocksCoverageInfo (filePath);
            int totalBlocks = blocksCoveredThisFile + blocksNotCoveredThisFile;
            double percent = ((double)blocksCoveredThisFile / totalBlocks) * 100.0;
            percent = Math.Round (percent, 2);
            string fileCvrgInfo = string.Format ("{0} : {1} / {2} : blocks  {3} %", filePath,
               blocksCoveredThisFile, totalBlocks,
               (totalBlocks) > 0 ? percent : 0.0); ;
            LoadedSrcFullFilePath = fileCvrgInfo;
         }
      }

      /// <summary>
      /// Callback method on the event of File menu "Close" is invoked
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnFileCloseClick (object sender, RoutedEventArgs e) => ClearView ();

      private void OnRecomputeClick (object sender, RoutedEventArgs e) {}

      /// <summary>
      /// Callback method when RMB contextual menu "Explode" is invoked.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnExplodeClick (object sender, RoutedEventArgs e) {
         if (treeView.SelectedItem is TreeViewItem selectedItem)
            ExplodeTree (selectedItem);
      }

      /// <summary>
      /// Callback method when RMB contextual menu "Collapse" is invoked.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnCollapseClick (object sender, RoutedEventArgs e) {
         if (treeView.SelectedItem is TreeViewItem selectedItem) {
            CollapseTree (selectedItem);
         }
      }

      /// <summary>
      /// Callback method to handle RMB selection of treeview node
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnRMBOnTreeviewNode (object sender, MouseButtonEventArgs e) {
         if (e.OriginalSource is DependencyObject originalSource) {
            var treeViewItem = FindAncestor<TreeViewItem> (originalSource);
            if (treeViewItem != null) {
               treeViewItem.IsSelected = true;
               e.Handled = true;
            }
         }
      }
      #endregion

      #region Helper methods
      /// <summary>
      /// This is the helper method to find the ancestor of a specific type
      /// </summary>
      /// <typeparam name="T">The type of the object to find</typeparam>
      /// <param name="current">Current object in the hierarchy</param>
      /// <returns></returns>
      private T? FindAncestor<T> (DependencyObject current) where T : DependencyObject {
         while (current != null) {
            if (current is T) return (T)current;
            current = VisualTreeHelper.GetParent (current);
         }
         return null;
      }
      #endregion

      #region Action Methods
      /// <summary>
      /// This method either adds a new node to the parent node with the given header
      /// OR returns the existing node with the same name
      /// </summary>
      /// <param name="parent">Parent to which the new or existing node with HEADER</param>
      /// <param name="header">Uniquely identifying the nodes by HEADER</param>
      /// <returns></returns>
      private static TreeViewItem AddOrGetNode (ItemsControl parent, string header) {
         foreach (var item in parent.Items) {
            TreeViewItem? currentItem = item as TreeViewItem;
            if (currentItem != null &&
               string.Compare (currentItem.Header.ToString (), header, StringComparison.OrdinalIgnoreCase) == 0)
               return currentItem;
         }
         // Node does not exist, create and add a new node
         TreeViewItem newNode = new TreeViewItem () { Header = header };
         parent.Items.Add (newNode);
         return newNode;
      }

      /// <summary>
      /// This method adds the treeview node with folders and file name
      /// provided in the full file path.
      /// </summary>
      /// <param name="treeView">The tree view which should be added with new nodes for folders/file</param>
      /// <param name="path">The full file path</param>
      /// <returns></returns>
      /// <exception cref="ArgumentException"></exception>
      public TreeViewItem? AddPathToTreeView (TreeView treeView, string path) {
         string? directoryPath = Path.GetDirectoryName (path);
         if (directoryPath == null) throw new ArgumentException ("Invalid path provided.", nameof (path));
         string[] pathSegments = directoryPath.Split (Path.DirectorySeparatorChar);
         TreeViewItem? currentNode = null;
         TreeViewItem? parentNode = null;

         foreach (string segment in pathSegments) {
            if (currentNode == null) currentNode = AddOrGetNode (treeView, segment);
            else currentNode = AddOrGetNode (currentNode, segment);
            SetIsFile (currentNode, false);
            parentNode = currentNode;
         }
         return parentNode;
      }

      /// <summary>
      /// Method to clear all the views.
      /// </summary>
      void ClearView () {
         treeView.Items.Clear ();
         if (CodeCover != null) CodeCover.FlowDoc = new FlowDocument ();
         LoadedSrcFullFilePath = "";
         AppTitle = mTitle;
      }

      /// <summary>
      /// The main method which creates the tree view by calling the method 
      /// AddPathToTreeView and AddOrGetNode. This also sets the attached properties 
      /// to the treeview items.
      /// </summary>
      /// <exception cref="Exception">Throws exceptions if the input is inconsistent</exception>
      void CreateTreeView () {
         treeView.Items.Clear ();
         if (CodeCover == null) return;
         if (CodeCover.SrcFiles == null) return;
         // Create the folder view of the source files.
         foreach (var srcFileData in CodeCover.SrcFiles) {
            if (srcFileData == null) continue;
            var (id, fullFilePath) = srcFileData;
            TreeViewItem? parentNode = AddPathToTreeView (treeView, fullFilePath);
            if (parentNode == null) throw new Exception ("Creation of path nodes failed");
            string? srcFileName = System.IO.Path.GetFileName (fullFilePath);
            if (string.IsNullOrEmpty (srcFileName)) throw new Exception ("Source file name is null or empty");
            TreeViewItem? srcFileTreeViewItem = AddOrGetNode (parentNode, srcFileName);

            SetIsFile (srcFileTreeViewItem, true);
            SetFilePath (srcFileTreeViewItem, fullFilePath);
         }
      }

      /// <summary>
      /// Method to Collapse the tree view
      /// </summary>
      /// <param name="treeViewItem"></param>
      void CollapseTree (TreeViewItem treeViewItem) {
         treeViewItem.IsExpanded = false;
         foreach (var item in treeViewItem.Items) {
            if (item is TreeViewItem childItem) CollapseTree (childItem);
         }
      }

      /// <summary>
      /// Method to expand tree view
      /// </summary>
      /// <param name="treeViewItem"></param>
      void ExplodeTree (TreeViewItem treeViewItem) {
         treeViewItem.IsExpanded = true;
         foreach (var item in treeViewItem.Items) {
            if (item is TreeViewItem childItem) ExplodeTree (childItem);
         }
      }
      #endregion
   }
}