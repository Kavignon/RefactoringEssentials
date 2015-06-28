using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RedundantCatchClauseAnalyzer : DiagnosticAnalyzer
    {
        static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID,
            GettextCatalog.GetString("Catch clause with a single 'throw' statement is redundant"),
            "{0}",
            DiagnosticAnalyzerCategories.RedundanciesInCode,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.RedundantCatchClauseAnalyzerID),
            customTags: DiagnosticCustomTags.Unnecessary
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        // "Remove redundant catch clauses" / "Remove 'catch'" / "'try' statement is redundant" /
        //"Remove all '{0}' redundant 'catch' clauses" / "Remove 'try' statement"
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnostic(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                  SyntaxKind.CatchClause 
            );
        }

        static bool TryGetDiagnostic(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var catchClause = nodeContext.Node as CatchClauseSyntax ;
            var tryStatement = catchClause?.Parent as TryStatementSyntax;
            if (tryStatement == null || tryStatement.Finally != null)
                return false;

            var catchBlock = catchClause.ChildNodes().OfType<BlockSyntax>().First();

            if (catchBlock != null || !catchBlock.ChildNodes().Any())
            {
                return false;
            }

            if (!IsThrowsClause(catchClause))
                return false;

            if (IsRedundant(catchClause, nodeContext))
            {
                diagnostic = Diagnostic.Create(descriptor, catchClause.GetLocation());
                return true;
            }
            return false;
        }

        static bool IsThrowsClause(CatchClauseSyntax catchClause)
        {
            var firstStatement = catchClause..Statements.FirstOrNullObject();
            if (firstStatement == null)
                return false;
            var throwStatement = firstStatement as ThrowStatementSyntax;

            return (throwStatement?.Expression != null);
        }

       static bool IsRedundant(CatchClauseSyntax catchClause, SyntaxNodeAnalysisContext nodeContext)
        {
            if (!IsThrowsClause(catchClause))
                return false;
            var type = nodeContext.SemanticModel.GetTypeInfo(catchClause).ConvertedType;
            var n = catchClause
            while (n != null)
            {
                var nextClause = n as CatchClause;
                if (nextClause != null)
                {
                    if (nextClause.Type.IsNull && !IsThrowsClause(nextClause))
                        return false;
                    if (!IsThrowsClause(nextClause) && type.GetDefinition().IsDerivedFrom(ctx.Resolve(nextClause.Type).Type.GetDefinition()))
                        return false;
                }
                n = n.NextSibling;
            }
            return true;
        }
    }

    //		class GatherVisitor : GatherVisitorBase<RedundantCatchClauseAnalyzer>
    //		{
    //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
    //				: base (semanticModel, addDiagnostic, cancellationToken)
    //			{
    //			}

    ////			public override void VisitTryCatchStatement(TryCatchStatement tryCatchStatement)
    ////			{
    ////				var redundantCatchClauses = new List<CatchClause>();
    ////				bool hasNonRedundantCatch = false;
    ////				foreach (var catchClause in tryCatchStatement.CatchClauses) {
    ////					if (IsRedundant(catchClause)) {
    ////						redundantCatchClauses.Add(catchClause);
    ////					} else {
    ////						hasNonRedundantCatch = true;
    ////					}
    ////				}
    ////
    ////				if (hasNonRedundantCatch || !tryCatchStatement.FinallyBlock.IsNull) {
    ////					AddDiagnosticAnalyzersForClauses(tryCatchStatement, redundantCatchClauses);
    ////				} else {
    ////					AddDiagnosticAnalyzerForTryCatchStatement(tryCatchStatement);
    ////				}
    ////			}
    ////
    ////			void AddDiagnosticAnalyzersForClauses(AstNode node, List<CatchClause> redundantCatchClauses)
    ////			{
    //			//				var allCatchClausesMessage = ctx.TranslateString("");
    ////				var removeAllRedundantClausesAction = new CodeAction(allCatchClausesMessage, script => {
    ////					foreach (var redundantCatchClause in redundantCatchClauses) {
    ////						script.Remove(redundantCatchClause);
    ////					}
    ////				}, node);
    ////				var singleCatchClauseMessage = ctx.TranslateString("");
    ////                var redundantCatchClauseMessage = ctx.TranslateString("");
    ////				foreach (var redundantCatchClause in redundantCatchClauses) {
    ////					var closureLocalCatchClause = redundantCatchClause;
    ////					var removeRedundantClauseAction = new CodeAction(singleCatchClauseMessage, script => {
    ////						script.Remove(closureLocalCatchClause);
    ////					}, node);
    ////					var actions = new List<CodeAction>();
    ////					actions.Add(removeRedundantClauseAction);
    ////					if (redundantCatchClauses.Count > 1) {
    ////						actions.Add(removeAllRedundantClausesAction);
    ////					}
    ////					AddDiagnosticAnalyzer(new CodeIssue(closureLocalCatchClause, redundantCatchClauseMessage, actions) { IssueMarker = IssueMarker.GrayOut });
    ////				}
    ////			}
    ////
    ////			void AddDiagnosticAnalyzerForTryCatchStatement(TryCatchStatement tryCatchStatement)
    ////			{
    ////				var lastCatch = tryCatchStatement.CatchClauses.LastOrNullObject();
    ////				if (lastCatch.IsNull)
    ////					return;
    ////
    ////				var removeTryCatchMessage = ctx.TranslateString("");
    ////
    ////				var removeTryStatementAction = new CodeAction(removeTryCatchMessage, script => {
    ////					var statements = tryCatchStatement.TryBlock.Statements;
    ////					if (statements.Count == 1 || tryCatchStatement.Parent is BlockStatement) {
    ////						foreach (var statement in statements) {
    ////							script.InsertAfter(tryCatchStatement.GetPrevSibling (s => !(s is NewLineNode)), statement.Clone());
    ////						}
    ////						script.Remove(tryCatchStatement);
    ////					} else {
    ////						var blockStatement = new BlockStatement();
    ////						foreach (var statement in statements) {
    ////							blockStatement.Statements.Add(statement.Clone());
    ////						}
    ////						script.Replace(tryCatchStatement, blockStatement);
    ////					}
    ////					// The replace and insert script functions does not format these things well on their own
    ////					script.FormatText(tryCatchStatement.Parent);
    ////				}, tryCatchStatement);
    ////
    ////				var fixes = new [] {
    ////					removeTryStatementAction
    ////				};
    ////				AddDiagnosticAnalyzer(new CodeIssue(tryCatchStatement.TryBlock.EndLocation, lastCatch.EndLocation, removeTryCatchMessage, fixes) { IssueMarker = IssueMarker.GrayOut });
    ////			}
    ////
    ////			static bool IsThrowsClause (CatchClause catchClause)
    ////			{
    ////				var firstStatement = catchClause.Body.Statements.FirstOrNullObject();
    ////				if (firstStatement.IsNull)
    ////					return false;
    ////				var throwStatement = firstStatement as ThrowStatement;
    ////				if (throwStatement == null || !throwStatement.Expression.IsNull)
    ////					return false;
    ////				return true;
    ////			}
    ////
    ////			bool IsRedundant(CatchClause catchClause)
    ////			{
    ////				if (!IsThrowsClause (catchClause))
    ////					return false;
    ////
    ////				var type = ctx.Resolve (catchClause.Type).Type;
    ////				var n = catchClause.NextSibling;
    ////				while (n != null) {
    ////					var nextClause = n as CatchClause;
    ////					if (nextClause != null) {
    ////						if (nextClause.Type.IsNull && !IsThrowsClause(nextClause))
    ////							return false;
    ////						if (!IsThrowsClause(nextClause) && type.GetDefinition ().IsDerivedFrom (ctx.Resolve (nextClause.Type).Type.GetDefinition ()))
    ////							return false;
    ////					}
    ////					n = n.NextSibling;
    ////				}
    ////				return true;
    ////			}
    //		}
}
}

