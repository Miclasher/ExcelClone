namespace ExcelClone.Services.Abstractions;

public interface ITableService
{
    void AddRow(int index);
    void RemoveRow(int index);
    void AddColumn(int index);
    void RemoveColumn(int index);

    void UpdateCell(string address, string rawExpression);
}