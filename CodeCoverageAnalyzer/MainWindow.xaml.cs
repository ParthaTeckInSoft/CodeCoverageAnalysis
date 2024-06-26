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
         mCodeCover = new CodeCoverage ();
         mAppTitle = mTitle;
         mFlowDoc = new FlowDocument ();
         this.DataContext = this;
      }
      #endregion

      #region Events
      public event PropertyChangedEventHandler? PropertyChanged;
      #endregion

      #region Properties/Variables
      readonly string mTitle = "Code Coverage Analyzer";
      CodeCoverage mCodeCover;
      public CodeCoverage CodeCover {
         get => mCodeCover;
         set {
            mCodeCover = value;
            OnPropertyChanged (nameof (CodeCover));
         }
      }

      /// <summary>
      /// This property is MVVM bound with the title text of the application frame
      /// </summary>
      string mAppTitle;
      public string AppTitle {
         get => mAppTitle;
         set {
            mAppTitle = value;
            OnPropertyChanged (nameof (AppTitle));
         }
      }

      /// <summary>
      /// This property is MVVM bound with the textbox nect to the 
      /// Recompute button
      /// </summary>
      string? mLoadedSrcFullFilePath;
      public string? LoadedSrcFullFilePath {
         get => mLoadedSrcFullFilePath;
         set {
            mLoadedSrcFullFilePath = value;
            OnPropertyChanged (nameof (LoadedSrcFullFilePath));
         }
      }

      /// <summary>
      /// This property is MVVM bound to the FlowDocument contained by
      /// the FlowDocument Scroll Viewer
      /// </summary>
      FlowDocument mFlowDoc;
      public FlowDocument FlowDoc {
         get => mFlowDoc;
         set {
            mFlowDoc = value;
            OnPropertyChanged (nameof (FlowDoc));
         }
      }
      #endregion

      #region Callbacks
      /// <summary>
      /// This is the method that is called when a MVVM bound property 
      /// value is set
      /// </summary>
      /// <param name="propertyName"></param>
      protected virtual void OnPropertyChanged (string propertyName) {
         PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
      }

      /// <summary>
      /// Handles the File->Open menu item click event
      /// Opens a dialog for the user to select a file to open and loads the selected (coverage XML) file
      /// </summary>
      /// <param name="sender">The source of the event, typically the menu item that was clicked</param>
      /// <param name="e">The event data containing information about the routed event</param>
      void OnFileOpenClick (object sender, RoutedEventArgs e) {
         // Create an instance of OpenFileDialog
         OpenFileDialog openFileDialog = new () {
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
               // Load the coverage document
               CodeCover.LoadXMLDocument (xmlFilename);
               
               // Clear the views
               ClearView ();
               
               // Create the treeview
               CreateTreeView ();
               double percent = 0.0;
               percent = ((double)CodeCover.BlocksCovered / CodeCover.TotalBlocks) * 100.0;
               percent = Math.Round (percent, 2);

               // Set the title with appropriate info
               string title = mTitle;
               string appTitle = string.Format ("{0} : {1} / {2} blocks covered : {3} %", title,
                  CodeCover.BlocksCovered, CodeCover.TotalBlocks, percent);
               AppTitle = appTitle;
            } catch (Exception ex) {
               MessageBox.Show ($"An error occurred while loading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      /// <summary>
      /// Handles the File->Open menu item click event, 
      /// Opens the file or application
      /// </summary>
      /// <param name="sender">The source of the event, typically the menu item that was clicked</param>
      /// <param name="e">The event data containing information about the routed event</param>
      void OnFileCloseClick (object sender, RoutedEventArgs e) => ClearView ();

      /// <summary>
      /// Handles the Recompute button click
      /// Yet to be implemented
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnRecomputeClick (object sender, RoutedEventArgs e) { }
      #endregion

      #region Clearances
      /// <summary>
      /// Method to clear all the views.
      /// </summary>
      void ClearView () {
         treeView.Items.Clear ();
         FlowDoc = new FlowDocument ();
         mTreeNodeMap.Clear ();
         LoadedSrcFullFilePath = "";
         AppTitle = mTitle;
      }
      #endregion

      #region Treeview create/helper methods
      /// <summary>
      /// This is the helper method to find the ancestor of a specific type
      /// </summary>
      /// <typeparam name="T">The type of the object to find</typeparam>
      /// <param name="current">Current object in the hierarchy</param>
      /// <returns></returns>
      T? FindAncestor<T> (DependencyObject current) where T : DependencyObject {
         while (current != null) {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent (current);
         }
         return null;
      }

      /// <summary>
      /// The main method which creates the tree view by calling the method 
      /// AddPathToTreeView and AddOrGetNode. This also sets the attached properties 
      /// to the treeview items.
      /// </summary>
      /// <exception cref="Exception">Throws exceptions if the input is inconsistent</exception>
      void CreateTreeView () {
         treeView.Items.Clear ();
         foreach (var srcFileData in CodeCover.SrcFiles)
            AddPathNodesToTree (srcFileData);
      }

      Dictionary<string, Tuple<TreeViewItem, bool>> mTreeNodeMap = [];

      /// <summary>
      /// This method adds new tree view nodes to the tree if a node is not already added
      /// to the parent. 
      /// </summary>
      /// <param name="path"></param>
      void AddPathNodesToTree (string path) {
         string[] parts = path.Split ('\\');
         string currentPath = string.Empty;

         for (int ii = 0; ii < parts.Length; ii++) {
            currentPath += parts[ii] + (ii < parts.Length - 1 ? @"\" : "");

            if (!mTreeNodeMap.ContainsKey (currentPath)) {
               TreeViewItem newItem = new () { Header = parts[ii] };
               mTreeNodeMap[currentPath] = new Tuple<TreeViewItem, bool> (newItem, ii == parts.Length - 1);
               if ( treeView.Items.Count == 0) treeView.Items.Add (newItem);
               newItem.Tag = currentPath;
               if (ii > 0) // Not the root node
               {  
                  string parentPath = currentPath[..currentPath.LastIndexOf (parts[ii])];
                  mTreeNodeMap[parentPath].Item1.Items.Add (newItem);
               }
            }
         }
         ExplodeTree (GetRootTreeViewItem (treeView));
      }

      /// <summary>
      /// Retrieves the root TreeViewItem from the specified TreeView
      /// </summary>
      /// <param name="treeView">The TreeView control from which to retrieve the root item</param>
      /// <returns>The root TreeViewItem if found; otherwise, null</returns>
      TreeViewItem? GetRootTreeViewItem (TreeView treeView) {
         // Check if the TreeView has items
         if (treeView.Items.Count > 0) {
            // Get the first root item data
            var rootItemData = treeView.Items[0];

            // Retrieve the TreeViewItem for the root data item
            if ( treeView.ItemContainerGenerator.ContainerFromItem (rootItemData) is TreeViewItem rootTreeViewItem ) return rootTreeViewItem;
         }
         return null; // Return null if no root item found
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
      void ExplodeTree (TreeViewItem? treeViewItem) {
         if (treeViewItem == null) return;
         treeViewItem.IsExpanded = true;
         foreach (var item in treeViewItem.Items) {
            if (item is TreeViewItem childItem) ExplodeTree (childItem);
         }
      }

      /// <summary>
      /// Callback method when RMB contextual menu "Explode" is invoked.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnExplodeClick (object sender, RoutedEventArgs e) {
         if (treeView.SelectedItem is TreeViewItem selectedItem)
            ExplodeTree (selectedItem);
      }

      /// <summary>
      /// Callback method when RMB contextual menu "Collapse" is invoked.
      /// </summary>
      /// <param name="sender">The source of the event, typically the TreeView control</param>
      /// <param name="e">The event data containing information about the mouse button event</param>
      void OnCollapseClick (object sender, RoutedEventArgs e) {
         if (treeView.SelectedItem is TreeViewItem selectedItem) CollapseTree (selectedItem);
      }

      /// <summary>
      /// Callback method to handle RMB selection of treeview node either to explode or to collapse
      /// the tree from that selected node. This issues two options. 1. Explode tree, 2. Collapse tree.
      /// </summary>
      /// <param name="sender">The source of the event, typically the TreeView control</param>
      /// <param name="e">The event data containing information about the mouse button event</param>
      void OnRMBOnTreeviewNode (object sender, MouseButtonEventArgs e) {
         if (e.OriginalSource is DependencyObject originalSource) {
            var treeViewItem = FindAncestor<TreeViewItem> (originalSource);
            if (treeViewItem != null) {
               treeViewItem.IsSelected = true;
               e.Handled = true;
            }
         }
      }

      /// <summary>
      /// This is a call back method when a tree node is selected. If the tree node happens to be 
      /// the leaf node, which shall be a source file node, the new document is loaded into FlowDocumentScrollViewer
      /// which internally calls HighlightLine for each of the line in the source file.
      /// </summary>
      /// <param name="sender">The source of the event, typically the TreeView control</param>
      /// <param name="e">The event data containing selected item</param>
      void OnTreeViewItemSelected (object sender, RoutedPropertyChangedEventArgs<object> e) {
         if (e.OriginalSource is DependencyObject) {

            // Retrieve the selected treeview item node object
            var item = (TreeViewItem)e.NewValue;

            // If tree view item is null, return.
            if (item == null) return;
            bool isFile = File.Exists (item.Tag as string);

            // If the treeview item does not contain the file path associated with it, return
            if (!isFile) return;

            // Get the associated file path (from Tag)
            string filePath = item.Tag as string ?? "";

            // If file path exists, load the file into FlowDocumentScrollViewer
            if (!string.IsNullOrEmpty (filePath) && File.Exists (filePath))
               LoadFileIntoDocumentViewer (filePath);

            // Get the blocks covered and not covered to display them
            (int blocksCoveredThisFile, int blocksNotCoveredThisFile) = CodeCover.GetFileBlocksCoverageInfo (filePath);
            int totalBlocks = blocksCoveredThisFile + blocksNotCoveredThisFile;
            double percent = ((double)blocksCoveredThisFile / totalBlocks) * 100.0;
            percent = Math.Round (percent, 2);
            string fileCvrgInfo = string.Format ("{0} : {1} / {2} : blocks  {3} %", filePath,
               blocksCoveredThisFile, totalBlocks,
               (totalBlocks) > 0 ? percent : 0.0); ;
            LoadedSrcFullFilePath = fileCvrgInfo;
         }
      }
      #endregion

      #region Flow Document Scroll Viewer Loading/Highlight methods
      /// <summary>
      /// This method is the entry point to load the memory of 
      /// code coverage, read from LoadXMLDocument() into the UI
      /// </summary>
      /// <param name="filePath"></param>
      public void LoadFileIntoDocumentViewer (string filePath) {
         FlowDocument flowDocument = new () {
            FontFamily = new FontFamily ("Consolas"),
            FontSize = 12
         };
         string[] fileLines = File.ReadAllLines (filePath);
         int lineNumber = -1;
         for (int ii = 0; ii < fileLines.Length; ii++) {
            List<Range> ranges = CodeCover.GetRanges (filePath, ii + 1);
            if (ii + 1 <= lineNumber) continue;
            lineNumber = ii + 1;
            HighlightLine (flowDocument, fileLines, ranges, ref lineNumber);
         }
         FlowDoc = flowDocument;
         OnPropertyChanged (nameof (FlowDoc));
      }

      /// <summary>
      /// This method highlights the a specific block in marked by Ranges list. The block can be of size 1 line, 
      /// if the range marks a single line with multiple start or end columns for coverage/non-coverage OR
      /// a block of lines, having more than 1 line, with start column on the first line and end column on the 
      /// last line, with all the intermediate lines either marked for coverage/non-coverage
      /// </summary>
      /// <param name="flowDocument">The Flowdocument that is to be updated</param>
      /// <param name="line">This is a ref parameter, means, it bears the most recent line number added. The caller
      /// should then look for line numbers line+1 onwards</param>
      /// <param name="ranges">The list of data structure that holds start/end line with start/end column that 
      /// a line or block of lines are either covered or not covered</param>
      void HighlightLine (FlowDocument flowDocument, string[] fileLines, List<Range> ranges,
         ref int lineNumber) {
         // Format the line number (e.g., 1, 2, 3...)
         string lineNumberText = $"{lineNumber:D4}: "; // D4 formats the number to 4 digits (e.g., 0001, 0002)

         // Create the paragraph with the line number
         Paragraph paragraph = new () {
            Margin = new Thickness (0),
            LineHeight = 12
         };

         // Add the line number with a different style if needed
         paragraph.Inlines.Add (new Run (lineNumberText) {
            Foreground = Brushes.Gray, // Line numbers in gray color for differentiation
            FontWeight = FontWeights.Bold
         });

         if (ranges == null || ranges.Count == 0) paragraph.Inlines.Add (new Run (fileLines[lineNumber - 1]));
         else {
            int prevEndColumn = 0;
            ranges = [.. ranges.OrderBy (rng => rng.StartColumn)];
            string afterHighlight;
            List<string> lineBlock = [];
            foreach (Range range in ranges) {
               lineBlock.Clear ();
               int highlightStartColumn = range.StartColumn - 1; // Adjust for 0-based index
               int highlightEndColumn = range.EndColumn - 1; // Adjust for 0-based index
               for (int jj = range.StartLine - 1; jj <= range.EndLine - 1; jj++) lineBlock.Add (fileLines[jj]);
               if (prevEndColumn > highlightEndColumn) continue;
               if (highlightStartColumn < 0) {
                  // If start column is invalid, clear and just add the line
                  paragraph.Inlines.Clear ();
                  lineBlock.ForEach (line => paragraph.Inlines.Add (new Run (line)));
                  return;
               }

               // Choose background color based on coverage
               SolidColorBrush coveredBgndBrush = Brushes.DeepSkyBlue;
               SolidColorBrush notCoveredBgndBrush = Brushes.DarkOrange;

               for (int ii = 0; ii < lineBlock.Count; ii++) {
                  var line = lineBlock[ii];
                  // Range with same line, different columns
                  if (lineBlock.Count == 1 && line.Length >= highlightEndColumn && !string.IsNullOrEmpty (line)) {
                     string beforeHighlight = line[prevEndColumn..highlightStartColumn];
                     string highlightedText = line[highlightStartColumn..highlightEndColumn];
                     prevEndColumn = highlightEndColumn;
                     paragraph.Inlines.Add (new Run (beforeHighlight));
                     paragraph.Inlines.Add (new Run (highlightedText) {
                        Background = range.IsCovered ? coveredBgndBrush : notCoveredBgndBrush
                     });
                  } else { // Range with multiple lines and multiple columns
                     string beforeHighlight;
                     if (ii == 0) { // First line
                        if (!string.IsNullOrEmpty (line)) {
                           beforeHighlight = line[..highlightStartColumn];
                           paragraph.Inlines.Add (new Run (beforeHighlight));
                           string highlightedText = line[highlightStartColumn..];
                           paragraph.Inlines.Add (new Run (highlightedText) {
                              Background = range.IsCovered ? coveredBgndBrush : notCoveredBgndBrush
                           });
                        }
                        paragraph.Inlines.Add (new LineBreak ());
                     } else if (ii == lineBlock.Count - 1) { // Last line
                        lineNumberText = $"{++lineNumber:D4}: ";
                        paragraph.Inlines.Add (new Run (lineNumberText) {
                           Foreground = Brushes.Gray,
                           FontWeight = FontWeights.Bold
                        });
                        if (!string.IsNullOrEmpty (line)) {
                           string highlightedText = line[..highlightEndColumn];
                           paragraph.Inlines.Add (new Run (highlightedText) {
                              Background = range.IsCovered ? coveredBgndBrush : notCoveredBgndBrush
                           });
                           afterHighlight = line[highlightEndColumn..];
                           paragraph.Inlines.Add (new Run (afterHighlight));
                        }
                     } else { // Intermediate lines
                        lineNumberText = $"{++lineNumber:D4}: ";
                        paragraph.Inlines.Add (new Run (lineNumberText) {
                           Foreground = Brushes.Gray,
                           FontWeight = FontWeights.Bold
                        });
                        paragraph.Inlines.Add (new Run (line) {
                           Background = range.IsCovered ? coveredBgndBrush : notCoveredBgndBrush
                        });
                        paragraph.Inlines.Add (new LineBreak ());
                     }
                  }
               }
            }
            if (lineBlock.Count == 1) {
               // Add remaining text after the last highlight
               afterHighlight = lineBlock[0][prevEndColumn..];
               paragraph.Inlines.Add (new Run (afterHighlight));
            }
         }

         // Add the paragraph to the FlowDocument
         flowDocument.Blocks.Add (paragraph);
      }
      #endregion
   }
}