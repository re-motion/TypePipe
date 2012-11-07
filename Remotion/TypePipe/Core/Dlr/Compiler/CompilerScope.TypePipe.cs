/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Linq;
using System.Reflection;
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