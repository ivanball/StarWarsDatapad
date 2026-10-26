using Datapad.Client.Services;
using Datapad.Contracts;
using Microsoft.AspNetCore.Components;

namespace Datapad.Client.Components;

public sealed partial class CharacterDetailsPanel
{
    private CancellationTokenSource? _currentLoad;
    private int? _loadedId;
    private CharacterDetails? _details;
    private string? _error;
    private bool _notFound;

    /// <summary>The selected character, or null when nothing is selected.</summary>
    [Parameter]
    public int? Id { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // The parent re-renders on every search keystroke; only a different character triggers a load.
        if (Id == _loadedId)
        {
            return;
        }

        _loadedId = Id;
        await LoadAsync();
    }

    /// <summary>
    /// Loads the current <see cref="Id"/>. Starting a new load cancels the previous one, and a response that arrives
    /// after a newer load started is dropped, so clicking quickly through the list never shows an older character.
    /// </summary>
    private async Task LoadAsync()
    {
        CancelCurrentLoad();
        _details = null;
        _error = null;
        _notFound = false;

        if (Id is not int id)
        {
            return;
        }

        var load = _currentLoad = new CancellationTokenSource();
        try
        {
            var details = await Api.GetCharacterAsync(id, load.Token);
            if (load == _currentLoad)
            {
                _details = details;
            }
        }
        catch (DatapadApiException ex)
        {
            if (load == _currentLoad)
            {
                _error = ex.Message;
                _notFound = ex.IsNotFound;
            }
        }
        catch (OperationCanceledException) when (load.IsCancellationRequested)
        {
            // Superseded by a newer selection or the component was disposed.
        }
    }

    private bool IsUnavailable(DetailSection section) => _details?.UnavailableSections.Contains(section) == true;

    private static List<(string Label, string Value)> SpeciesFacts(SpeciesInfo species) =>
    [
        ("Classification:", Display.Text(species.Classification)),
        ("Designation:", Display.Text(species.Designation)),
        ("Average height:", Display.Number(species.AverageHeight, " cm")),
        ("Average lifespan:", Display.Number(species.AverageLifespan, " yr")),
        ("Language:", Display.Text(species.Language)),
    ];

    private static List<(string Label, string Value)> PlanetFacts(PlanetInfo planet) =>
    [
        ("Population:", Display.Number(planet.Population)),
        ("Terrain:", Display.Text(planet.Terrain)),
        ("Climate:", Display.Text(planet.Climate)),
        ("Surface water:", Display.Number(planet.SurfaceWater, "%")),
        ("Diameter:", Display.Number(planet.Diameter, " km")),
        ("Rotation period:", Display.Number(planet.RotationPeriod, " hr")),
        ("Orbital period:", Display.Number(planet.OrbitalPeriod, " d")),
        ("Gravity:", Display.Text(planet.Gravity)),
    ];

    private void CancelCurrentLoad()
    {
        if (_currentLoad is null)
        {
            return;
        }

        _currentLoad.Cancel();
        _currentLoad.Dispose();
        _currentLoad = null;
    }

    public void Dispose() => CancelCurrentLoad();
}
