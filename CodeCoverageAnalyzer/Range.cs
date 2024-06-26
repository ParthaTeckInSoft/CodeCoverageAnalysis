using System;

namespace CoverageAnalyzer;

/// <summary>
/// This struct is a place holder of the XML element Range
/// CodeCoverage class aggregates a list of Ranges against the
/// full file path of the source as the KEY
/// The Range element in the XML looks like
/// ranges
/// range source_id = "0" start_line="4" end_line="4" start_column="1" end_column="37" covered="yes" 
/// ...
/// </summary>
public struct Range (int sourceId, int startLine, int endLine, int startColumn, int endColumn, bool isCovered) {
   #region Properties
   public int SourceId { get; set; } = sourceId;
   public int StartLine { get; set; } = startLine;
   public int EndLine { get; set; } = endLine;
   public int StartColumn { get; set; } = startColumn;
   public int EndColumn { get; set; } = endColumn;
   public bool IsCovered { get; set; } = isCovered;
   #endregion
}

