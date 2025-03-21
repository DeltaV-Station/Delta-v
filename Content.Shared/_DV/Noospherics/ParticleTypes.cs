namespace Content.Shared._DV.Noospherics;

public enum ParticleType
{
    Delta,
    Epsilon,
    Zeta,
    Omega,
}

public sealed class NoosphericParticleEnergy
{
    private Dictionary<ParticleType, float> _particles = [];

    public float this[ParticleType type]
    {
        get => _particles[type];
        set => _particles[type] = value;
    }

    public IEnumerator<KeyValuePair<ParticleType, float>> GetEnumerator()
    {
        return _particles.GetEnumerator();
    }

    public void AddToAll(float value)
    {
        foreach (var item in _particles)
        {
            _particles[item.Key] += value;
        }
    }

    public float Average()
    {
        var total = 0f;
        foreach (var item in _particles)
        {
            total += item.Value;
        }

        return total;
    }

    public static NoosphericParticleEnergy operator +(
        NoosphericParticleEnergy lhs,
        NoosphericParticleEnergy rhs)
    {
        var result = lhs;
        foreach (var item in result._particles)
        {
            result._particles[item.Key] += rhs._particles[item.Key];
        }

        return result;
    }

    public static NoosphericParticleEnergy operator *(
        NoosphericParticleEnergy energy,
        float value)
    {
        var result = energy;
        foreach (var item in result._particles)
        {
            result._particles[item.Key] *= value;
        }

        return result;
    }

    public NoosphericParticleEnergy() : this(0f) { }

    public NoosphericParticleEnergy(float initialValue)
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
