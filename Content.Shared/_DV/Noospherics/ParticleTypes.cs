
namespace Content.Shared._DV.Noospherics;

public enum ParticleType
{
    Delta,
    Epsilon,
    Zeta,
    Omega,
}

public sealed class NoosphericParticlesHolder<T>
{
    private readonly Dictionary<ParticleType, T> _particles = [];

    public T this[ParticleType type]
    {
        get => _particles[type];
        set => _particles[type] = value;
    }

    public NoosphericParticlesHolder(T initialValue)
    {
        _particles = new()
        {
            { ParticleType.Delta, initialValue },
            { ParticleType.Epsilon, initialValue },
            { ParticleType.Zeta, initialValue },
            { ParticleType.Omega, initialValue }
        };
    }
}
