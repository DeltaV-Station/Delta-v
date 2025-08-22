using Content.Server.Emp;
using Content.Shared.Damage;

namespace Content.Server._DV.Silicons;
///<summary>
///This components prevents the normal EMP effects on an entity, and makes it take ion damage instead
///</summary>
[RegisterComponent]
public sealed partial class SiliconEmpComponent : Component;
