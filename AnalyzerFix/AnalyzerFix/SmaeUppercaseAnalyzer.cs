using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerFix
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class SmaeUppercaseAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "SyntaxAnalyzer0002";
		internal const string Title = "Attribute name contains uppercase letter(s)";
		internal const string MessageFormat = "Attribute name '{0}' contains uppercase letter(s)";
		internal const string Category = ".NET Compiler Demo";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(AnalyzeSmae, SyntaxKind.SimpleMemberAccessExpression);
		}

		private static void AnalyzeSmae(SyntaxNodeAnalysisContext context)
		{
			SyntaxNode node = context.Node;
			if (!node.IsKind(SyntaxKind.SimpleMemberAccessExpression)) return;

			MemberAccessExpressionSyntax mae = (MemberAccessExpressionSyntax)node;

			//account.Attributes.Add("Name", "test");
			IdentifierNameSyntax identifier = mae.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
			if (identifier == null) return;

			ITypeSymbol type = context.SemanticModel.GetTypeInfo(identifier).Type;
			if (type == null) return;

			//If type is not Microsoft.Xrm.Sdk.Entity - exit
			if (type.ToString() != "Microsoft.Xrm.Sdk.Entity") return;

			//If argument does not contain an uppercase letter - exit
			var arguments = mae.Parent.DescendantNodes().OfType<ArgumentSyntax>();
			if (!arguments.Any()) return;

			if (!arguments.First().Expression.GetFirstToken().ValueText.ToCharArray().Any(char.IsUpper)) return;

			var diagnostic = Diagnostic.Create(Rule, arguments.First().GetLocation(), arguments.First().Expression.GetFirstToken().ValueText);
			context.ReportDiagnostic(diagnostic);
		}
	}
}