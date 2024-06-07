namespace CoverageAnalyzer;

public struct SrcDetail {
   public string? FullFilePath { get; set; }
   public string? ClassName { get; set; }
   public string? MethodName { get; set; }

   List<Tuple<int, int>> mLineDetails;
   public void SetLineDetails (int line, int hits) {
      mLineDetails = new List<Tuple<int, int>> ();
      var tupLine = new Tuple<int, int> (line, hits);
      mLineDetails.Add (tupLine);
   }

   public List<Tuple<int, int>> LineDetails { get => mLineDetails; }
   public SrcDetail () {
      mLineDetails = new List<Tuple<int, int>> ();
   }
}

