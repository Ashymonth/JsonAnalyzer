using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace JsonAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(JsonAnalyzerCodeFixProvider)), Shared]
    public class JsonAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string JsonPropertyAttribute = "JsonProperty";
        private const string WhiteSpaceFormat = "        ";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(JsonAnalyzerAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var classDeclaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedDocument: c => CreateJsonAttributes(context.Document, classDeclaration, root),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        private static Task<Document> CreateJsonAttributes(Document document, ClassDeclarationSyntax classDeclarationSyntax, SyntaxNode root)
        {
            var propertiesWithoutAttribute = Helper.GetProperties(classDeclarationSyntax);
            var propertiesWithAddedAttribute = propertiesWithoutAttribute.Select((property, i) =>
                property.WithAttributeLists(AddAttribute(ToJsonCase(property.Identifier.ValueText), i == 0))).Cast<MemberDeclarationSyntax>().ToArray();

            var list = new List<MemberDeclarationSyntax>();
            foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>())
            {
                var propertyToUpdate = propertiesWithAddedAttribute.OfType<PropertyDeclarationSyntax>()
                    .FirstOrDefault(syntax => syntax.Identifier.ValueText == memberDeclarationSyntax.Identifier.ValueText);

                list.Add(propertyToUpdate ?? memberDeclarationSyntax);
            }

            root = root.ReplaceNode(classDeclarationSyntax,
                classDeclarationSyntax.WithMembers(new SyntaxList<MemberDeclarationSyntax>(list)));

            return Task.FromResult(document.WithSyntaxRoot(root));
        }

        private static SyntaxList<AttributeListSyntax> AddAttribute(string literal, bool insertTrivia = false)
        {
            var attribute =
                SingletonList(
                    AttributeList(
                            SingletonSeparatedList(
                                Attribute(
                                        IdentifierName(JsonPropertyAttribute))
                                    .WithArgumentList(
                                        AttributeArgumentList(
                                            SingletonSeparatedList(
                                                AttributeArgument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal(literal))))))))
                        .WithOpenBracketToken(
                            Token(
                                TriviaList(LineFeed, Whitespace(WhiteSpaceFormat)),
                                SyntaxKind.OpenBracketToken,
                                TriviaList())));

            return attribute;
        }

        private static string ToJsonCase(string text) => char.ToLowerInvariant(text[0]) + text.Substring(1);
    }
}