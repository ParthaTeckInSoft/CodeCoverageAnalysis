using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.ComponentModel;

namespace CoverageAnalyzer;
public enum LineHitStatus {
   Hit,
   NoHit,
   None
}

public class CodeCoverage : INotifyPropertyChanged {
   public CodeCoverage () => mFlowDoc = new FlowDocument ();

   List<SrcDetail>? mSrcDetails;
   public List<SrcDetail>? SrcDetails {
      get => mSrcDetails ??= new List<SrcDetail> ();
      set => mSrcDetails = value;
   }

   FlowDocument mFlowDoc;
   public FlowDocument FlowDoc {
      get => mFlowDoc;
      set {
         mFlowDoc = value;
         OnPropertyChanged (nameof (FlowDoc));
      }
   }
   
   public event PropertyChangedEventHandler? PropertyChanged;
   protected virtual void OnPropertyChanged (string propertyName) => PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));

   public void LoadFileIntoDocumentViewer (string filePath) {
      FlowDocument flowDocument = new FlowDocument ();

      Table table = new Table {
         CellSpacing = 0,
         BorderThickness = new Thickness (0), // No border
         FontFamily = new FontFamily ("Consolas"),
         FontSize = 10
      };

      for (int i = 0; i < 3; i++) table.Columns.Add (new TableColumn ());
      table.Columns[0].Width = new GridLength (30);
      table.Columns[1].Width = new GridLength (30);

      TableRow headerRow = new TableRow ();
      TableRowGroup headerGroup = new TableRowGroup ();
      headerGroup.Rows.Add (headerRow);
      table.RowGroups.Add (headerGroup);

      string[] fileLines = File.ReadAllLines (filePath);

      TableRowGroup bodyGroup = new TableRowGroup ();
      for (int i = 0; i < fileLines.Length; i++) {
         (LineHitStatus lineHitStatus, int hits) = ToHighlight (filePath, i + 1);
         string hitsStr = "";
         if (lineHitStatus != LineHitStatus.None) hitsStr = hits.ToString ();

         TableRow row = new TableRow ();
         row.Cells.Add (CreateBodyCell (hitsStr));
         row.Cells.Add (CreateBodyCell ((i + 1).ToString ()));
         TableCell contentCell = CreateBodyCell (fileLines[i]);

         if (lineHitStatus == LineHitStatus.Hit)
            contentCell.Background = new SolidColorBrush (Color.FromRgb (204, 255, 204));
         else if (lineHitStatus == LineHitStatus.NoHit)
            contentCell.Background = new SolidColorBrush (Color.FromRgb (255, 204, 204));

         row.Cells.Add (contentCell);
         bodyGroup.Rows.Add (row);
      }
      table.RowGroups.Add (bodyGroup);
      flowDocument.Blocks.Add (table);
      FlowDoc = flowDocument;
   }

   private TableCell CreateHeaderCell (string text) {
      Paragraph paragraph = new Paragraph (new Run (text)) {
         LineHeight = 8, // Reduced line height for less vertical spacing
         Margin = new Thickness (0), // Reduced margin
         FontFamily = new FontFamily ("Consolas"),
         FontSize = 12
      };
      TableCell cell = new TableCell (paragraph) {
         FontWeight = FontWeights.Bold,
         Background = Brushes.LightGray,
         Padding = new Thickness (2),
         BorderThickness = new Thickness (0), // No border
         BorderBrush = Brushes.Black
      };
      return cell;
   }

   private TableCell CreateBodyCell (string text) {
      Paragraph paragraph = new Paragraph (new Run (text)) {
         LineHeight = 4, // Reduced line height for less vertical spacing
         Margin = new Thickness (0), // Reduced margin
         FontFamily = new FontFamily ("Consolas"),
         FontSize = 10
      };
      TableCell cell = new TableCell (paragraph) {
         Padding = new Thickness (2), // Reduced padding
         BorderThickness = new Thickness (0), // No border
         BorderBrush = Brushes.Black
      };
      return cell;
   }

   (LineHitStatus, int) ToHighlight (string fullFilePath, int lineNumber) {
      var matchingLineDetail = SrcDetails?
          .Where (srcDetail => srcDetail.FullFilePath == fullFilePath)
          .SelectMany (srcDetail => srcDetail.LineDetails)
          .FirstOrDefault (lineDetail => lineDetail.Item1 == lineNumber);

      if (matchingLineDetail == null) return (LineHitStatus.None, 0);
      if (matchingLineDetail.Item2 > 0) return (LineHitStatus.Hit, matchingLineDetail.Item2);
      else return (LineHitStatus.NoHit, matchingLineDetail.Item2);
   }
}

