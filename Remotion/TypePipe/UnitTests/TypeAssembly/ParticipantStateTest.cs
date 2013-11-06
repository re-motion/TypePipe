using System;
using NUnit.Framework;
using Remotion.TypePipe.TypeAssembly;

namespace Remotion.TypePipe.UnitTests.TypeAssembly
{
  [TestFixture]
  public class ParticipantStateTest
  {
    [Test]
    public void GetState_WithIDForAddedValue_ReturnsValue ()
    {
      var participantState = new ParticipantState();
      var value = new object();
      participantState.AddState ("TheID", value);

      Assert.That (participantState.GetState ("TheID"), Is.SameAs (value));
    }

    [Test]
    public void GetState_WithUnknownID_ReturnsNull ()
    {
      var participantState = new ParticipantState();

      Assert.That (participantState.GetState ("UnknownID"), Is.Null);
    }

    [Test]
    public void AddState_WithExistingID_ThrowsInvalidOperationException ()
    {
      var participantState = new ParticipantState();
      participantState.AddState ("TheID", new object());

      Assert.That (
          () => participantState.AddState ("TheID", new object()),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("State identified by the id 'TheID' already exists. State identifier must be unique."));
    }
  }
}