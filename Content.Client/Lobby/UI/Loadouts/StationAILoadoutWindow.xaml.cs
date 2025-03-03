using System.Linq;
using Content.Client.Silicons.Laws.Ui;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.StationAi;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI.Loadouts;

// ADT File
[GenerateTypedNameReferences]
public sealed partial class StationAILoadoutWindow : BaseLoadoutWindow, ILoadoutOverride
{
    public Action<KeyValuePair<string, string>>? OnValueChanged { get; set; }
    public HumanoidCharacterProfile? Profile { get; set; }
    private SiliconLawsetPrototype? _lawset;

    public StationAILoadoutWindow()
    {
        RobustXamlLoader.Load(this);
        SaveNameButton.OnPressed += args => OnValueChanged?.Invoke(new(SharedStationAiSystem.ExtraLoadoutNameId, RoleNameEdit.Text));
        Title = Loc.GetString("station-ai-customization-window");
    }

    public void Refresh(HumanoidCharacterProfile? profile, RoleLoadout loadout, IPrototypeManager protoMan)
    {
        Profile = profile;
        PopulateSkins(protoMan, loadout);
        PopulateLaws(protoMan, loadout);
        PopulateName(loadout);
    }

    private void PopulateSkins(IPrototypeManager protoMan, RoleLoadout loadout)
    {
        SkinList.DisposeAllChildren();
        var list = protoMan.EnumeratePrototypes<StationAIScreenPrototype>()
                            .Where(x => x.Roundstart)
                            .OrderBy(x => x.Priority).ThenByDescending(x => x.ID)
                            .ToList();

        var screen = "Default";
        if (loadout.ExtraData.TryGetValue(SharedStationAiSystem.ExtraLoadoutScreenId, out var screenId))
            screen = screenId;

        foreach (var item in list)
        {
            var button = new Button()
            {
                StyleClasses = { StyleNano.ButtonSquare },
                Margin = new(4),
                SetSize = new(96, 96),
                ToggleMode = true,
                MuteSounds = true,
                Pressed = screen == item.ID
            };
            button.OnPressed += args =>
            {
                UserInterfaceManager.ClickSound();  // To avoid click spamming
                OnValueChanged?.Invoke(new(SharedStationAiSystem.ExtraLoadoutScreenId, item.ID));
            };

            // Screen preview
            var sprite = new ScaledAnimatedTextureRect()
            {
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
                Margin = new Thickness(2),
            };
            sprite.SetFromSpriteSpecifier(item.Icon, new(2, 2));
            button.AddChild(sprite);

            SkinList.AddChild(button);
        }
    }

    private void PopulateLaws(IPrototypeManager protoMan, RoleLoadout loadout)
    {
        Laws.DisposeAllChildren();
        LawsPreview.DisposeAllChildren();
        var list = protoMan.EnumeratePrototypes<SiliconLawsetPrototype>()
                            .Where(x => x.Roundstart)
                            .OrderBy(x => x.Priority).ThenByDescending(x => $"lawset-{x.ID}-name")
                            .ToList();
        var lawset = "Crewsimov";
        if (loadout.ExtraData.TryGetValue(SharedStationAiSystem.ExtraLoadoutLawsetId, out var lawsetId))
            lawset = lawsetId;

        foreach (var item in list)
        {
            if (lawset == item.ID)
                _lawset = item;

            var button = new Button()
            {
                Text = Loc.GetString($"lawset-{item.ID}-name"),
                Margin = new(4),
                HorizontalExpand = true,
                ToggleMode = true,
                MuteSounds = true,
                Pressed = lawset == item.ID
            };

            button.OnPressed += args =>
            {
                UserInterfaceManager.ClickSound();  // To avoid click spamming
                OnValueChanged?.Invoke(new(SharedStationAiSystem.ExtraLoadoutLawsetId, item.ID));
            };

            Laws.AddChild(button);
        }

        if (_lawset == null)
            return;

        // Populate lawset preview
        foreach (var lawId in _lawset.Laws)
        {
            var law = protoMan.Index<SiliconLawPrototype>(lawId);
            var display = new LawDisplay(EntityUid.Invalid, law, null)
            {
                Margin = new(4)
            };

            LawsPreview.AddChild(display);
        }
    }

    private void PopulateName(RoleLoadout loadout)
    {
        RoleNameEdit.Clear();
        if (loadout.ExtraData.TryGetValue(SharedStationAiSystem.ExtraLoadoutNameId, out var name))
            RoleNameEdit.SetText(name);
    }
}
