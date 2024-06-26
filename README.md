# Code Coverage Analyzer
This is a WPF application to analyze the code coverage output generated in the form of XML, given out by dotnet-coverage tool. 

__Steps to use this tool__
* Build the tool using the solution file provided (inside the folder CodeCoverageAnalyzer)
* Invoke the tool and open the XML file (coverage.xml)
* Browse through the tree in the left side. 
* "click select- Invoke" and load the corresponding source file.
* Close when done

__The content of the xml be like__
```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<results>
  <modules>
    <module id="A5C05FCC41466E4F92D0A60FD107E75E747C73F5" name="CodeCoverageTestsSet.dll" path="CodeCoverageTestsSet.dll" block_coverage="65.96" line_coverage="73.33" blocks_covered="31" blocks_not_covered="16" lines_covered="22" lines_partially_covered="4" lines_not_covered="4">
      <functions>
        <function id="8272" token="0x6000001" name="DoSomething()" namespace="CodeCoverageAnalyze" type_name="SomeClass" block_coverage="0.00" line_coverage="0.00" blocks_covered="0" blocks_not_covered="1" lines_covered="0" lines_partially_covered="0" lines_not_covered="2">
          <ranges>
            <range source_id="0" start_line="8" end_line="8" start_column="5" end_column="6" covered="no" />
            <range source_id="0" start_line="10" end_line="10" start_column="5" end_column="6" covered="no" />
          </ranges>
        </function>
        <function id="8284" token="0x6000003" name="Main(string[])" namespace="CodeCoverageAnalyze" type_name="Program" block_coverage="100.00" line_coverage="100.00" blocks_covered="11" blocks_not_covered="0" lines_covered="11" lines_partially_covered="0" lines_not_covered="0">
          <ranges>
            <range source_id="1" start_line="18" end_line="18" start_column="7" end_column="61" covered="yes" />
            <range source_id="1" start_line="19" end_line="19" start_column="7" end_column="82" covered="yes" />
            <range source_id="1" start_line="20" end_line="20" start_column="4" end_column="5" covered="yes" />
          </ranges>
        </function>
        </functions>
      <source_files>
        <source_file id="0" path="C:\Users\CodeCoverageAnalyze\src\CodeCoverageAnalyze\Class1.cs" checksum_type="SHA256" checksum="38F6E43B42960361CA6689953C19C27A21FFFB71074A59D07FD944FE3A42F719" />
        <source_file id="1" path="C:\Users\CodeCoverageAnalyze\src\CodeCoverageAnalyze\Program.cs" checksum_type="SHA256" checksum="6D13079EFDB43D0D613E5619C9ACF54688230245C5ECB1891C005B2621956FEB" />
        <source_file id="2" path="C:\Users\CodeCoverageAnalyze\src\CodeCoverageAnalyze\MultFolder\Multiply.cs" checksum_type="SHA256" checksum="81863AA921AFF647DAE33E3C2747617B0C1E60C1610D7332056003390AD69F27" />
        <source_file id="3" path="C:\Users\odeCoverageAnalyze\src\CodeCoverageAnalyze\AdditionsFolder\Additions.cs" checksum_type="SHA256" checksum="3D7D89B4333D7C42CD19832EC2484B7235E227DB92B23B415BA0AAF8E21E684E" />
      </source_files>
      <skipped_functions />
    </module>
  </modules>
  <skipped_modules>
  <skipped_module name="system.linq.dll" path="system.linq.dll" reason="no_symbols" />
  </skipped_modules>
</results>
