using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

// ReSharper disable CheckNamespace
namespace Microsoft.Scripting.Ast.Compiler
// ReSharper restore CheckNamespace
{
  partial class CompilerScope
  {
    private static ConstructorInfo GetStrongBoxConstructor (Type constructedBoxType)
    {
      ArgumentUtility.CheckNotNull ("constructedBoxType", constructedBoxType);

      return constructedBoxType.IsRuntimeType()
                 ? constructedBoxType.GetConstructors().Single()
                 : new ConstructorOnTypeInstantiation (constructedBoxType, typeof (StrongBox<>).GetConstructors().Single());
    }

    private static FieldInfo GetStrongBoxValueField (Type constructedBoxType)
    {
      ArgumentUtility.CheckNotNull ("constructedBoxType", constructedBoxType);

      return constructedBoxType.IsRuntimeType()
                 ? constructedBoxType.GetField ("Value")
                 : new FieldOnTypeInstantiation (constructedBoxType, typeof (StrongBox<>).GetField ("Value"));
    }
  }
}