using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared._White.Medical.Consciousness.Components;
using Content.Shared._White.Medical.Pain;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Consciousness.Systems;

public partial class ConsciousnessSystem
{
    private void InitNet()
    {
        SubscribeLocalEvent<ConsciousnessComponent, ComponentGetState>(OnComponentGet);
        SubscribeLocalEvent<ConsciousnessComponent, ComponentHandleState>(OnComponentHandleState);

        SubscribeLocalEvent<ConsciousnessComponent, MapInitEvent>(OnConsciousnessMapInit);

        SubscribeLocalEvent<ConsciousnessRequiredComponent, ComponentInit>(OnConsciousnessPartInit);

        SubscribeLocalEvent<ConsciousnessRequiredComponent, PainModifierChangedEvent>(OnPainChanged);

        SubscribeLocalEvent<ConsciousnessRequiredComponent, BodyPartAddedEvent>(OnBodyPartAdded);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, BodyPartRemovedEvent>(OnBodyPartRemoved);

        SubscribeLocalEvent<ConsciousnessRequiredComponent, OrganAddedToBodyEvent>(OnOrganAdded);
        SubscribeLocalEvent<ConsciousnessRequiredComponent, OrganRemovedFromBodyEvent>(OnOrganRemoved);
    }

    private void OnComponentHandleState(EntityUid uid, ConsciousnessComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ConsciousnessComponentState state)
            return;

        component.Threshold = state.Threshold;
        component.RawConsciousness = state.RawConsciousness;
        component.Multiplier = state.Multiplier;
        component.Cap = state.Cap;
        component.ForceDead = state.ForceDead;
        component.ForceUnconscious = state.ForceUnconscious;
        component.IsConscious = state.IsConscious;
        component.Modifiers.Clear();
        component.Multipliers.Clear();
        component.RequiredConsciousnessParts.Clear();

        foreach (var ((modEntity, modType), modifier) in state.Modifiers)
        {
            component.Modifiers.Add((EntityManager.GetEntity(modEntity),modType),modifier);
        }

        foreach (var ((multEntity, multType), modifier) in state.Multipliers)
        {
            component.Multipliers.Add((EntityManager.GetEntity(multEntity),multType),modifier);
        }

        foreach (var (id, (entity, causesDeath, isLost)) in state.RequiredConsciousnessParts)
        {
            component.RequiredConsciousnessParts.Add(id, (EntityManager.GetEntity(entity), causesDeath, isLost));
        }
    }

    private void OnComponentGet(EntityUid uid, ConsciousnessComponent comp, ref ComponentGetState args)
    {
        var state = new ConsciousnessComponentState
        {
            Threshold = comp.Threshold,
            RawConsciousness = comp.RawConsciousness,
            Multiplier = comp.Multiplier,
            Cap = comp.Cap,
            ForceDead = comp.ForceDead,
            ForceUnconscious = comp.ForceUnconscious,
            IsConscious = comp.IsConscious,
        };

        foreach (var ((modEntity, modType), modifier) in comp.Modifiers)
        {
            state.Modifiers.Add((EntityManager.GetNetEntity(modEntity),modType),modifier);
        }

        foreach (var ((multEntity, multType), modifier) in comp.Multipliers)
        {
            state.Multipliers.Add((EntityManager.GetNetEntity(multEntity),multType),modifier);
        }

        foreach (var (id, (entity, causesDeath, isLost)) in comp.RequiredConsciousnessParts)
        {
            state.RequiredConsciousnessParts.Add(id, (EntityManager.GetNetEntity(entity), causesDeath, isLost));
        }

        args.State = state;
    }
}
