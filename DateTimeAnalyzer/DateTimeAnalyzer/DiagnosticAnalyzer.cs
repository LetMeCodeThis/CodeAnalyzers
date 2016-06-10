namespace DateTimeAnalyzer
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DateTimeAnalyzer";

        internal static readonly LocalizableString Title =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerTitle),
                Resources.ResourceManager,
                typeof(Resources));

        internal static readonly LocalizableString Description =
            new LocalizableResourceString(
                nameof(Resources.AnalyzerDescription),
                Resources.ResourceManager,
                typeof(Resources));

        internal const string Category = "API Guidance";

        internal static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(
                DiagnosticId,
                Title,
                Description,
                Category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true,
                description: Description);

        private static readonly string[] PropertiesToCheckForUsage = { "Now", "Today" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.IdentifierName);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var identifier = (IdentifierNameSyntax)context.Node;

            if (PropertiesToCheckForUsage.Contains(identifier.Identifier.Text))
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(identifier);

                if (IsSymbolUsedFromDateTimeType(symbolInfo) && IsApplicable(context))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Rule,
                            GetSquigglesLocation(identifier),
                            symbolInfo.Symbol.Name));
                }
            }
        }

        private static bool IsSymbolUsedFromDateTimeType(SymbolInfo symbolInfo)
            => symbolInfo.Symbol?.ContainingType.SpecialType == SpecialType.System_DateTime;

        private static bool IsApplicable(SyntaxNodeAnalysisContext context)
            => !context.SemanticModel.Compilation.AssemblyName.Contains("UnitTest");

        private static Location GetSquigglesLocation(IdentifierNameSyntax identifier)
            => identifier.FirstAncestorOrSelf<MemberAccessExpressionSyntax>().GetLocation();
    }
}