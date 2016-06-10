namespace Analyzers.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public static class EnumerableSyntaxNodeExtensions
    {
        public static SyntaxList<SyntaxNode> ToSyntaxList<TSyntaxNode>(
            this IEnumerable<TSyntaxNode> enumerable)
            where TSyntaxNode : SyntaxNode
            =>
            SyntaxFactory.List(enumerable);

        public static SyntaxList<SyntaxNode> ToSyntaxList<TSyntaxNode>(
            this IOrderedEnumerable<TSyntaxNode> enumerable)
            where TSyntaxNode : SyntaxNode
            =>
            SyntaxFactory.List(enumerable);
    }
}