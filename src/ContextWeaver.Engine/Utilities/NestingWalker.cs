using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContextWeaver.Utilities;

/// <summary>
///     Calcula la profundidad máxima de anidamiento de un bloque de código.
///     Útil para detectar complejidad cognitiva que puede confundir a un LLM.
/// </summary>
public class NestingWalker : CSharpSyntaxWalker
{
    private int _currentDepth;
    private int _maxDepth;

    public int MaxDepth => _maxDepth;

    public override void Visit(SyntaxNode? node)
    {
        if (node == null)
            return;

        var isNestingNode = IsNestingNode(node);

        if (isNestingNode)
        {
            _currentDepth++;
            if (_currentDepth > _maxDepth)
                _maxDepth = _currentDepth;
        }

        base.Visit(node);

        if (isNestingNode)
        {
            _currentDepth--;
        }
    }

    private bool IsNestingNode(SyntaxNode node)
    {
        return node is IfStatementSyntax
               or ForStatementSyntax
               or ForEachStatementSyntax
               or WhileStatementSyntax
               or DoStatementSyntax
               or SwitchStatementSyntax
               or TryStatementSyntax
               or UsingStatementSyntax
               or LockStatementSyntax
               or ParenthesizedLambdaExpressionSyntax
               or SimpleLambdaExpressionSyntax
               or AnonymousMethodExpressionSyntax;
    }
}
