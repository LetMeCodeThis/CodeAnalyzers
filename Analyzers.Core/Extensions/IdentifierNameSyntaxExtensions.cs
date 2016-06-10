namespace Analyzers.Core.Extensions
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class IdentifierNameSyntaxExtensions
    {
        public static string GetFullyQualifiedName(
            this IdentifierNameSyntax identifierNameSyntax,
            SemanticModel semanticModel)
        {
            var genericArgumentSymbol = (INamedTypeSymbol)semanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;

            return genericArgumentSymbol.BaseType.OriginalDefinition.ToDisplayString();
        }
    }
}