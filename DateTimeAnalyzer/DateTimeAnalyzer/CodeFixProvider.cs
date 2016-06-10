namespace DateTimeAnalyzer
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Analyzers.Core.Extensions;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeAnalyzerCodeFixProvider)), Shared]
    public class DateTimeAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string SystemTimeTypeNamespace = "Playground";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(DateTimeAnalyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.
                GetSyntaxRootAsync(context.CancellationToken).
                ConfigureAwait(false);
            var memberAccessExpression = (MemberAccessExpressionSyntax)root.FindNode(context.Span);
            var propertyIdentifierName = memberAccessExpression.Name.Identifier.Text;

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Replace with SystemTime",
                    token => this.ReplaceWithSystemTime(
                        context.Document,
                        propertyIdentifierName,
                        memberAccessExpression,
                        token),
                    null),
                context.Diagnostics.First());
        }

        private async Task<Document> ReplaceWithSystemTime(
            Document document,
            string propertyIdentifierName,
            MemberAccessExpressionSyntax memberAccessExpression,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(
                memberAccessExpression,
                SyntaxFactory.IdentifierName($"SystemTime.{propertyIdentifierName}()"));
            var compilationUnit = newRoot.DescendantNodesAndSelf().OfType<CompilationUnitSyntax>().First();

            if (!compilationUnit.ContainsUsingDirective(SystemTimeTypeNamespace))
            {
                var newCompilationUnit = compilationUnit.AddAndSortUsingDirectives(SystemTimeTypeNamespace);

                newRoot = newRoot.ReplaceNode(compilationUnit, newCompilationUnit);
            }

            return document.WithSyntaxRoot(newRoot);
        }
    }
}