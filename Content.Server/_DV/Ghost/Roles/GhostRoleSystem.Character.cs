using Content.Server.Administration.Commands;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Preferences.Managers;
using Content.Server.Station.Systems;
using Content.Shared.Mind.Components;
using Content.Shared.Preferences;

namespace Content.Server.Ghost.Roles
{
    public sealed partial class GhostRoleSystem
    {
        [Dependency] private readonly IServerPreferencesManager _prefs = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private void OnSpawnerTakeCharacter(Entity<GhostRoleCharacterSpawnerComponent> ent,
            ref TakeGhostRoleEvent args)
        {
            var uid = ent.Owner;
            var component = ent.Comp;

            if (!TryComp(uid, out GhostRoleComponent? ghostRole) ||
                ghostRole.Taken)
            {
                args.TookRole = false;
                return;
            }

            var character = (HumanoidCharacterProfile) _prefs.GetPreferences(args.Player.UserId).SelectedCharacter;

            var mob = _entityManager.System<StationSpawningSystem>()
                .SpawnPlayerMob(Transform(uid).Coordinates, null, character, null);
            _transform.AttachToGridOrMap(mob);

            var spawnedEvent = new GhostRoleSpawnerUsedEvent(uid, mob);
            RaiseLocalEvent(mob, spawnedEvent);

            EnsureComp<MindContainerComponent>(mob);

            GhostRoleInternalCreateMindAndTransfer(args.Player, uid, mob, ghostRole);

            SetOutfitCommand.SetOutfit(mob, component.OutfitPrototype, _entityManager);

            EntityManager.AddComponents(mob, component.AddedComponents);

            if (++component.CurrentTakeovers < component.AvailableTakeovers)
            {
                args.TookRole = true;
                return;
            }

            ghostRole.Taken = true;

            if (component.DeleteOnSpawn)
                QueueDel(uid);

            args.TookRole = true;
        }
    }
}
