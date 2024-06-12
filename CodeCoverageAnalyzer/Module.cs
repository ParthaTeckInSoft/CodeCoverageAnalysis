using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoverageAnalyzer;

/// <summary>
/// This class is a place holder of the XML element data <module>. 
/// CodeCoverage class aggregates the list of modules
/// </summary>
public struct Module {
   #region Constructor
   public Module (string moduleId, string moduleName, string modulePath, double blkCvrg, double lnCvrg,
      int blkCovered, int blksNotCovered, int linesCovered, int linesPrtllyCovered, int linesNotCovered) {
      Id = moduleId; Name = moduleName; Path = modulePath; BlockCoverage = blkCvrg; LineCoverage = lnCvrg; BlocksCovered = blkCovered; 
      BlocksNotCovered = blksNotCovered; LinesCovered = linesCovered; LinesPartiallyCovered = linesPrtllyCovered; 
      LinesNotCovered = linesNotCovered;
   }
   #endregion

   #region Properties
   public string Id { get; set; }
   public string Name { get; set; }
   public string Path { get; set; }
   public double BlockCoverage { get; set; }
   public double LineCoverage { get; set; }
   public int BlocksCovered { get; set; }
   public int BlocksNotCovered { get; set; }
   public int LinesCovered { get; set; }
   public int LinesPartiallyCovered { get; set; }
   public int LinesNotCovered { get; set; }
   #endregion
}


