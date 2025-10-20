namespace ExcelClone.Shared;

public sealed record EditCellRequest(string CellAddress, string NewExpression);