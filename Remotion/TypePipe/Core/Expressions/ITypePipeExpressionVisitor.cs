namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Defines an interface for classes handling <see cref="ITypePipeExpression"/> instances.
  /// </summary>
  public interface ITypePipeExpressionVisitor
  {
    void VisitThisExpression (ThisExpression expression);
    void VisitTypeAsUnderlyingSystemTypeExpression (TypeAsUnderlyingSystemTypeExpression expression);
  }
}