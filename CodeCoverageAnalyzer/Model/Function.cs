namespace CoverageAnalyzer;

/// <summary>
/// This struct is a place holder of the XML element Function
/// CodeCoverage class aggregates a list of Functions against the
/// full file path of the source as the KEY
/// A Function element in coverage.xml will look similar to
/// functions
/// function id = "8272" token="0x6000001" name="&lt;Main&gt;$(string[])" 
///          type_name="Program" block_coverage="100.00" line_coverage="100.00" 
///          blocks_covered="6" blocks_not_covered="0" lines_covered="7" 
///          lines_partially_covered="0" lines_not_covered="0">
/// </summary>
public struct Function(int blocksCovered, int blocksNotCovered)  {
   #region Properties
   public int BlocksCovered { get; set; } = blocksCovered;
   public int BlocksNotCovered { get; set; } = blocksNotCovered;
   #endregion
}


