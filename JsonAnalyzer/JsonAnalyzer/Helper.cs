using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace JsonAnalyzer
{
    public static class Helper
    {
        private const string JsonPropertyAttribute = "JsonProperty";
        private const string JsonIgnoreAttribute = "JsonIgnore";

        public static IEnumerable<PropertyDeclarationSyntax> GetProperties(ClassDeclarationSyntax classDeclaration)
        {
            var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
            foreach (var propertyDeclarationSyntax in properties)
            {
                var modifier = propertyDeclarationSyntax.Modifiers.First();
                if (!modifier.IsKind(SyntaxKind.PublicKeyword))
                {
                    continue;
                }

                var accessors = propertyDeclarationSyntax.AccessorList.Accessors;
                if (accessors.Count != 2)
                    continue;

                var getAccessor = accessors[0];
                var setAccessor = accessors[1];

                if (getAccessor.Modifiers.Any() || setAccessor.Modifiers.Any())
                {
                    continue;
                }

                if (CheckAttributeName(propertyDeclarationSyntax.AttributeLists, new[] { JsonPropertyAttribute, JsonIgnoreAttribute }))
                {
                    continue;
                }

                yield return propertyDeclarationSyntax;
            }
        }

        public static bool CheckAttributeName(SyntaxList<AttributeListSyntax> attributes, IEnumerable<string> attributeNames)
        {
            var names = attributes.FirstOrDefault()?.Attributes.Select(syntax => syntax.Name.ToFullString()).ToArray();
            if (names == null || !names.Any())
            {
                return false;
            }

            var existedAttributes = names.Where(attributeNames.Contains);

            return existedAttributes.Any();
        }
    }
}