using System.Collections.Generic;

namespace HubTopology_API.Model
{
  public class ResultTable
  {
    public int totalRecords { get; set; }
    public int count { get; set; }
    public Table data { get; set; }
  }

  public class Column
  {
    public string name { get; set; }
    public string type { get; set; }
  }

  public class Table
  {
    public List<Column> columns { get; set; }
    public List<List<string>> rows { get; set; }
  }
}
