namespace Analyzers.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class CompilationUnitSyntaxExtensions
    {
        public static NamespaceDeclarationSyntax GetNamespaceDeclaration(
            this CompilationUnitSyntax compilation)
            =>
            compilation
                .DescendantNodesAndSelf()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

        public static bool ContainsUsingDirective(
            this CompilationUnitSyntax compilation,
            string namespaceName)
        {
            // Using not found at the top of the file so we are looking for inside the namespace
            if (!compilation.Usings.Any(u => u.Name.ToFullString() == namespaceName))
            {
                var ns = compilation.GetNamespaceDeclaration();

                return ns?.Usings.Any(u => u.Name.ToFullString() == namespaceName) ?? false;
            }

            return true;
        }

        public static CompilationUnitSyntax SortUsingDirectives(this CompilationUnitSyntax compilation)
        {
            var ns = compilation.GetNamespaceDeclaration();

            return compilation.SortUsingDirectives(ref ns);
        }

        public static CompilationUnitSyntax RemoveAllUsings(
            this CompilationUnitSyntax compilation,
            ref NamespaceDeclarationSyntax ns)
        {
            if (compilation.Usings.Count > 0)
            {
                compilation = compilation.WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>());
            }

            if (ns?.Usings.Count > 0)
            {
                var newNs = ns.WithUsings(SyntaxFactory.List<UsingDirectiveSyntax>());

                compilation = compilation.ReplaceNode(ns, newNs);
                ns = newNs;
            }

            return compilation;
        }

        public static CompilationUnitSyntax AddAndSortUsingDirectives(
            this CompilationUnitSyntax compilation,
            string namespaceName)
        {
            var ns = compilation.GetNamespaceDeclaration();

            return compilation.AddUsingDirective(namespaceName, ref ns).SortUsingDirectives(ref ns);
        }

        private static CompilationUnitSyntax SortUsingDirectives(
            this CompilationUnitSyntax compilation,
            ref NamespaceDeclarationSyntax ns)
        {
            // TODO: Add removing of unused namespaces
            var usings = compilation
                .GetAllUsingDirectives(ns)
                .OrderBy(u => u, UsingsAndExternAliasesDirectiveComparer.SystemFirstInstance)
                .ToSyntaxList();
            
            compilation = compilation.RemoveAllUsings(ref ns);

            if (ns != null)
            {
                var newNs = ns.WithUsings(usings);

                compilation = compilation.ReplaceNode(ns, newNs);
                ns = newNs;
            }
            else
            {
                compilation = compilation.WithUsings(usings);
            }

            return compilation;
        }

        private static CompilationUnitSyntax AddUsingDirective(
            this CompilationUnitSyntax compilation,
            string namespacesToAdd,
            ref NamespaceDeclarationSyntax ns)
        {
            var usingDirective = CreateUsingDirective(namespacesToAdd);

            // Namespace doesn't exists so we are adding using at the top of the file
            if (ns == null)
            {
                if (!compilation.Usings.Any(u => u.Name.ToFullString() == namespacesToAdd))
                {
                    var newUsings = compilation.Usings.Add(usingDirective);

                    compilation = compilation.WithUsings(newUsings);
                }
            }
            else if (!ns.Usings.Any(u => u.Name.ToFullString() == namespacesToAdd))
            {
                var newUsings = ns.Usings.Add(usingDirective);
                var newNs = ns.WithUsings(newUsings);

                compilation = compilation.ReplaceNode(ns, newNs);
                ns = newNs;
            }

            return compilation;
        }

        private static CompilationUnitSyntax AddUsingDirectives(
            this CompilationUnitSyntax compilation,
            IEnumerable<string> namespacesToAdd,
            ref NamespaceDeclarationSyntax ns)
        {
            foreach (var namespaceName in namespacesToAdd)
            {
                compilation = compilation.AddUsingDirective(namespaceName, ref ns);
            }

            return compilation;
        }

        private static SyntaxList<UsingDirectiveSyntax> GetAllUsingDirectives(
            this CompilationUnitSyntax compilation,
            NamespaceDeclarationSyntax ns)
            => ns != null
                ? compilation.Usings.AddRange(ns.Usings)
                : compilation.Usings;

        private static UsingDirectiveSyntax CreateUsingDirective(string namespaceName)
            => SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName(namespaceName).NormalizeWhitespace());
    }
}