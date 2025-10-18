using ExcelClone.Domain.Tables;

namespace ExcelClone.Tests;

[TestClass]
public class CalculationTests
{
    private Table _table = null!;

    [TestInitialize]
    public void Setup()
    {
        _table = new Table("TestSheet");
    }

    [TestMethod]
    public void BigNumberTest()
    {
        _table.SetExpression("A1", "=12333333333333 * 2");
        var cell = _table.GetCell("A1");
        Assert.AreEqual("24666666666666", cell.Value.ToString());
    }

    [TestMethod]
    public void DependancyChainCalculationTest()
    {
        _table.SetExpression("A2", "10");
        _table.SetExpression("B2", "=A2 * 2");
        _table.SetExpression("C2", "=B2 + 10");
        _table.SetExpression("D2", "=C2 / 5");

        Assert.AreEqual("6", _table.GetCell("D2").Value.ToString(), "Initial chain calculation is wrong.");

        _table.SetExpression("A2", "50");

        Assert.AreEqual("100", _table.GetCell("B2").Value.ToString());
        Assert.AreEqual("110", _table.GetCell("C2").Value.ToString());
        Assert.AreEqual("22", _table.GetCell("D2").Value.ToString());
    }

    [TestMethod]
    public void ComplexBoolLogicTest()
    {
        _table.SetExpression("A1", "50");
        _table.SetExpression("B1", "100");
        _table.SetExpression("C1", "110");
        _table.SetExpression("D1", "22");
        _table.SetExpression("E1", "=(A1 > B1)");
        _table.SetExpression("F1", "=(C1 > D1)");

        _table.SetExpression("G1", "=E1 <> F1");

        var cell = _table.GetCell("G1");
        Assert.AreEqual("TRUE", cell.Value.ToString());
    }

    [TestMethod]
    public void TypeMismatchErrorTest()
    {
        _table.SetExpression("E2", "TRUE");
        _table.SetExpression("F2", "=E2 + 5");

        var cell = _table.GetCell("F2");
        Assert.AreEqual("#VALUE!", cell.Value.ToString());
    }

    [TestMethod]
    public void CircularRefErrorTest()
    {
        _table.SetExpression("A3", "=B3");
        _table.SetExpression("B3", "=A3");

        var cellA = _table.GetCell("A3");
        var cellB = _table.GetCell("B3");

        Assert.AreEqual("#REF!", cellA.Value.ToString());
        Assert.AreEqual("#REF!", cellB.Value.ToString());
    }

    [TestMethod]
    public void NonExistantCellRefErrorTest()
    {
        _table.SetExpression("C3", "=X99 * 10");

        var cell = _table.GetCell("C3");
        Assert.AreEqual("#REF!", cell.Value.ToString());
    }

    [TestMethod]
    public void AutomaticDependaciesUpdateWithZeroTest()
    {
        _table.SetExpression("A2", "10");
        _table.SetExpression("B2", "=A2 * 2");
        _table.SetExpression("C2", "=B2 + 10");
        _table.SetExpression("D2", "=C2 / 5");

        _table.SetExpression("A2", "");

        Assert.AreEqual("0", _table.GetCell("B2").Value.ToString());
        Assert.AreEqual("10", _table.GetCell("C2").Value.ToString());
        Assert.AreEqual("2", _table.GetCell("D2").Value.ToString());
    }

    [TestMethod]
    public void MismatchingParenthesesErrorTest()
    {
        _table.SetExpression("D3", "=5 * (10 + 2");

        var cell = _table.GetCell("D3");
        Assert.AreEqual("#SYNTAX!", cell.Value.ToString());
    }

    [TestMethod]
    public void DecimalDivisionPrecisionTest()
    {
        _table.SetExpression("A4", "=10 / 3");

        var cell = _table.GetCell("A4");
        var value = cell.Value.ToString();

        Assert.IsTrue(value.StartsWith("3.33333333"));
        Assert.IsTrue(value.Length > 20);
    }
}
