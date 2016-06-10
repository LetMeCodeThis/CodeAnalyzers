namespace Analyzers.Core.Extensions
{
    using Microsoft.CodeAnalysis;

    public static class NamedTypeSymbolExtensions
    {
        public static string GetFullyQualifiedName(this INamedTypeSymbol nameTypeSymbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return nameTypeSymbol.ToDisplayString(symbolDisplayFormat);
        }
    }
}