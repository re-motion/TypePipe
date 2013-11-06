using System;
using JetBrains.Annotations;
using Remotion.TypePipe.CodeGeneration;

namespace Remotion.TypePipe.TypeAssembly
{
  /// <summary>
  /// Defines the API for holding state created by the participants during code generation. 
  /// The <see cref="IParticipantState"/> is local to a single <see cref="AssemblyContext"/>.
  /// </summary>
  public interface IParticipantState
  {
    /// <summary>
    /// Adds the <paramref name="value"/> into the dictionary using the <paramref name="id"/> as key. If the <paramref name="id"/> already exists, an exception is thrown.
    /// </summary>
    /// <param name="id">The identifier. Must not be <see langword="null" /> or empty.</param>
    /// <param name="value">The value. Must not be <see langword="null" />.</param>
    /// <exception cref="InvalidOperationException">State with the specified <paramref name="id"/> already exists.</exception>
    void AddState ([NotNull] string id, [NotNull] object value);

    /// <summary>
    /// Adds the state value identified by <paramref name="id"/> parameter. If the <paramref name="id"/> does not exist, <see langword="null" /> is returned.
    /// </summary>
    /// <param name="id">The identifier. Must not be <see langword="null" /> or empty.</param>
    /// <returns>The identified state value or <see langword="null" />.</returns>
    [CanBeNull]
    object GetState (string id);
  }
}