using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ContextWeaver.Utilities;

/// <summary>
///     BUENA PRÁCTICA: Clase de Utilidad Estática.
///     Contiene lógica pura y sin estado para calcular métricas de código C#.
///     El uso de la API de Roslyn es una técnica avanzada para el análisis estático.
/// </summary>
public static class CSharpMetricsCalculator
{
    /// <summary>
    ///     Calcula la complejidad ciclomática de un nodo de sintaxis dado.
    ///     Comienza con una complejidad base de 1 y suma 1 por cada sentencia de flujo de control.
    /// </summary>
    /// <param name="root">El nodo raíz de sintaxis a analizar.</param>
    /// <returns>La complejidad ciclomática calculada.</returns>
    public static int CalculateCyclomaticComplexity(SyntaxNode root)
    {
        if (root == null)
            return 1;

        var walker = new ComplexityWalker();
        walker.Visit(root);

        return walker.Complexity;
    }

    /// <summary>
    ///     Calcula la profundidad máxima de anidamiento de un nodo de sintaxis dado.
    /// </summary>
    /// <param name="root">El nodo raíz de sintaxis a analizar.</param>
    /// <returns>La profundidad máxima de anidamiento encontrada.</returns>
    public static int CalculateMaxNestingDepth(SyntaxNode root)
    {
        var walker = new NestingWalker();
        walker.Visit(root);
        return walker.MaxDepth;
    }

    private sealed class ComplexityWalker : CSharpSyntaxWalker
    {
        public int Complexity { get; private set; } = 1;

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Complexity++;
            base.VisitIfStatement(node);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Complexity++;
            base.VisitForEachStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Complexity++;
            base.VisitForStatement(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Complexity++;
            base.VisitWhileStatement(node);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            Complexity++;
            base.VisitCaseSwitchLabel(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Complexity++;
            base.VisitConditionalExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.LogicalAndExpression) || node.IsKind(SyntaxKind.LogicalOrExpression))
                Complexity++;
            base.VisitBinaryExpression(node);
        }
    }
}
