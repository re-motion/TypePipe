using Microsoft.Scripting.Ast;

namespace Remotion.TypePipe.Expressions
{
  /// <summary>
  /// Identifies an infrastructure <see cref="Expression"/> defined by the TypePipe that can be handled using an 
  /// <see cref="ITypePipeExpressionVisitor"/>.
  /// </summary>
  public interface ITypePipeExpression
  {
    Expression Accept (ITypePipeExpressionVisitor visitor);
  }
}