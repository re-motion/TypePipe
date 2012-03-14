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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="ITypeModificationHandler"/> by applying the modifications made to a <see cref="MutableType"/> to a subclass proxy.
  /// </summary>
  public class TypeModificationHandler : ITypeModificationHandler
  {
    private readonly ITypeBuilder _subclassProxyBuilder;

    public TypeModificationHandler (ITypeBuilder subclassProxyBuilder)
    {
      _subclassProxyBuilder = ArgumentUtility.CheckNotNull ("subclassProxyBuilder", subclassProxyBuilder);
    }

    public ITypeBuilder SubclassProxyBuilder
    {
      get { return _subclassProxyBuilder; }
    }

    public void HandleAddedInterface (Type addedInterface)
    {
      ArgumentUtility.CheckNotNull ("addedInterface", addedInterface);
      _subclassProxyBuilder.AddInterfaceImplementation (addedInterface);
    }

    public void HandleAddedField (MutableFieldInfo addedField)
    {
      ArgumentUtility.CheckNotNull ("addedField", addedField);
      _subclassProxyBuilder.DefineField (addedField.Name, addedField.FieldType, addedField.Attributes);
    }

    public void HandleAddedConstructor (MutableConstructorInfo addedConstructor)
    {
      throw new NotImplementedException ("TODO 4686");
    }
  }
}