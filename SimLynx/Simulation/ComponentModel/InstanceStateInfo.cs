using System.Reflection;

namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Information about a state member.
/// </summary>
/// <param name="Key">The state's key</param>
/// <param name="Member">The member info of the state field or property.</param>
public record InstanceStateInfo(TypeKey Key, MemberInfo Member);
