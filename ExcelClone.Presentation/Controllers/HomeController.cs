using ExcelClone.Presentation.ViewModels;
using ExcelClone.Services.Abstractions;
using ExcelClone.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ExcelClone.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ITableService _tableService;

    public HomeController(ITableService tableService)
    {
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string id = "MySheet", string selected = "A1")
    {
        var tableDto = await _tableService.GetTableDtoAsync(id);

        var selectedCell = tableDto?.Cells
            .SelectMany(row => row)
            .FirstOrDefault(cell => cell.Address.Equals(selected, StringComparison.OrdinalIgnoreCase));

        var viewModel = new TableViewModel
        {
            TableId = id,
            Table = tableDto,
            SelectedCellAddress = selected,
            SelectedCellExpression = selectedCell?.Expression ?? ""
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EditCell(string tableId, string cellAddress, string? cellExpression)
    {
        await _tableService.UpdateCellAsync(tableId, new EditCellRequest(cellAddress, cellExpression ?? string.Empty));
        return RedirectToAction("Index", new { id = tableId, selected = cellAddress });
    }

    [HttpPost]
    public async Task<IActionResult> AddRow(string tableId, string selectedCellAddress)
    {
        await _tableService.AddRowAsync(tableId);
        return RedirectToAction("Index", new { id = tableId, selected = selectedCellAddress });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRow(string tableId, string selectedCellAddress)
    {
        await _tableService.RemoveLastRowAsync(tableId);
        return RedirectToAction("Index", new { id = tableId, selected = selectedCellAddress });
    }

    [HttpPost]
    public async Task<IActionResult> AddColumn(string tableId, string selectedCellAddress)
    {
        await _tableService.AddColumnAsync(tableId);
        return RedirectToAction("Index", new { id = tableId, selected = selectedCellAddress });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveColumn(string tableId, string selectedCellAddress)
    {
        await _tableService.RemoveLastColumnAsync(tableId);
        return RedirectToAction("Index", new { id = tableId, selected = selectedCellAddress });
    }

    [HttpPost]
    public async Task<IActionResult> Save(string tableId)
    {
        await _tableService.SaveTableToDriveAsync(tableId);
        TempData["StatusMessage"] = $"Таблицю зі ідентифікатором '{tableId}' збережено.";

        return RedirectToAction("Index", new { id = tableId });
    }

    public IActionResult Info()
    {
        return View();
    }
}
