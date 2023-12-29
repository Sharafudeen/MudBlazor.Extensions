﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Extensions.Attribute;
using MudBlazor.Extensions.Components.Base;
using MudBlazor.Extensions.Core;
using MudBlazor.Extensions.Helper;
using MudBlazor.Extensions.Helper.Internal;
using MudBlazor.Utilities;
using Nextended.Core.Extensions;

namespace MudBlazor.Extensions.Components;

/// <summary>
/// Simple Card List with a hover effect.
/// </summary>
/// <typeparam name="TData"></typeparam>
public partial class MudExCardList<TData> : MudBaseBindableItemsControl<MudItem, TData>, IJsMudExComponent<MudExCardList<TData>>
{
    private string _id = Guid.NewGuid().ToFormattedId();
    private MudExCardHoverMode? _hoverMode = MudExCardHoverMode.LightBulb | MudExCardHoverMode.Zoom;
    /// <summary>
    /// Dependency Injection for IJSRuntime Service.
    /// </summary>
    [Inject]
    public IJSRuntime JsRuntime { get; set; }

    /// <summary>
    /// Gets or Sets IJSObjectReference JsReference Property.
    /// </summary>
    public IJSObjectReference JsReference { get; set; }

    /// <summary>
    /// Gets or Sets IJSObjectReference ModuleReference Property.
    /// </summary>
    public IJSObjectReference ModuleReference { get; set; }

    /// <summary>
    /// Gets or Sets ElementReference Property.
    /// </summary>
    public ElementReference ElementReference { get; set; }

    private IJsMudExComponent<MudExCardList<TData>> AsJsComponent => this;

    /// <summary>
    /// Gets or Sets MudExColor BackgroundColor Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public MudExColor BackgroundColor { get; set; } = Color.Default;

    /// <summary>
    /// Gets or Sets MudExColor HoverColor Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public MudExColor HoverColor { get; set; } = Color.Primary;

    /// <summary>
    /// Gets or Sets bool ZoomOnHover Property.
    /// </summary>
    [Obsolete("Use HoverMode instead")]
    [Parameter]
    [SafeCategory("Behavior")]
    public bool ZoomOnHover
    {
        get => HoverModeMatches(MudExCardHoverMode.Zoom);
        set
        {
            if (value && HoverMode.HasValue)
                HoverMode |= MudExCardHoverMode.Zoom;
            else if (!value && HoverMode.HasValue) HoverMode &= ~MudExCardHoverMode.Zoom;
        }
    }

    /// <summary>
    /// Gets or Sets MudExCardHoverMode HoverMode Property.
    /// </summary>
    /// <value>
    /// MudExCardHoverMode
    /// </value>
    [Parameter]
    [SafeCategory("Behavior")]
    public MudExCardHoverMode? HoverMode
    {
        get => _hoverMode;
        set
        {
            _hoverMode = value;
            _ = UpdateJs();
        }
    }

    /// <summary>
    /// Gets or Sets Justify Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public Justify Justify { get; set; } = Justify.Center;

    /// <summary>
    /// Gets or Sets Spacing Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public int Spacing { get; set; } = 15;

    /// <summary>
    /// Gets or Sets Light Bulb Size Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public int LightBulbSize { get; set; } = 30;

    /// <summary>
    /// Gets or Sets Light Bulb Size Unit Property.
    /// </summary>
    [Parameter]
    [SafeCategory("Appearance")]
    public CssUnit LightBulbSizeUnit { get; set; } = CssUnit.Percentage;


    private bool HoverModeMatches(MudExCardHoverMode mode) => HoverMode.HasValue && HoverMode.Value.HasFlag(mode);

    /// <summary>
    /// Methods returns List of MudExCardHoverMode, where hover modes are applied.
    /// </summary>
    public List<MudExCardHoverMode> AllAppliedHoverModes => Enum.GetValues(typeof(MudExCardHoverMode)).Cast<MudExCardHoverMode>().Where(HoverModeMatches).ToList();

    private string GetCss()
    {
        var res = CssBuilder.Default("mud-ex-card-list")
            .AddClass($"mud-ex-card-list-{_id}");

        foreach (var mode in AllAppliedHoverModes)
            res.AddClass($"mud-ex-card-list-{mode.ToString().ToLower()}");

        res.AddClass(Class);
        return res.Build();
    }

    /// <summary>
    /// Style for outer element.
    /// </summary>
    /// <returns></returns>
    public string GetStyle()
    {
        return MudExStyleBuilder.Default
            .With($"--mud-ex-card-bulb-size", $"{GetBulbSize()}{LightBulbSizeUnit.GetDescription()}")
            .With($"--mud-ex-card-list-bg-color", $"{BackgroundColor.ToCssStringValue()}")
            .With($"--mud-ex-card-list-hover-color", $"{HoverColor.ToCssStringValue()}")
            .With($"justify-content", Justify.GetDescription())
            .AddRaw(Style)
            .Build();
    }

    private int GetBulbSize() => LightBulbSizeUnit == CssUnit.Percentage ? Math.Min(Math.Max(LightBulbSize, 0), 100) : LightBulbSize;

    /// <summary>
    /// Method gets called OnParametersSetAsync.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await UpdateJs();
    }

    /// <summary>
    /// Method gets called UpdateJs for fetching JSRuntime.
    /// </summary>
    private async Task UpdateJs()
    {
        if (AsJsComponent.JsReference != null)
        {
            var options = Options();
            await AsJsComponent.JsReference.InvokeVoidAsync("initialize", options);
        }
    }

    /// <summary>
    /// Method gets called OnAfterRenderAsync for rendering and Initializing Modules.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await AsJsComponent.ImportModuleAndCreateJsAsync();
        await base.OnAfterRenderAsync(firstRender);
    }

    private object Options()
    {
        return new
        {
            Id = _id,
            Use3dEffect = HoverModeMatches(MudExCardHoverMode.CardEffect3d),
            UseZoomEffect = HoverModeMatches(MudExCardHoverMode.Zoom)
        };
    }

    /// <summary>
    /// Returns arguments for passed to JS code
    /// </summary>
    public virtual object[] GetJsArguments() => new[] { AsJsComponent.ElementReference, AsJsComponent.CreateDotNetObjectReference(), Options() };

    /// <summary>
    /// Method to dispose the module.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return AsJsComponent.DisposeModulesAsync();
    }
}