namespace Analyzers.Core.Extensions
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class GenericNameSyntaxExtensions
    {
        public static string GetFullyQualifiedName(
            this GenericNameSyntax genericNameSyntax,
            SemanticModel semanticModel)
        {
            var symbol = (INamedTypeSymbol)semanticModel.GetSymbolInfo(genericNameSyntax).Symbol;

            // When symbol for type is not found the return empty string (for missing namespace cases)
            return symbol?.GetFullyQualifiedName() ?? string.Empty;
        }
    }
}