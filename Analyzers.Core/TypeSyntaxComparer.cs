namespace Analyzers.Core
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Copy & paste from https://github.com/dotnet/roslyn/blob/03e30451ce7eb518e364b5806c524623424103e4/src/Workspaces/CSharp/Portable/Utilities/TypeSyntaxComparer.cs
    /// Changed class visibility to public
    /// </summary>
    public class TypeSyntaxComparer : IComparer<TypeSyntax>
    {
        private readonly IComparer<SyntaxToken> _tokenComparer;
        internal IComparer<NameSyntax> NameComparer;

        internal TypeSyntaxComparer(IComparer<SyntaxToken> tokenComparer)
        {
            _tokenComparer = tokenComparer;
        }

        public int Compare(TypeSyntax x, TypeSyntax y)
        {
            if (x == y)
            {
                return 0;
            }

            x = UnwrapType(x);
            y = UnwrapType(y);

            if (x is NameSyntax && y is NameSyntax)
            {
                return NameComparer.Compare((NameSyntax)x, (NameSyntax)y);
            }

            // we have two predefined types, or a predefined type and a normal C# name.  We only need
            // to compare the first tokens here.
            return _tokenComparer.Compare(x.GetFirstToken(includeSkipped: true), y.GetFirstToken());
        }

        private TypeSyntax UnwrapType(TypeSyntax type)
        {
            while (true)
            {
                switch (type.Kind())
                {
                    case SyntaxKind.ArrayType:
                        type = ((ArrayTypeSyntax)type).ElementType;
                        break;
                    case SyntaxKind.PointerType:
                        type = ((PointerTypeSyntax)type).ElementType;
                        break;
                    case SyntaxKind.NullableType:
                        type = ((NullableTypeSyntax)type).ElementType;
                        break;
                    default:
                        return type;
                }
            }
        }
    }
}