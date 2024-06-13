using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Xml;
using System.Windows.Shapes;

namespace CoverageAnalyzer;

public class CodeCoverage : INotifyPropertyChanged {
   #region Constructor(s)
   public CodeCoverage () {
      mFlowDoc = new FlowDocument ();
      mRanges = new List<Range> ();
      mSrcFiles = new List<Tuple<int, string>> ();
      mFunctions = new List<Function> ();
      mModules = new List<Module> ();
   }
   #endregion

   #region Properties and specific setters and getters
   List<Range>? mRanges;
   public List<Range>? Ranges {
      get => mRanges ??= new List<Range> ();
   }
   public void AddRange (int sourceId, int startLine, int endLine, int startColumn, int endColumn, bool isCovered) {
      var index = mRanges?.FindIndex (it => (it.SourceId == sourceId && it.StartLine == startLine && 
      it.EndLine == endLine && it.StartColumn == startColumn && it.EndColumn == endColumn && it.IsCovered == isCovered));
      if (index < 0) mRanges?.Add (new Range (sourceId, startLine, endLine, startColumn, endColumn, isCovered));
   }

   List<Tuple<int, string>>? mSrcFiles;
   public List<Tuple<int, string>>? SrcFiles {
      get => mSrcFiles ??= new List<Tuple<int, string>> ();
   }
   public void AddSrcFile (int sId, string srcFilename) {
      var index = mSrcFiles?.FindIndex (it => it.Item1 == sId && it.Item2 == srcFilename);
      if (index < 0) mSrcFiles?.Add (new Tuple<int, string> (sId, srcFilename));
   }
   public Tuple<int,int> GetBlocksCoverageInfo(string filepath) {
      int? index = mSrcFiles?.FindIndex (it => it.Item2 == filepath);
      int blocksCovered = 0, blocksNotCovered = 0;
      if (index .HasValue && index >= 0 ) {
         var functions = mFunctions?.Where( fun => fun.SourceId == index.Value ).ToList ();
         foreach( var fun in functions??new List<Function>()) {
            blocksCovered += fun.BlocksCovered;
            blocksNotCovered += fun.BlocksNotCovered;
         }
      }
      return new Tuple<int,int> (blocksCovered, blocksNotCovered);
   }
   List<Module>? mModules;
   public List<Module>? Modules {
      get => mModules ??= new List<Module> ();
   }
   public void AddModule (string moduleId, string moduleName, string modulePath, double blkCvrg, double lnCvrg,
      int blkCovered, int blksNotCovered, int linesCovered, int linesPrtllyCovered, int linesNotCovered) {
      var index = mModules?.FindIndex (it => it.Id == moduleId && it.Name == moduleName && it.Path == modulePath &&
      it.BlockCoverage == blkCvrg && it.BlocksNotCovered == blksNotCovered && it.LinesCovered == linesCovered &&
      it.LinesPartiallyCovered == linesPrtllyCovered && it.LinesNotCovered == linesNotCovered);
      if (index < 0) mModules?.Add (new Module (moduleId, moduleName, modulePath, blkCvrg, lnCvrg,
         blkCovered, blksNotCovered, linesCovered, linesPrtllyCovered, linesNotCovered));
   }

   List<Function>? mFunctions;
   public List<Function>? Functions {
      get => mFunctions ??= new List<Function> ();
   }
   public void SetSourceIdToLastFunction (int sId) {
      Function func;
      if (mFunctions != null) {
         func = mFunctions[^1];
         func.SourceId = sId;
         mFunctions[^1] = func;
      }
   }
   public void AddFunction (int functionId, string funName, string nspace, string classname, double blockCvrg,
      double lineCvrg, int blocksCvrd, int blocksNotCvrd, int linesCvrd,
      int linesPartiallyCvrd, int linesNotCvrd) {
      var index = mFunctions?.FindIndex (it => it.FunctionId == functionId && it.FunctionName == funName &&
      it.Namespace == nspace && it.Classname == classname && it.BlockCoverage == blockCvrg && it.LineCoverage == lineCvrg &&
      it.BlocksCovered == blocksCvrd && it.BlocksNotCovered == blocksNotCvrd && it.LinesCovered == linesCvrd &&
      it.LinesPartiallyCovered == linesPartiallyCvrd && it.LinesNotCovered == linesNotCvrd);
      if (index < 0) mFunctions?.Add (new Function (functionId, funName, nspace, classname, blockCvrg, lineCvrg, blocksCvrd,
         blocksNotCvrd, linesCvrd, linesPartiallyCvrd, linesNotCvrd));
   }
   int GetSourceId(string fullFilePath) {
      var srcFileEntry = mSrcFiles?.FirstOrDefault (it => it.Item2 == fullFilePath);
      if (srcFileEntry != null) return srcFileEntry.Item1;
      else return -1;
   }
   Range? GetRangeEntry(string fullFilePath, int lineNo) {
      var range = mRanges?.FirstOrDefault(it=>it.SourceId == GetSourceId(fullFilePath) && it.StartLine == lineNo);
      // Check if the range is the default value for the Range struct
      if (range.Equals (default (Range)) && range?.StartLine == 0) // Confirming no default matching range
      {
         // Return null if the default value is found (indicating no match)
         return null;
      }
      return range;
   }
   List<Range>? GetAllRanges (string fullFilePath, int lineNo) {
      var ranges = mRanges?.Where (it => it.SourceId == GetSourceId (fullFilePath) && it.StartLine == lineNo).ToList();
      return ranges?? new List<Range>();
      
   }
   FlowDocument mFlowDoc;
   public FlowDocument FlowDoc {
      get => mFlowDoc;
      set {
         mFlowDoc = value;
         OnPropertyChanged (nameof (FlowDoc));
      }
   }

   public int TotalBlocks { get; set; }
   public int BlocksCovered {  get; set; }
   #endregion

   #region Clearances
   public void ClearAll () {
      mFunctions?.Clear ();
      mModules?.Clear ();
      mSrcFiles?.Clear ();
      mRanges?.Clear ();
   }
   #endregion

   #region Events
   public event PropertyChangedEventHandler? PropertyChanged;
   #endregion

   #region Callbacks(s)
   protected virtual void OnPropertyChanged (string propertyName) => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
   #endregion

   #region XML read methods
   /// <summary>
   /// This method is the entry point to load the memory of 
   /// code coverage, read from LoadXMLDocument() into the UI
   /// </summary>
   /// <param name="filePath"></param>
   public void LoadFileIntoDocumentViewer (string filePath) {
      FlowDocument flowDocument = new FlowDocument {
         FontFamily = new FontFamily ("Consolas"),
         FontSize = 12
      };
      string[] fileLines = File.ReadAllLines (filePath);
      for (int ii = 0; ii < fileLines.Length; ii++) {
         string line = fileLines[ii];
         List<Range>? ranges = GetAllRanges (filePath, ii + 1);
         HighlightLine (flowDocument, line, ranges??(new List<Range>()), ii+1);
      }
      FlowDoc = flowDocument; 
      OnPropertyChanged (nameof (FlowDoc));
   }

   /// <summary>
   /// This method highlights the a specific block in line marked by Ranges list
   /// </summary>
   /// <param name="flowDocument"></param>
   /// <param name="line"></param>
   /// <param name="ranges"></param>
   void HighlightLine (FlowDocument flowDocument, string line, List<Range> ranges, int lineNumber) {
      // Format the line number (e.g., 1, 2, 3...)
      string lineNumberText = $"{lineNumber:D4}\t: "; // D4 formats the number to 4 digits (e.g., 0001, 0002)

      // Create the paragraph with the line number
      Paragraph paragraph = new Paragraph () {
         Margin = new Thickness (0),
         LineHeight = 12
      };

      // Add the line number with a different style if needed
      paragraph.Inlines.Add (new Run (lineNumberText) {
         Foreground = Brushes.Black, // Line numbers in gray color for differentiation
         FontWeight = FontWeights.Bold
      });

      if (ranges == null || !ranges.Any ()) {
         // If no ranges are provided, just add the line text with line number
         paragraph.Inlines.Add (new Run (line));
      } else {
         int prevEndColumn = 0;
         ranges = ranges.OrderBy (rng => rng.StartColumn).ToList ();

         foreach (Range range in ranges) {
            int highlightStartColumn = range.StartColumn - 1; // Adjust for 0-based index
            int highlightEndColumn = range.EndColumn - 1; // Adjust for 0-based index

            if (prevEndColumn > highlightEndColumn) continue;

            bool isCovered = range.IsCovered;

            if (highlightStartColumn < 0) {
               // If start column is invalid, clear and just add the line
               paragraph.Inlines.Clear ();
               paragraph.Inlines.Add (new Run (line));
               return;
            } else if (line.Length >= highlightEndColumn) {
               // Before highlighting
               string beforeHighlight = line.Substring (prevEndColumn, highlightStartColumn - prevEndColumn);

               // Highlighted text
               string highlightedText = line.Substring (highlightStartColumn, highlightEndColumn - highlightStartColumn);
               prevEndColumn = highlightEndColumn;

               // Choose background color based on coverage
               Color backgroundColor = isCovered ? Color.FromRgb (0, 165, 240) : Color.FromRgb (255, 165, 0);

               // Add text segments to the paragraph
               paragraph.Inlines.Add (new Run (beforeHighlight));
               paragraph.Inlines.Add (new Run (highlightedText) {
                  Background = new SolidColorBrush (backgroundColor)
               });
            }
         }

         // Add remaining text after the last highlight
         string afterHighlight = line.Substring (prevEndColumn);
         paragraph.Inlines.Add (new Run (afterHighlight));
      }

      // Add the paragraph to the FlowDocument
      flowDocument.Blocks.Add (paragraph);
   }


   /// <summary>
   /// This method loads the coverage.xml document to memory
   /// </summary>
   /// <param name="xmlFilename">Filepath to the xml document</param>
   /// <exception cref="Exception">This method throws exception if the XML coverage 
   /// dobument misses the required elements or attributes</exception>
   public void LoadXMLDocument (string xmlFilename) {
      ClearAll ();

      // Read and process the XML file
      XmlDocument coverageXmlDoc = new XmlDocument ();
      coverageXmlDoc.Load (xmlFilename);
      // Get the root element
      XmlElement? xmlRoot = coverageXmlDoc.DocumentElement;
      if (xmlRoot == null) return;
      FlowDocument flowdoc = new FlowDocument ();
      //string[] srcFileNames = [];

      // Read all <Module> information
      XmlNodeList? moduleNodes = coverageXmlDoc.SelectNodes ("//modules/module");
      if (moduleNodes == null) return;
      foreach (XmlNode moduleNode in moduleNodes) {
         if (moduleNode == null) throw new Exception ("Module node is null");
         XmlAttribute? moduleIdAttr = moduleNode?.Attributes?["id"];
         if (moduleIdAttr == null) throw new Exception ("Module Id is null");
         string modId = moduleIdAttr.InnerText;
         XmlAttribute? moduleNameAttr = moduleNode?.Attributes?["name"];
         if (moduleNameAttr == null) throw new Exception ("Module Name is null");
         string modName = moduleNameAttr.InnerText;
         XmlAttribute? modulePathAttr = moduleNode?.Attributes?["path"];
         if (modulePathAttr == null) throw new Exception ("Module Path is null");
         string modPath = modulePathAttr.InnerText;
         XmlAttribute? blockCoverageAttr = moduleNode?.Attributes?["block_coverage"];
         if (blockCoverageAttr == null) throw new Exception ("Block Coverage is null");
         double blkCoverage = double.Parse (blockCoverageAttr.InnerText);
         XmlAttribute? lineCoverageAttr = moduleNode?.Attributes?["line_coverage"];
         if (lineCoverageAttr == null) throw new Exception ("Line Coverage is null");
         double lineCoverage = double.Parse (lineCoverageAttr.InnerText);
         XmlAttribute? blocksCoveredAttr = moduleNode?.Attributes?["blocks_covered"];
         if (blocksCoveredAttr == null) throw new Exception ("Blocks covered is null");
         int blocksCovered = int.Parse (blocksCoveredAttr.InnerText);
         XmlAttribute? blocksNotCoveredAttr = moduleNode?.Attributes?["blocks_not_covered"];
         if (blocksNotCoveredAttr == null) throw new Exception ("Blocks Not Covered is null");
         int blocksNotCovered = int.Parse (blocksNotCoveredAttr.InnerText);
         XmlAttribute? linesCoveredAttr = moduleNode?.Attributes?["lines_covered"];
         if (linesCoveredAttr == null) throw new Exception ("Lines Covered is null");
         int linesCovered = int.Parse (linesCoveredAttr.InnerText);
         XmlAttribute? linesPartiallyCoveredAttr = moduleNode?.Attributes?["lines_partially_covered"];
         if (linesPartiallyCoveredAttr == null) throw new Exception ("Lines Partially Covered is null");
         int linesPartiallyCovered = int.Parse (linesPartiallyCoveredAttr.InnerText);
         XmlAttribute? linesNotCoveredAttr = moduleNode?.Attributes?["lines_not_covered"];
         if (linesNotCoveredAttr == null) throw new Exception ("Lines Not Covered is null");
         int linesNotCovered = int.Parse (linesNotCoveredAttr.InnerText);
         this.AddModule (modId, modName, modPath, blkCoverage, lineCoverage, blocksCovered,
            blocksNotCovered, linesCovered, linesPartiallyCovered, linesNotCovered);


         // Get all <source> elements
         XmlNode? sourceFilesNode = moduleNode?.SelectSingleNode ("source_files");
         if (sourceFilesNode == null) return;
         XmlNodeList? sourceFiles = sourceFilesNode.SelectNodes ("source_file");
         if (sourceFiles == null) return;
         foreach (XmlNode sourceNode in sourceFiles) {
            if (sourceNode == null) continue;
            XmlAttribute? fullFilePathAttr = sourceNode.Attributes?["path"];
            if (fullFilePathAttr == null) throw new Exception ("XmlAttribute for source_file/path is null");
            string? srcFileName = System.IO.Path.GetFileName (fullFilePathAttr.InnerText);
            if (string.IsNullOrEmpty (srcFileName)) throw new Exception ("Source file name is null or empty");
            XmlAttribute? xmlSrcId = sourceNode.Attributes?["id"];
            if (xmlSrcId == null) throw new Exception ("Id of src file can not be retrieved");
            int id = int.Parse (xmlSrcId.InnerText);
            this.AddSrcFile (id, fullFilePathAttr.InnerText);
         }

         // Read <Function> information
         XmlNode? functionsNode = moduleNode?.SelectSingleNode ("functions");
         if (functionsNode == null) throw new Exception ("No functions node");
         XmlNodeList? functionNodeList = functionsNode.SelectNodes ("function");
         if (functionNodeList == null) throw new Exception ("function node not found");
         foreach (XmlNode functionNode in functionNodeList) {
            XmlAttribute? funcIdAttr = functionNode.Attributes?["id"];
            if (funcIdAttr == null) throw new Exception ("Function ID missing");
            int funId = int.Parse (funcIdAttr.InnerText);
            XmlAttribute? funcNameAttr = functionNode.Attributes?["name"];
            if (funcNameAttr == null) throw new Exception ("Function Name missing");
            string funName = funcNameAttr.InnerText;
            XmlAttribute? funcNamespaceAttr = functionNode.Attributes?["namespace"];
            if (funcNamespaceAttr == null) throw new Exception ("Namespace missing");
            string funNameSpace = funcNamespaceAttr.InnerText;
            XmlAttribute? funcClassNameAttr = functionNode.Attributes?["type_name"];
            if (funcClassNameAttr == null) throw new Exception ("Class Name missing");
            string className = funcClassNameAttr.InnerText;
            blockCoverageAttr = functionNode.Attributes?["block_coverage"];
            if (blockCoverageAttr == null) throw new Exception ("Blocks coverage missing");
            double blockCvrg = double.Parse (blockCoverageAttr.InnerText);
            lineCoverageAttr = functionNode.Attributes?["line_coverage"];
            if (lineCoverageAttr == null) throw new Exception ("Line Coverage missing");
            double lineCvrg = double.Parse (lineCoverageAttr.InnerText);
            blocksCoveredAttr = functionNode.Attributes?["blocks_covered"];
            if (blocksCoveredAttr == null) throw new Exception ("Blocks Covered missing");
            blocksCovered = int.Parse (blocksCoveredAttr.InnerText);
            blocksNotCoveredAttr = functionNode.Attributes?["blocks_not_covered"];
            if (blocksNotCoveredAttr == null) throw new Exception ("Blocks not covered missing");
            blocksNotCovered = int.Parse (blocksNotCoveredAttr.InnerText);
            linesCoveredAttr = functionNode.Attributes?["lines_covered"];
            if (linesCoveredAttr == null) throw new Exception ("linesCoveredAttr missing");
            linesCovered = int.Parse (linesCoveredAttr.InnerText);
            linesPartiallyCoveredAttr = functionNode.Attributes?["lines_partially_covered"];
            if (linesPartiallyCoveredAttr == null) throw new Exception ("linesPartiallyCoveredAttr missing");
            linesPartiallyCovered = int.Parse (linesPartiallyCoveredAttr.InnerText);
            linesNotCoveredAttr = functionNode.Attributes?["lines_not_covered"];
            if (linesNotCoveredAttr == null) throw new Exception ("linesNotCoveredAttr missing");
            linesNotCovered = int.Parse (linesNotCoveredAttr.InnerText);
            this.AddFunction (funId, funName, funNameSpace, className, blockCvrg, lineCvrg, blocksCovered,
               blocksNotCovered, linesCovered, linesPartiallyCovered, linesNotCovered);

            // Read <range> information
            XmlNode? rangesNode = functionNode.SelectSingleNode ("ranges");
            if (rangesNode == null) throw new Exception ("Ranges node not found");
            XmlNodeList? rangeNodeList = rangesNode.SelectNodes ("range");
            if (rangeNodeList == null) throw new Exception ("Range node list is null");
            bool sourceIdSetToFunction = false;
            foreach (XmlNode rangeNode in rangeNodeList) {
               XmlAttribute? sourceIdAttr = rangeNode.Attributes?["source_id"];
               if (sourceIdAttr == null) throw new Exception ("Source Id missing");
               int sourceId = int.Parse (sourceIdAttr.InnerText);
               XmlAttribute? startLineAttr = rangeNode.Attributes?["start_line"];
               if (startLineAttr == null) throw new Exception ("Start Line missing");
               int startLine = int.Parse (startLineAttr.InnerText);
               XmlAttribute? endLineAttr = rangeNode.Attributes?["end_line"];
               if (endLineAttr == null) throw new Exception ("End Line missing");
               int endLine = int.Parse (endLineAttr.InnerText);
               XmlAttribute? startColmnAttr = rangeNode.Attributes?["start_column"];
               if (startColmnAttr == null) throw new Exception ("Start Column missing");
               int startColumn = int.Parse (startColmnAttr.InnerText);
               XmlAttribute? endColmnAttr = rangeNode.Attributes?["end_column"];
               if (endColmnAttr == null) throw new Exception ("end Column missing");
               int endColumn = int.Parse (endColmnAttr.InnerText);
               XmlAttribute? isCoveredAttr = rangeNode.Attributes?["covered"];
               if (isCoveredAttr == null) throw new Exception ("end Column missing");
               bool isCovered = false;
               if (isCoveredAttr.InnerText.ToLower () == "yes") isCovered = true;
               this.AddRange (sourceId, startLine, endLine, startColumn, endColumn, isCovered);
               if (!sourceIdSetToFunction) {
                  SetSourceIdToLastFunction (sourceId);
                  sourceIdSetToFunction = true;
               }
            }
         }
      }

      // Compute total blocks, blocks covered
      foreach( var module in Modules??new List<Module> ()) {
         TotalBlocks += module.BlocksCovered + module.BlocksNotCovered;
         BlocksCovered += module.BlocksCovered;
      }
   }
   #endregion
}

