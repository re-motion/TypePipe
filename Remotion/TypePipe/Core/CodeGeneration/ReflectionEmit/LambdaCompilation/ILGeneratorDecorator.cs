// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// A decorator which adapts emit calls for <see cref="ConstructorAsMethodInfoAdapter"/>.
  /// </summary>
  public class ILGeneratorDecorator : IILGenerator
  {
    private readonly IILGenerator _innerILGenerator;
    private readonly EmittableOperandProvider _emittableOperandProvider;

    [CLSCompliant (false)]
    public ILGeneratorDecorator (IILGenerator innerIlGenerator, EmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("innerIlGenerator", innerIlGenerator);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _innerILGenerator = innerIlGenerator;
      _emittableOperandProvider = emittableOperandProvider;
    }

    [CLSCompliant (false)]
    public IILGenerator InnerILGenerator
    {
      get { return _innerILGenerator; }
    }

    public EmittableOperandProvider EmittableOperandProvider
    {
      get { return _emittableOperandProvider; }
    }

    public int ILOffset
    {
      get { return _innerILGenerator.ILOffset; }
    }

    [CLSCompliant (false)]
    public IILGeneratorFactory GetFactory ()
    {
      return new ILGeneratorDecoratorFactory (_innerILGenerator.GetFactory(), _emittableOperandProvider);
    }

    public void BeginCatchBlock (Type exceptionType)
    {
      _innerILGenerator.BeginCatchBlock (exceptionType);
    }

    public void BeginExceptFilterBlock ()
    {
      _innerILGenerator.BeginExceptFilterBlock();
    }

    public void BeginExceptionBlock ()
    {
      _innerILGenerator.BeginExceptionBlock();
    }

    public void BeginFaultBlock ()
    {
      _innerILGenerator.BeginFaultBlock();
    }

    public void BeginFinallyBlock ()
    {
      _innerILGenerator.BeginFinallyBlock();
    }

    public LocalBuilder DeclareLocal (Type localType)
    {
      return _innerILGenerator.DeclareLocal (localType);
    }

    public Label DefineLabel ()
    {
      return _innerILGenerator.DefineLabel();
    }

    [CLSCompliant (false)]
    public void Emit (OpCode opcode, sbyte arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, long arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, int arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, byte arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, ConstructorInfo con)
    {
      ArgumentUtility.CheckNotNull ("con", con);

      var emittableOperand = _emittableOperandProvider.GetEmittableOperand (con);
      emittableOperand.Emit (_innerILGenerator, opcode);
    }

    public void Emit (OpCode opcode, Type cls)
    {
      _innerILGenerator.Emit (opcode, cls);
    }

    public void Emit (OpCode opcode, float arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, double arg)
    {
      _innerILGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, Label label)
    {
      _innerILGenerator.Emit (opcode, label);
    }

    public void Emit (OpCode opcode, FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("field", field);

      var emittableOperand = _emittableOperandProvider.GetEmittableOperand (field);
      emittableOperand.Emit (_innerILGenerator, opcode);
    }

    public void Emit (OpCode opcode, MethodInfo meth)
    {
      ArgumentUtility.CheckNotNull ("meth", meth);

      var constructorMethodInfo = meth as ConstructorAsMethodInfoAdapter;
      if (constructorMethodInfo != null)
      {
        Emit (opcode, constructorMethodInfo.ConstructorInfo);
        return;
      }

      var baseCallMethodInfo = meth as BaseCallMethodInfoAdapter;
      if (baseCallMethodInfo != null)
      {
        Emit (AdjustOpCodeForBaseCall (opcode), baseCallMethodInfo.AdaptedMethodInfo);
        return;
      }

      var emittableMethodOperand = _emittableOperandProvider.GetEmittableOperand (meth);
      emittableMethodOperand.Emit (_innerILGenerator, opcode);
    }

    public void EmitCall (OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);
      // Optional parameters may be null

      var baseConstructorMethodInfo = methodInfo as ConstructorAsMethodInfoAdapter;
      if (baseConstructorMethodInfo != null)
      {
        if (!ArrayUtility.IsNullOrEmpty (optionalParameterTypes))
          throw new InvalidOperationException ("Constructor calls cannot have optional parameters.");

        Emit (opcode, baseConstructorMethodInfo.ConstructorInfo);
        return;
      }

      var baseCallMethodInfo = methodInfo as BaseCallMethodInfoAdapter;
      if (baseCallMethodInfo != null)
      {
        EmitCall (AdjustOpCodeForBaseCall (opcode), baseCallMethodInfo.AdaptedMethodInfo, optionalParameterTypes);
        return;
      }

      var emittableMethodOperand = _emittableOperandProvider.GetEmittableOperand (methodInfo);
      emittableMethodOperand.EmitCall (_innerILGenerator, opcode, optionalParameterTypes);
    }

   public void Emit (OpCode opcode, Label[] labels)
   {
     _innerILGenerator.Emit (opcode, labels);
   }

   public void Emit (OpCode opcode)
   {
     _innerILGenerator.Emit (opcode);
   }

   public void Emit (OpCode opcode, string str)
   {
     _innerILGenerator.Emit (opcode, str);
   }

   public void Emit (OpCode opcode, LocalBuilder local)
   {
     _innerILGenerator.Emit (opcode, local);
   }

    public void EndExceptionBlock ()
    {
      _innerILGenerator.EndExceptionBlock();
    }

    public void MarkLabel (Label loc)
    {
      _innerILGenerator.MarkLabel (loc);
    }

    public void MarkSequencePoint (ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
    {
      _innerILGenerator.MarkSequencePoint (document, startLine, startColumn, endLine, endColumn);
    }

    private OpCode AdjustOpCodeForBaseCall (OpCode opcode)
    {
      return opcode == OpCodes.Callvirt ? OpCodes.Call : opcode;
    }
  }
}