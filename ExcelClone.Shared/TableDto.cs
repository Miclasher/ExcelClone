namespace ExcelClone.Shared;

public sealed record TableDto(string Id, List<List<CellDto>> Cells);