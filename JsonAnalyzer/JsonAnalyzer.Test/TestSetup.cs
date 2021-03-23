using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Newtonsoft.Json;

namespace JsonAnalyzer.Test
{
    public static class TestSetup
    {
        public static async Task<(ImmutableArray<Diagnostic> diagnostics, Document document, Workspace workspace)> SetupAsync(string sourceCodeFile)
        {
            var workSpace = new AdhocWorkspace();

            var solution = workSpace.CurrentSolution;

            var projectId = ProjectId.CreateNewId();

            solution = solution.AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp);
            var documentId = DocumentId.CreateNewId(projectId);

            var sourceCode = await File.ReadAllTextAsync(sourceCodeFile);
            solution = solution.AddDocument(documentId, "File.cs", sourceCode);

            var project = solution.GetProject(projectId);
            project = project!.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            project = project.AddMetadataReferences(GetAllReferences(typeof(JsonObjectAttribute)));
            if (!workSpace.TryApplyChanges(project.Solution))
                throw new Exception("Changes was not applied");

            var compilation = await project.GetCompilationAsync();
            var compilationWithAnalyzer = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new JsonAnalyzerAnalyzer()));

            var diagnostic = await compilationWithAnalyzer.GetAllDiagnosticsAsync();

            return (diagnostic, workSpace.CurrentSolution.GetDocument(documentId), workSpace);
        }

        private static MetadataReference[] GetAllReferences(Type type)
        {
            var files = GetAllAssemblyFiles(type);

            return files.Select(s => MetadataReference.CreateFromFile(s)).Cast<MetadataReference>().ToArray();
        }

        private static ImmutableArray<string> GetAllAssemblyFiles(Type type)
        {
            return type.Assembly.GetReferencedAssemblies()
                .Select(name => Assembly.Load(name.FullName))
                .Append(type.Assembly)
                .Select(assembly => assembly.Location)
                .ToImmutableArray();
        }
    }
}