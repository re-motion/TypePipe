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

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures.TestDomain
{
  public class ClassForSignatureComparisons
  {
    public ClassForSignatureComparisons () { }
    public ClassForSignatureComparisons (int i) { }

    public int M1 () { return 0; }
    public int M2 () { return 0; }
    public int M3 (int i) { return 0; }

    public int P1 { get; set; }
    public int P2 { get; set; }
    public string P3 { get; set; }

    public event EventHandler E1;
    public event EventHandler E2;
    public event EventHandler<EventArgs> E3;

    public string F1;
    public string F2;
    public object F3;
  }
}
