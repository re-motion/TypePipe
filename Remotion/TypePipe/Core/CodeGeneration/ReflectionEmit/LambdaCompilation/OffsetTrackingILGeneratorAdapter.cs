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
using Microsoft.Scripting.Ast.Compiler;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  /// <summary>
  /// Adapts <see cref="OffsetTrackingILGenerator"/> to implement the <see cref="IILGenerator"/> interface.
  /// </summary>
  [CLSCompliant (false)]
  public class OffsetTrackingILGeneratorAdapter : IILGenerator
  {
    private readonly OffsetTrackingILGenerator _ilGenerator;

    internal OffsetTrackingILGeneratorAdapter (OffsetTrackingILGenerator ilGenerator)
    {
      ArgumentUtility.CheckNotNull ("ilGenerator", ilGenerator);
      _ilGenerator = ilGenerator;
    }

    internal OffsetTrackingILGenerator ILGenerator
    {
      get { return _ilGenerator; }
    }

    public int ILOffset
    {
      get { return _ilGenerator.ILOffset; }
    }

    public IILGeneratorFactory GetFactory ()
    {
      return new OffsetTrackingILGeneratorFactory();
    }

    public void BeginCatchBlock (Type exceptionType)
    {
      _ilGenerator.BeginCatchBlock (exceptionType);
    }

    public void BeginExceptFilterBlock ()
    {
      _ilGenerator.BeginExceptFilterBlock();
    }

    public void BeginExceptionBlock ()
    {
      _ilGenerator.BeginExceptionBlock();
    }

    public void BeginFaultBlock ()
    {
      _ilGenerator.BeginFaultBlock();
    }

    public void BeginFinallyBlock ()
    {
      _ilGenerator.BeginFinallyBlock();
    }

    public LocalBuilder DeclareLocal (Type localType)
    {
      return _ilGenerator.DeclareLocal (localType);
    }

    public Label DefineLabel ()
    {
      return _ilGenerator.DefineLabel();
    }

    public void Emit (OpCode opcode, Label[] labels)
    {
      _ilGenerator.Emit (opcode, labels);
    }

    public void Emit (OpCode opcode, Label label)
    {
      _ilGenerator.Emit (opcode, label);
    }

    public void Emit (OpCode opcode, FieldInfo field)
    {
      _ilGenerator.Emit (opcode, field);
    }

    public void Emit (OpCode opcode, LocalBuilder local)
    {
      _ilGenerator.Emit (opcode, local);
    }

    public void Emit (OpCode opcode, string str)
    {
      _ilGenerator.Emit (opcode, str);
    }

    public void Emit (OpCode opcode, MethodInfo meth)
    {
      _ilGenerator.Emit (opcode, meth);
    }

    public void Emit (OpCode opcode)
    {
      _ilGenerator.Emit (opcode);
    }

    public void Emit (OpCode opcode, int arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, byte arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    [CLSCompliant (false)]
    public void Emit (OpCode opcode, sbyte arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, long arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, float arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, double arg)
    {
      _ilGenerator.Emit (opcode, arg);
    }

    public void Emit (OpCode opcode, ConstructorInfo con)
    {
      _ilGenerator.Emit (opcode, con);
    }

    public void Emit (OpCode opcode, Type cls)
    {
      _ilGenerator.Emit (opcode, cls);
    }

    public void EmitCall (OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
    {
      _ilGenerator.EmitCall (opcode, methodInfo, optionalParameterTypes);
    }

    public void EndExceptionBlock ()
    {
      _ilGenerator.EndExceptionBlock();
    }

    public void MarkLabel (Label loc)
    {
      _ilGenerator.MarkLabel (loc);
    }

    public void MarkSequencePoint (ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
    {
      _ilGenerator.MarkSequencePoint (document, startLine, startColumn, endLine, endColumn);
    }
  }
}