using Microsoft.Scripting.Ast;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Defines an interface for classes handling <see cref="ITypePipeExpression"/> instances.
  /// </summary>
  public interface ITypePipeExpressionVisitor
  {
    Expression VisitThis (ThisExpression expression);
    Expression VisitOriginalBody (OriginalBodyExpression expression);
    Expression VisitMethodAddress (MethodAddressExpression expression);
    Expression VisitVirtualMethodAddress (VirtualMethodAddressExpression expression);
  }
}