using ExcelClone.Shared;

namespace ExcelClone.Presentation.ViewModels;

public sealed class TableViewModel
{
    public string TableId { get; set; } = "MySheet";
    public TableDto? Table { get; set; }
    public string SelectedCellAddress { get; set; } = "A1";
    public string SelectedCellExpression { get; set; } = "";
}