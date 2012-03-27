using Microsoft.Scripting.Ast;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Defines an interface for classes handling <see cref="ITypePipeExpression"/> instances.
  /// </summary>
  public interface ITypePipeExpressionVisitor
  {
    Expression VisitThisExpression (ThisExpression expression);
    Expression VisitTypeAsUnderlyingSystemTypeExpression (TypeAsUnderlyingSystemTypeExpression expression);
    Expression VisitOriginalBodyExpression (OriginalBodyExpression expression);
  }
}