using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace JsonAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class JsonAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "JsonAnalyzer";

        private const string JsonObjectAttribute = "JsonObject";
        private const string JsonPropertyAttribute = "JsonProperty";
        private const string JsonIgnoreAttribute = "JsonIgnore";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.ClassDeclaration);
        }

        //private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        //{
        //    var classDeclaration = (ClassDeclarationSyntax)context.Node;
        //    if (!classDeclaration.AttributeLists.Any())
        //    {
        //        return;
        //    }

        //    if (!CheckAttributeName(classDeclaration.AttributeLists, new[] { JsonObjectAttribute }))
        //    {
        //        return;
        //    }

        //    var properties = classDeclaration.Members.OfType<PropertyDeclarationSyntax>();
        //    foreach (var propertyDeclarationSyntax in properties)
        //    {
        //        var modifier = propertyDeclarationSyntax.Modifiers.First();
        //        if (!modifier.IsKind(SyntaxKind.PublicKeyword))
        //        {
        //            continue;
        //        }

        //        var accessors = propertyDeclarationSyntax.AccessorList.Accessors;
        //        if (accessors.Count != 2)
        //            continue;

        //        var getAccessor = accessors[0];
        //        var setAccessor = accessors[1];

        //        if (getAccessor.Modifiers.Any() || setAccessor.Modifiers.Any())
        //        {
        //            continue;
        //        }

        //        if (CheckAttributeName(propertyDeclarationSyntax.AttributeLists, new[] { JsonPropertyAttribute, JsonIgnoreAttribute }))
        //        {
        //            continue;
        //        }

        //        context.ReportDiagnostic(Diagnostic.Create(Rule, propertyDeclarationSyntax.Identifier.GetLocation()));
        //    }
        //}

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            if (!classDeclaration.AttributeLists.Any())
            {
                return;
            }

            if (!Helper.CheckAttributeName(classDeclaration.AttributeLists, new[] { JsonObjectAttribute }))
            {
                return;
            }

            var result = Helper.GetProperties(classDeclaration);
            if (result.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
            }
        }
    }
}