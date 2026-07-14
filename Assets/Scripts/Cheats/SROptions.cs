using System.ComponentModel;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;

public partial class SROptions
{
    private static PlayerStatsSO _stats;
    private static IHealthService _health;
    private static ILevelCatalog _catalog;

    private int _forcedPresetFlat = -1;

    public static void Bind(PlayerStatsSO stats) => _stats = stats;
    public static void Bind(IHealthService health) => _health = health;
    public static void Bind(ILevelCatalog catalog) => _catalog = catalog;

    [Category("Cheats")]
    [DisplayName("Double Jump")]
    public bool DoubleJump
    {
        get => _stats != null && _stats.AllowDoubleJump;
        set { _stats?.SetAllowDoubleJump(value); OnPropertyChanged(nameof(DoubleJump)); }
    }

    [Category("Cheats")]
    [DisplayName("Knockback")]
    public bool Knockback
    {
        get => _stats != null && _stats.AllowKnockback;
        set { _stats?.SetAllowKnockback(value); OnPropertyChanged(nameof(Knockback)); }
    }

    [Category("Cheats")]
    public void KillPlayer()
    {
        if (_health == null || _health.IsDead) return;
        _health.TakeDamage(_health.MaxHealth, DamageType.Default);
    }

    [Category("Cheats")]
    public void ResetCheats()
    {
        _stats?.SetAllowDoubleJump(false);
        _stats?.SetAllowKnockback(false);
        _forcedPresetFlat = -1;
        OnPropertyChanged(nameof(DoubleJump));
        OnPropertyChanged(nameof(Knockback));
        OnPropertyChanged(nameof(ForcedPreset));
    }

    [Category("Level")]
    [DisplayName("Forced Preset")]
    public string ForcedPreset
    {
        get
        {
            if (_forcedPresetFlat < 0) return "off (random)";

            var entries = _catalog?.Entries;
            if (entries == null || entries.Count == 0)
                return $"#{_forcedPresetFlat} (catalog not bound)";

            int i = ClampFlat(_forcedPresetFlat);
            var entry = entries[i];
            return $"{entry.WorldId} / {entry.Name}  ({i + 1}/{entries.Count})";
        }
    }

    [Category("Level")]
    public void NextPreset() => StepForcedPreset(+1);

    [Category("Level")]
    public void PrevPreset() => StepForcedPreset(-1);

    [Category("Level")]
    public void ForcedPresetOff()
    {
        _forcedPresetFlat = -1;
        OnPropertyChanged(nameof(ForcedPreset));
    }

    public string ResolveForcedWorldId()
    {
        var entries = _catalog?.Entries;
        if (_forcedPresetFlat < 0 || entries == null || entries.Count == 0)
            return string.Empty;

        return entries[ClampFlat(_forcedPresetFlat)].WorldId;
    }

    public int ResolveForcedPresetIndex()
    {
        var entries = _catalog?.Entries;
        if (_forcedPresetFlat < 0 || entries == null || entries.Count == 0)
            return -1;

        return entries[ClampFlat(_forcedPresetFlat)].LocalIndex;
    }

    private void StepForcedPreset(int direction)
    {
        int total = _catalog?.Entries?.Count ?? 0;
        if (total == 0) return;

        int current = _forcedPresetFlat < 0 ? (direction > 0 ? -1 : 0) : _forcedPresetFlat;
        _forcedPresetFlat = (((current + direction) % total) + total) % total;

        OnPropertyChanged(nameof(ForcedPreset));
    }

    private int ClampFlat(int index)
    {
        int total = _catalog?.Entries?.Count ?? 0;
        if (total == 0) return 0;
        return ((index % total) + total) % total;
    }
}
