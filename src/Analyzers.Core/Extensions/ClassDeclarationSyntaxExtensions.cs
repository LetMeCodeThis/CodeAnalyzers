namespace Analyzers.Core.Extensions
{
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class ClassDeclarationSyntaxExtensions
    {
        public static bool DoesContainTypeInBaseList(
            this ClassDeclarationSyntax classDeclaration,
            SemanticModel semanticModel,
            string typeToCheckFullyQualifiedName)
        {
            return classDeclaration?.BaseList?.Types
                    .Select(t => t.Type)
                    .OfType<GenericNameSyntax>()
                    .Any(g => g.GetFullyQualifiedName(semanticModel) == typeToCheckFullyQualifiedName) ?? false;
        }
    }
}