using System.ComponentModel;
using System.Windows;
using System.Xml;

namespace CoverageAnalyzer;

public class CodeCoverage : INotifyPropertyChanged {
   #region Constructor(s)
   public CodeCoverage () {
      mRangesMap = new ();
      mSrcFilesMap = new ();
      mFunctionsMap = new ();
   }
   #endregion

   #region Properties and specific setters and getters
   Dictionary<string, List<Range>> mRangesMap;

   /// <summary>
   /// This method populates a new Range with the list held by the mRangesMap dictionary. The
   /// key to the dictionary is the full file path.
   /// </summary>
   /// <param name="filePath">Full file path, which is the key</param>
   /// <param name="sourceId">Range member Source ID which is the index of the full file path in mSrcFilesMap dictionary</param>
   /// <param name="startLine">Range member start line ( starting from 1) that coverage tool lists as covered or not covered</param>
   /// <param name="endLine">Range member End Line</param>
   /// <param name="startColumn">Range member Start Column either covered or not covered(starting from 1) </param>
   /// <param name="endColumn">Range member End column </param>
   /// <param name="isCovered">Range member boolean if the start to end ( line/column) is covered or not</param>
   public void AddRange (string filePath, int sourceId, int startLine, int endLine, int startColumn, int endColumn, bool isCovered) {
      var range = new Range (sourceId, startLine, endLine, startColumn, endColumn, isCovered);
      if (!mRangesMap.ContainsKey (filePath)) mRangesMap[filePath] = [];
      mRangesMap[filePath].Add (range);
   }

   Dictionary<int, string> mSrcFilesMap;

   /// <summary>
   /// Returns the list of source files mentioned in the coverage output xml document
   /// </summary>
   public List<string> SrcFiles {
      get {
         List<string> files = [];
         foreach (KeyValuePair<int, string> kvp in mSrcFilesMap)
            files.Add (kvp.Value);
         return files;
      }
   }

   /// <summary>
   /// Specific method to add source file path to the mSrcFilesMap dictionary
   /// </summary>
   /// <param name="sId">The same ID that XML file stores against each full file path as KEY</param>
   /// <param name="srcFilename">The full file path</param>
   public void AddSrcFile (int sId, string srcFilename) {
      if (!mSrcFilesMap.ContainsKey (sId)) mSrcFilesMap[sId] = srcFilename;
   }

   /// <summary>
   /// Method to return the full file path of the file, given its key ID
   /// </summary>
   /// <param name="sId"></param>
   /// <returns>Full file path if key sId is found or Empty string if sId is not found</returns>
   public string GetSrcFilename (int sId) {
      if (mSrcFilesMap.TryGetValue (sId, out string? value)) 
         return value??string.Empty;
       else return string.Empty;
   }

   /// <summary>
   /// This method returns sums up the blocks covered and blocks not covered
   /// for the given source file.
   /// </summary>
   /// <param name="filepath">The full file path of the give source file</param>
   /// <returns>A tuple of Blocks Covered AND Blocks not covered</returns>
   public Tuple<int, int> GetFileBlocksCoverageInfo (string filepath) {
      var funs = mFunctionsMap[filepath];
      int blocksCovered = funs.Sum (f => f.BlocksCovered);
      int blocksNotCovered = funs.Sum( f=> f.BlocksNotCovered);
      return new Tuple<int,int>(blocksCovered, blocksNotCovered);
   }

   Dictionary<string, List<Function>> mFunctionsMap;

   /// <summary>
   /// THis method Adds a new Function Info to add it to the dictionary mFunctionsMap
   /// </summary>
   /// <param name="filename">The key for the dictionary, which is the full file path</param>
   /// <param name="blocksCovered">The no. of blocks covered for this function</param>
   /// <param name="blocksNotCovered">The no. of blocks not covered for this function</param>
   public void AddFunction (string filename, int blocksCovered, int blocksNotCovered) {
      if ( !mFunctionsMap.ContainsKey (filename)) mFunctionsMap[filename] = new ();
         mFunctionsMap[filename].Add(new Function(blocksCovered, blocksNotCovered));
   }

   /// <summary>
   /// This method returns the List of Range, for a specific source file,
   /// where a range holds start/end line and start/end column of coverage/non coverage
   /// for a particular line number
   /// </summary>
   /// <param name="fullFilePath">The full file path of the source file, which is also the key</param>
   /// <param name="lineNo">The ranges that cover a specific line number</param>
   /// <returns></returns>
   public List<Range> GetRanges (string fullFilePath, int lineNo) {
      List<Range> ranges = new List<Range> ();
      if (mRangesMap.ContainsKey (fullFilePath)) {
         ranges = mRangesMap[fullFilePath];
         ranges = ranges.Where (it => it.StartLine == lineNo).ToList ();
      }
      return ranges;
   }

   public int TotalBlocks { get; set; }
   public int BlocksCovered { get; set; }
   #endregion

   #region Clearances
   /// <summary>
   /// Method to reset all the dictionaries.
   /// </summary>
   public void ClearAll () {
      mFunctionsMap?.Clear ();
      mSrcFilesMap?.Clear ();
      mRangesMap?.Clear ();
   }
   #endregion

   #region Events
   public event PropertyChangedEventHandler? PropertyChanged;
   #endregion

   #region Callbacks(s)
   protected virtual void OnPropertyChanged (string propertyName) => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
   #endregion

   #region XML loading/helper methods

   /// <summary>
   /// This method is a helper which reads the attribute from the XMLNode
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="node">The XMLNode whose "attributeName" has to be parsed</param>
   /// <param name="attributeName">The name of the attribute</param>
   /// <param name="parseFunc">The parsing function. Example: in the case of intger, it is int.Parse</param>
   /// <returns>The parsed value of the type T</returns>
   /// <exception cref="Exception">Throws exception when encountered one while reading XML attribute</exception>
   static T GetAttributeVal<T> (XmlNode? node, string attributeName, Func<string, T>? parseFunc = null) {
      var attribute = node?.Attributes?[attributeName];
      if (attribute == null) throw new Exception ("XML coverage file read failed");

      // If parseFunc is not provided, use a default identity function for string type
      parseFunc ??= text => (T)(object)text;

      // Apply the parsing function
      return parseFunc (attribute.InnerText);
   }

   /// <summary>
   /// This method loads the coverage report XML document
   /// </summary>
   /// <param name="xmlFilename">Filepath to the xml document</param>
   /// <exception cref="Exception">This method throws exception if the XML coverage 
   /// document is inconsistent</exception>
   public void LoadXMLDocument (string xmlFilename) {
      string errormsg = "Reading of coverage report document filed";
      int totalBlocksCovered = 0, totalBlocksNotCovered = 0;
      try {
         // Clear any previously loaded data
         ClearAll ();

         // Read and process the XML file
         XmlDocument coverageXmlDoc = new XmlDocument ();
         coverageXmlDoc.Load (xmlFilename);

         // Get the root element
         XmlElement? xmlRoot = coverageXmlDoc.DocumentElement??throw new Exception (errormsg);

         // Read all <Module> information
         XmlNodeList? moduleNodes = coverageXmlDoc.SelectNodes ("//modules/module")??throw new Exception (errormsg);
         XmlNodeList? sourceFiles = coverageXmlDoc.SelectNodes ("//modules/module/source_files/source_file") ?? throw new Exception (errormsg);
         foreach (XmlNode moduleNode in moduleNodes) {
            totalBlocksCovered += GetAttributeVal (moduleNode, "blocks_covered", int.Parse);
            totalBlocksNotCovered += GetAttributeVal (moduleNode, "blocks_not_covered", int.Parse);

            // Read the source file names. The source files are read and are referred to the index for use
            foreach (XmlNode sourceNode in sourceFiles) {
               string srcFileName = GetAttributeVal<string> (sourceNode, "path");
               int id = GetAttributeVal (sourceNode, "id", int.Parse);
               AddSrcFile (id, srcFileName);
            }

            // Read <Function> information
            XmlNode? functionsNode = moduleNode?.SelectSingleNode ("functions")?? throw new Exception (errormsg);
            XmlNodeList? functionNodeList = functionsNode.SelectNodes ("function") ?? throw new Exception (errormsg);

            foreach (XmlNode functionNode in functionNodeList) {
               XmlNode? rangesNode = functionNode.SelectSingleNode ("ranges") ?? throw new Exception (errormsg);
               XmlNodeList? rangeNodeList = rangesNode.SelectNodes ("range") ?? throw new Exception (errormsg);

               bool functionMapSet = false;
               foreach (XmlNode rangeNode in rangeNodeList) {
                  int sourceId = GetAttributeVal (rangeNode, "source_id", int.Parse);
                  int startLine = GetAttributeVal (rangeNode, "start_line", int.Parse);
                  int endLine = GetAttributeVal (rangeNode, "end_line", int.Parse);
                  int startColumn = GetAttributeVal (rangeNode, "start_column", int.Parse);
                  int endColumn = GetAttributeVal (rangeNode, "end_column", int.Parse);
                  string covered = GetAttributeVal<string> (rangeNode, "covered");
                  bool isCovered = false; if (covered.Equals("yes", StringComparison.CurrentCultureIgnoreCase)) isCovered = true;
                  AddRange (GetSrcFilename (sourceId), sourceId, startLine, endLine, startColumn, endColumn, isCovered);
                  if (!functionMapSet) {
                     int blocksCovered = GetAttributeVal (functionNode, "blocks_covered", int.Parse);
                     int blocksNotCovered = GetAttributeVal (functionNode, "blocks_not_covered", int.Parse);
                     AddFunction (GetSrcFilename (sourceId), blocksCovered, blocksNotCovered);
                     functionMapSet = true;
                  }
               }
            }
         }
         // The total blocks covered and not covered is for the entire source code
         // exposed to the coverage analyzer. This is read from the Modules.
         TotalBlocks = totalBlocksCovered + totalBlocksNotCovered;
         BlocksCovered = totalBlocksCovered;
      } catch (Exception ex) {
         MessageBox.Show (ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
   }
   #endregion
}