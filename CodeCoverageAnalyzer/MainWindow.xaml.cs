using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using System.Xml;
using System.ComponentModel;

namespace CoverageAnalyzer {
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged {

      #region Constrcutor(s)
      public MainWindow () {
         InitializeComponent ();
         CodeCover = new CodeCoverage ();
         this.DataContext = this;
      }
      #endregion

      #region Events
      public event PropertyChangedEventHandler? PropertyChanged;
      #endregion

      #region Properties/Variables
      CodeCoverage? mCodeCover;
      public CodeCoverage? CodeCover {
         get => mCodeCover;
         set {
            mCodeCover = value;
            OnPropertyChanged (nameof (CodeCover));
         }
      }

      List<SrcDetail> mSrcDetails = new List<SrcDetail> ();
      
      string? mLoadedSrcFullFilePath;
      public string? LoadedSrcFullFilePath {
         get => mLoadedSrcFullFilePath;
         set {
            mLoadedSrcFullFilePath = value;
            OnPropertyChanged (nameof (LoadedSrcFullFilePath));
         }
      }
      #endregion

      #region XML loading methods(s)
      /// <summary>
      /// This method loads the code coverage result XML file, 
      /// which is output from dotnet-coverage tool
      /// </summary>
      /// <param name="xmlFilename">The full file path name of the coverage.cobertura.xml</param>
      void LoadXMLDocument (string xmlFilename) {
         mSrcDetails = new List<SrcDetail> ();
         try {
            // Read and process the XML file
            XmlDocument coverageCoberturaXmlDoc = new XmlDocument ();
            coverageCoberturaXmlDoc.Load (xmlFilename);

            // Get the root element
            XmlElement? xmlRoot = coverageCoberturaXmlDoc.DocumentElement;
            if (xmlRoot == null) return;

            // Create a TreeView to display the nodes
            treeView.Items.Clear ();
            FlowDocument flowdoc = new FlowDocument ();
            string[] srcFileNames = [];
            // Get all <source> elements
            XmlNodeList? sourceNodes = xmlRoot.SelectNodes ("/coverage/sources/source");
            if (sourceNodes == null) return;

            foreach (XmlNode sourceNode in sourceNodes) {
               if (sourceNode == null || string.IsNullOrEmpty (sourceNode.InnerText)) continue;
               // Extract the last folder name from the path

               string? folderName = System.IO.Path.GetFileName (System.IO.Path.GetDirectoryName (sourceNode.InnerText));
               // Find the corresponding package name (which is the same as the folder name)
               XmlNode? projectNode = xmlRoot.SelectSingleNode ($"/coverage/packages/package[@name='{folderName}']");
               if (projectNode == null || projectNode.Attributes == null) continue;

               // Create a project node
               var projectTreeNode = new TreeViewItem { Header = $"Project: {folderName}" };
               SetIsFile (projectTreeNode, false);

               string? relFilename = "";
               // Get all <class> elements within the package
               XmlNodeList? classNodes = projectNode.SelectNodes ("classes/class");
               if (classNodes == null) continue;

               foreach (XmlNode classNode in classNodes) {
                  if (classNode.Attributes == null) continue;
                  TreeViewItem? srcFileNode = null;
                  if (classNode.Attributes["filename"] == null) continue;
                  XmlAttribute? fullFileNameAttr = classNode.Attributes["filename"];
                  relFilename = fullFileNameAttr?.InnerText;
                  string[]? tokens = relFilename?.Split (new char[] { '\\' });
                  if (tokens == null || tokens.Length == 0) continue;
                  bool firstTime = true;
                  TreeViewItem? parentNode = null;

                  foreach (string? token in tokens) {
                     TreeViewItem folderFileTreeNode = new TreeViewItem { Header = $"{token}" };
                     if (firstTime) {
                        projectTreeNode.Items.Add (folderFileTreeNode);
                        firstTime = false;
                     } else if (parentNode != null) parentNode.Items.Add (folderFileTreeNode);

                     srcFileNode = folderFileTreeNode;
                     if (srcFileNode != null) SetIsFile (srcFileNode, false);
                     parentNode = folderFileTreeNode;
                  }
                  if (srcFileNode != null) {
                     SetIsFile (srcFileNode, true);
                     SetFilePath (srcFileNode, sourceNode.InnerText + relFilename);
                  }

                  if (classNode.Attributes["name"] == null) continue;
                  // Create a class node
                  XmlAttribute? classNodeAttrbute4Name = classNode.Attributes["name"];
                  string className = classNodeAttrbute4Name?.Value ?? string.Empty;

                  TreeViewItem classTreeNode = new TreeViewItem { Header = $"Class: {className}" };
                  if (srcFileNode != null) srcFileNode.Items.Add (classTreeNode);
                  SetIsFile (classTreeNode, true);
                  SetFilePath (classTreeNode, sourceNode.InnerText + relFilename);

                  // Get all <method> elements within the class
                  XmlNodeList? methodNodes = classNode.SelectNodes ("methods/method");
                  if (methodNodes == null || methodNodes.Count == 0) continue;
                  foreach (XmlNode methodNode in methodNodes) {
                     if (methodNode.Attributes == null) continue;
                     // Create a method node
                     XmlAttribute? methodNodeAttribute4Name = methodNode.Attributes["name"];
                     string methodName = methodNodeAttribute4Name?.Value ?? string.Empty;
                     TreeViewItem methodTreeNode = new TreeViewItem { Header = $"Method: {methodName}" };
                     classTreeNode.Items.Add (methodTreeNode);
                     SetIsFile (methodTreeNode, true);
                     SetFilePath (methodTreeNode, sourceNode.InnerText + relFilename);

                     // Parse lines within the method
                     XmlNodeList? lineNodes = methodNode.SelectNodes ("lines/line");
                     if (lineNodes == null || lineNodes.Count == 0) continue;
                     foreach (XmlNode lineNode in lineNodes) {
                        if (lineNode.Attributes == null) continue;
                        // Create a line node
                        XmlAttribute? lineNodeAttribute4Number = lineNode.Attributes["number"];
                        string lineNumber = lineNodeAttribute4Number?.Value ?? string.Empty;
                        int number = int.Parse (lineNumber);

                        XmlAttribute? lineNodeAttribute4Hits = lineNode.Attributes["hits"];
                        string hitsStr = lineNodeAttribute4Hits?.Value ?? string.Empty;
                        int hits = int.Parse (hitsStr);

                        // Add SrcFile attributes
                        SrcDetail xmlSrcDetail = new SrcDetail ();
                        xmlSrcDetail.FullFilePath = sourceNode.InnerText + relFilename;
                        xmlSrcDetail.MethodName = methodName;
                        xmlSrcDetail.ClassName = className;
                        xmlSrcDetail.SetLineDetails (number, hits);
                        mSrcDetails?.Add (xmlSrcDetail);
                     }
                  }
               }
               // Add the package node to the TreeView
               treeView.Items.Add (projectTreeNode);
            }
         } catch (Exception) {
            throw;
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

      public static bool GetIsFile (DependencyObject obj) {
         return (bool)obj.GetValue (IsFileProperty);
      }

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

      public static string GetFilePath (DependencyObject obj) {
         return (string)obj.GetValue (FilePathProperty);
      }

      public static void SetFilePath (DependencyObject obj, string value) {
         obj.SetValue (FilePathProperty, value);
      }
      #endregion

      #region Callbacks
      protected virtual void OnPropertyChanged (string propertyName) {
         PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
      }
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
               LoadXMLDocument (xmlFilename);
            } catch (Exception ex) {
               MessageBox.Show ($"An error occurred while loading the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }
      /// <summary>
      /// Callback when double click event is observed on the TreeView node(s)
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void OnMouseDoubleClickOnTreeViewNode (object sender, MouseButtonEventArgs e) {
         if (e.OriginalSource is DependencyObject originalSource) {
            TreeViewItem? item = FindAncestor<TreeViewItem> (originalSource);

            if (item == null) return;
            bool isFile = GetIsFile (item);

            if (!isFile) return;
            LoadedSrcFullFilePath = GetFilePath (item);

            if (!string.IsNullOrEmpty (LoadedSrcFullFilePath) && File.Exists (LoadedSrcFullFilePath)) {
               CodeCoverage cvrg = new CodeCoverage ();
               cvrg.SrcDetails = mSrcDetails;
               cvrg.LoadFileIntoDocumentViewer (LoadedSrcFullFilePath);
               CodeCover = cvrg;
            }
         }
      }
      private void OnFileCloseClick (object sender, RoutedEventArgs e) {
         treeView.Items.Clear ();
         if (CodeCover != null) CodeCover.FlowDoc = new FlowDocument (); ;
      }

      private void OnRecomputeClick (object sender, RoutedEventArgs e) {

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
   }
}