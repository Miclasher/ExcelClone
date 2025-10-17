using System.Security.Cryptography.X509Certificates;

namespace ExcelClone.Domain.Abstractions;

public sealed record Error(string Code, string Name)
{
    public static Error None() => new Error(string.Empty, string.Empty);
    public static Error NullValue() => new Error("Error.NullValue", "Provided value was null");
};