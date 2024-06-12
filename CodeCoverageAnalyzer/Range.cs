namespace CoverageAnalyzer;

/// <summary>
/// This class is a place holder of the XML element data <Range>. 
/// CodeCoverage class aggregates the list of ranges
/// </summary>
public struct Range {
   #region Constrcutor
   public Range (int sourceId, int startLine, int endLine, int startColumn, int endColumn, bool isCovered) {
      SourceId = sourceId;
      StartLine = startLine;
      EndLine = endLine;
      StartColumn = startColumn;
      EndColumn = endColumn;
      IsCovered = isCovered;
   }
   #endregion

   #region Properties
   public Range () { }
   public int SourceId { get; set; }
   public int StartLine { get; set; }
   public int EndLine { get; set; }
   public int StartColumn { get; set; }
   public int EndColumn { get; set; }
   public bool IsCovered { get; set; }
   #endregion
}

