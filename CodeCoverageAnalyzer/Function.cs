using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageAnalyzer;

/// <summary>
/// This class is a place holder of the XML element data <Function>. 
/// CodeCoverage class aggregates the list of functions</summary>
public struct Function {
   #region Constructor
   public Function (int funId, string funcName, string nspace, string cName, double blkCvrg, double lnCvrg,
      int blkCovered, int blksNotCovered, int linesCovered, int linesPrtllyCovered, int linesNotCovered) {
      FunctionId = funId; FunctionName = funcName; Namespace = nspace; Classname = cName;
      BlockCoverage = blkCvrg; LineCoverage = lnCvrg; BlocksCovered = blkCovered; BlocksNotCovered = blksNotCovered;
      LinesCovered = linesCovered; LinesPartiallyCovered = linesPrtllyCovered; LinesNotCovered = linesNotCovered;
   }
   #endregion

   #region Properties
   public int SourceId { get; set; }
   public int FunctionId { get; set; }
   public string FunctionName { get; set; }
   public string Namespace { get; set; }
   public string Classname { get; set; }
   public double BlockCoverage { get; set; }
   public double LineCoverage { get; set; }
   public int BlocksCovered { get; set; }
   public int BlocksNotCovered { get; set; }
   public int LinesCovered { get; set; }
   public int LinesPartiallyCovered { get; set; }
   public int LinesNotCovered { get; set; }
   #endregion
}


