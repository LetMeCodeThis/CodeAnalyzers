﻿namespace Analyzers.Core
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    
    public class NameSyntaxComparer : IComparer<NameSyntax>
    {
        private readonly IComparer<SyntaxToken> tokenComparer;

        internal TypeSyntaxComparer TypeComparer;

        internal NameSyntaxComparer(IComparer<SyntaxToken> tokenComparer)
        {
            this.tokenComparer = tokenComparer;
        }

        public static IComparer<NameSyntax> Create() => Create(TokenComparer.NormalInstance);

        public static IComparer<NameSyntax> Create(IComparer<SyntaxToken> tokenComparer)
        {
            var nameComparer = new NameSyntaxComparer(tokenComparer);
            var typeComparer = new TypeSyntaxComparer(tokenComparer);

            nameComparer.TypeComparer = typeComparer;
            typeComparer.NameComparer = nameComparer;

            return nameComparer;
        }

        public int Compare(NameSyntax x, NameSyntax y)
        {
            if (x == y)
            {
                return 0;
            }

            if (x.IsMissing && y.IsMissing)
            {
                return 0;
            }

            if (x.IsMissing)
            {
                return -1;
            }
            else if (y.IsMissing)
            {
                return 1;
            }

            // If we have a basic name, then it's simple to compare.  Just
            // check that token versus whatever the other name has as the
            // first token.
            if (x is IdentifierNameSyntax && y is IdentifierNameSyntax)
            {
                return this.tokenComparer.Compare(x.GetFirstToken(includeSkipped: true), y.GetFirstToken());
            }
            else if (x is GenericNameSyntax && y is GenericNameSyntax)
            {
                // if both names are generic, then use a specialized routine
                // that will check the names *and* the arguments.
                return this.Compare((GenericNameSyntax)x, (GenericNameSyntax)y);
            }
            else if (x is IdentifierNameSyntax && y is GenericNameSyntax)
            {
                var compare = this.tokenComparer.Compare(x.GetFirstToken(includeSkipped: true), y.GetFirstToken());

                if (compare != 0)
                {
                    return compare;
                }

                // Foo goes before Foo<T>
                return -1;
            }
            else if (x is GenericNameSyntax && y is IdentifierNameSyntax)
            {
                var compare = this.tokenComparer.Compare(x.GetFirstToken(includeSkipped: true), y.GetFirstToken());

                if (compare != 0)
                {
                    return compare;
                }

                // Foo<T> goes after Foo
                return 1;
            }

            // At this point one or both of the nodes is a dotted name or
            // aliased name.  Break them apart into individual pieces and
            // compare those.

            var xNameParts = this.DecomposeNameParts(x);
            var yNameParts = this.DecomposeNameParts(y);

            for (var i = 0; i < xNameParts.Count && i < yNameParts.Count; i++)
            {
                var compare = this.Compare(xNameParts[i], yNameParts[i]);

                if (compare != 0)
                {
                    return compare;
                }
            }

            // they matched up to this point.  The shorter one should come
            // first.
            return xNameParts.Count - yNameParts.Count;
        }

        private IList<SimpleNameSyntax> DecomposeNameParts(NameSyntax name)
        {
            var result = new List<SimpleNameSyntax>();

            this.DecomposeNameParts(name, result);

            return result;
        }

        private void DecomposeNameParts(NameSyntax name, List<SimpleNameSyntax> result)
        {
            switch (name.Kind())
            {
                case SyntaxKind.QualifiedName:
                    var dottedName = (QualifiedNameSyntax)name;
                    this.DecomposeNameParts(dottedName.Left, result);
                    this.DecomposeNameParts(dottedName.Right, result);
                    break;
                case SyntaxKind.AliasQualifiedName:
                    var aliasedName = (AliasQualifiedNameSyntax)name;
                    result.Add(aliasedName.Alias);
                    this.DecomposeNameParts(aliasedName.Name, result);
                    break;
                case SyntaxKind.IdentifierName:
                    result.Add((IdentifierNameSyntax)name);
                    break;
                case SyntaxKind.GenericName:
                    result.Add((GenericNameSyntax)name);
                    break;
            }
        }

        private int Compare(GenericNameSyntax x, GenericNameSyntax y)
        {
            var compare = tokenComparer.Compare(x.Identifier, y.Identifier);

            if (compare != 0)
            {
                return compare;
            }

            // The one with less type params comes first.
            compare = x.Arity - y.Arity;

            if (compare != 0)
            {
                return compare;
            }

            // Same name, same parameter count.  Compare each parameter.
            for (var i = 0; i < x.Arity; i++)
            {
                var xArg = x.TypeArgumentList.Arguments[i];
                var yArg = y.TypeArgumentList.Arguments[i];

                compare = this.TypeComparer.Compare(xArg, yArg);

                if (compare != 0)
                {
                    return compare;
                }
            }

            return 0;
        }
    }
}