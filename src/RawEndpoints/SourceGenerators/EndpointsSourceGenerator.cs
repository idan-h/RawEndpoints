using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RawEndpoints.SourceGenerators
{
    [Generator]
    public class RawEndpointsSourceGenerator : ISourceGenerator
    {
        private const string _runMethodName = "Run";
        private static readonly string[] _httpMethods = { "Get", "Post", "Put", "Patch", "Delete" };

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            var interfaceSymbol = context.Compilation.GetTypeByMetadataName($"RawEndpoints.IEndpoint");
            var nullableEnabled = context.Compilation.Options.NullableContextOptions != NullableContextOptions.Disable;
            var endpointClassesFullNames = new List<string>();

            foreach (var classDeclaration in receiver.ClassesToAugment)
            {
                var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                if (!(model.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
    "GEN001", "Source Generator", $"Injected method into class: {classSymbol.Name}",
    "SourceGenerator", DiagnosticSeverity.Info, true), Location.None));

                if (!classSymbol.AllInterfaces.Contains(interfaceSymbol))
                {
                    continue;
                }

                // Check for the required method
                var hasRequiredMethod = classSymbol.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == _runMethodName && m.DeclaredAccessibility == Accessibility.Public);

                if (!hasRequiredMethod)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ME001",
                            "Missing Required Method",
                            "Class '{0}' should implement a public method called 'Run'.",
                            "Usage",
                            DiagnosticSeverity.Error,
                            true),
                        classDeclaration.Identifier.GetLocation(),
                        classSymbol.Name));
                }

                // Generate the additional method
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var generatedSource = _generateMapMethods(classSymbol, namespaceName, nullableEnabled);
                context.AddSource($"{classSymbol.Name}_GeneratedMinimalApiMapMethods.cs", SourceText.From(generatedSource, Encoding.UTF8));
                endpointClassesFullNames.Add($"{namespaceName}.{classSymbol.Name}");
            }

            var generatedExtensionClass = _generateMapRawEndpointsExtensionClass(endpointClassesFullNames);
            context.AddSource($"WebApplicationExtensions.cs", SourceText.From(generatedExtensionClass, Encoding.UTF8));
        }

        private static string _generateMapMethods(INamedTypeSymbol classSymbol, string namespaceName, bool nullableEnabled)
        {
            StringBuilder mapMethodsStringBuilder = new StringBuilder();

            foreach (var httpMethod in _httpMethods)
            {
                mapMethodsStringBuilder.Append($@"
        private RouteHandlerBuilder Map{httpMethod}([StringSyntax(""Route"")] string pattern)
        {{
            return _endpointRouteBuilder.Map{httpMethod}(pattern, {_runMethodName});
        }}");
            }

            var className = classSymbol.Name;
            return $@"
using System.Diagnostics.CodeAnalysis;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        private IEndpointRouteBuilder _endpointRouteBuilder{(nullableEnabled ? " = default!" : "")};

        public static {className} InstanceWithEndpointRouteBuilder(IEndpointRouteBuilder endpointRouteBuilder)
        {{
            {className} instance = new {className}();
            instance._endpointRouteBuilder = endpointRouteBuilder;
            return instance;
        }}
{mapMethodsStringBuilder}
    }}
}}
";
        }

        private static string _generateMapRawEndpointsExtensionClass(List<string> endpointClassesFullNames)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var endpointClassName in endpointClassesFullNames)
            {
                sb.AppendLine($@"
            {endpointClassName}.InstanceWithEndpointRouteBuilder(app).Define();");
            }
            return $@"
namespace RawEndpoints.Extensions
{{
    public static class WebApplicationExtensions
    {{
        public static WebApplication MapRawEndpoints(this WebApplication app)
        {{{sb}
            return app;
        }}
    }}
}}
";
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> ClassesToAugment { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    ClassesToAugment.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
