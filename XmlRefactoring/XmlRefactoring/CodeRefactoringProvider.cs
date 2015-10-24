using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace XmlRefactoring
{
	[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(XmlRefactoringCodeRefactoringProvider)), Shared]
	internal class XmlRefactoringCodeRefactoringProvider : CodeRefactoringProvider
	{
		public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root.FindNode(context.Span);

			if (node.IsKind(SyntaxKind.Argument))
			{
				var argSyntax = (ArgumentSyntax)node;
				if (argSyntax == null) return;

				//Exit if not valid XML
				if (!IsXml(argSyntax.Expression.GetFirstToken().ValueText)) return;

				var action = CodeAction.Create("Format XML", c => FormatXmlAsync(context.Document, argSyntax, c));
				context.RegisterRefactoring(action);
			}

			if (node.IsKind(SyntaxKind.StringLiteralExpression))
			{
				var sleSyntax = (LiteralExpressionSyntax)node;
				if (sleSyntax == null) return;

				//Exit if not valid XML
				var text = sleSyntax.GetFirstToken().ValueText;
				if (!IsXml(text)) return;

				var action = CodeAction.Create("Format XML", c => FormatXmlAsync(context.Document, sleSyntax, c));
				context.RegisterRefactoring(action);
			}
		}

		private async Task<Document> FormatXmlAsync(Document document, LiteralExpressionSyntax sleSyntax, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var tree = root.SyntaxTree;

			//Get the character position of the LiteralExpressionSyntax
			FileLinePositionSpan position = tree.GetLineSpan(sleSyntax.Span);
			int cSpace = position.StartLinePosition.Character;

			//Figure out the preceeding trivia since we can't get the column position from GetLineSpan
			var parentTrivia = sleSyntax.GetLeadingTrivia().ToFullString();

			var xml = sleSyntax.GetFirstToken().ValueText;
			var newXmlText = FormatXml(xml);

			//Process each line of the formatted XML and prepend the parent trivia & spaces
			string[] xmlLines = newXmlText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			for (int i = 1; i < xmlLines.Length; i++)
			{
				xmlLines[i] = parentTrivia + new String(' ', (cSpace + 2)) + xmlLines[i];
			}

			newXmlText = String.Join("\r\n", xmlLines);
			var newXmlValue = "@\"" + newXmlText + "\"";

			var newNode = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
				SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.StringLiteralToken, newXmlValue, newXmlText, SyntaxTriviaList.Empty));
			var newRoot = root.ReplaceNode(sleSyntax, newNode);
			return document.WithSyntaxRoot(newRoot);
		}

		private async Task<Document> FormatXmlAsync(Document document, ArgumentSyntax argSntax, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var tree = root.SyntaxTree;

			//Get the character position of the ArgumentSyntax
			FileLinePositionSpan position = tree.GetLineSpan(argSntax.Span);
			int cSpace = position.StartLinePosition.Character;

			//Get the parent VariableDeclaration to figure out the preceeding trivia 
			var parent = argSntax.Ancestors().FirstOrDefault(t => t.IsKind(SyntaxKind.VariableDeclaration));
			var parentTrivia = parent.GetLeadingTrivia().ToFullString();

			var xml = argSntax.Expression.GetFirstToken().ValueText;
			var newXml = FormatXml(xml);

			//Process each line of the formatted XML and prepend the parent trivia & spaces
			string[] xmlLines = newXml.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
			for (int i = 1; i < xmlLines.Length; i++)
			{
				xmlLines[i] = parentTrivia + new String(' ', (cSpace - 14)) + xmlLines[i];
			}

			newXml = String.Join("\r\n", xmlLines);
			newXml = "@\"" + newXml + "\"";

			var newNode = SyntaxFactory.Argument(null, SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.IdentifierName(newXml));
			var newRoot = root.ReplaceNode(argSntax, newNode);
			return document.WithSyntaxRoot(newRoot);
		}

		private bool IsXml(string xml)
		{
			if (string.IsNullOrEmpty(xml)) return false;
			xml = xml.Trim();
			if (!xml.StartsWith("<") && !xml.EndsWith(">")) return false;
			try
			{
				XDocument doc = XDocument.Parse(xml);
			}
			catch (XmlException)
			{
				return false;
			}

			return true;
		}

		private string FormatXml(string xml)
		{
			try
			{
				XDocument doc = XDocument.Parse(xml);
				return doc.ToString().Replace("\"", "'");
			}
			catch (XmlException)
			{
				return xml;
			}
		}
	}
}