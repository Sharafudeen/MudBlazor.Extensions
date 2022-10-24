﻿using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor.Extensions.Components.ObjectEdit.Options;
using MudBlazor.Extensions.Extensions;
using MudBlazor.Extensions.Helper;
using MudBlazor.Extensions.Options;
using MudBlazor.Utilities;
using Newtonsoft.Json;
using Nextended.Blazor.Models;
using Nextended.Core.Extensions;

namespace MudBlazor.Extensions.Components.ObjectEdit;

public partial class MudExObjectEdit<T>
{
    [Inject] private IServiceProvider _serviceProvider { get; set; }
    
    private IStringLocalizer<MudExObjectEdit<T>> _fallbackLocalizer => _serviceProvider.GetService<IStringLocalizer<MudExObjectEdit<T>>>();
    private IDialogService dialogService => _serviceProvider.GetService<IDialogService>();
    private IObjectMetaConfiguration<T> _configService => _serviceProvider.GetService<IObjectMetaConfiguration<T>>();
    private IJSRuntime _js => _serviceProvider.GetService<IJSRuntime>();
    private Color _importButtonColor;
    [Parameter] public IStringLocalizer Localizer { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    protected bool IsInternalLoading;

    [Parameter]
    public T Value
    {
        get => _value;
        set => SetValue(value);
    }
    //[Parameter] public bool Virtualize { get; set; } = true;
    [Parameter] public bool LightOverlayLoadingBackground { get; set; } = true;
    [Parameter] public Color ToolbarButtonColor { get; set; } = Color.Inherit;
    [Parameter] public bool AddScrollToTop { get; set; } = true;
    [Parameter] public bool AllowExport { get; set; }
    [Parameter] public bool AllowImport { get; set; }
    [Parameter] public bool AutoSaveRestoreState { get; set; }
    [Parameter] public StateTarget StateTargetStorage { get; set; } = StateTarget.SessionStorage;
    [Parameter] public string ExportFileName { get; set; }
    [Parameter] public string ImportIcon { get; set; } = Icons.Material.Outlined.FileOpen;
    [Parameter] public string SearchIcon { get; set; } = Icons.Material.Outlined.Search;
    [Parameter] public string ExpandCollapseIcon { get; set; } = Icons.Material.Filled.Expand;
    [Parameter] public string ExportIcon { get; set; } = Icons.Material.Filled.Save;
    [Parameter] public bool AutoSkeletonOnLoad { get; set; }
    [Parameter] public Color ToolbarColor { get; set; } = Color.Default;
    [Parameter] public Color GroupLineColor { get; set; } = Color.Secondary;
    [Parameter] public int? GroupElevation { get; set; }
    [Parameter] public int ToolBarElevation { get; set; }
    [Parameter] public string Class { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public string ToolBarClass { get; set; }
    [Parameter] public string ToolBarPaperClass { get; set; }
    [Parameter] public bool StickyToolbar { get; set; }
    [Parameter] public string StickyToolbarTop { get; set; } = "var(--mud-appbar-height)";
    [Parameter] public EventCallback<T> AfterImport { get; set; }
    [Parameter] public EventCallback<T> AfterExport { get; set; }
    [Parameter] public EventCallback<T> BeforeExport { get; set; }
    [Parameter] public EventCallback<T> ValueChanged { get; set; }
    [Parameter] public ObjectEditMeta<T> MetaInformation { get; set; }
    [Parameter] public bool ShowPathAsTitleForEachProperty { get; set; }
    [Parameter] public PathDisplayMode PathDisplayMode { get; set; }
    [Parameter] public GroupingStyle GroupingStyle { get; set; }
    [Parameter] public PropertyFilterMode FilterMode { get; set; } = PropertyFilterMode.Toggleable;
    [Parameter] public string Filter { get; set; }
    [Parameter] public bool AutoHideDisabledFields { get; set; }
    [Parameter] public string DefaultGroupName { get; set; }
    [Parameter] public bool DisableFieldFallback { get; set; }
    [Parameter] public bool? WrapInMudGrid { get; set; }
    [Parameter] public bool GroupsCollapsible { get; set; } = true;
    [Parameter] public GlobalResetSettings GlobalResetSettings { get; set; } = new();
    [Parameter] public PropertyResetSettings DefaultPropertyResetSettings { get; set; }
    [Parameter] public MessageBoxOptions ResetConfirmationMessageBoxOptions { get; set; }
    [Parameter] public DialogOptionsEx ResetConfirmationDialogOptions { get; set; }
    [Parameter] public Action<ObjectEditMeta<T>> MetaConfiguration { get; set; }
    [Parameter] public Task<Action<ObjectEditMeta<T>>> MetaConfigurationAsync { get; set; }
    [Parameter] public ActionAlignment ToolBarActionAlignment { get; set; } = ActionAlignment.Right;
    [Parameter] public RenderFragment ToolBarContent { get; set; }
    [Parameter] public RegisteredConfigurationBehaviour ConfigureBehaviourForRegisteredConfigurations { get; set; } = RegisteredConfigurationBehaviour.ExecutedBefore;
    /**
     * If this setting is true a manual passed MetaInformation will also re configured
     */
    [Parameter] public bool ConfigureMetaInformationAlways { get; set; }
    [Parameter] public string ErrorMessage { get; set; }

    private static Type[] handleAsPrimitive = { typeof(string), typeof(decimal), typeof(MudColor), typeof(System.Drawing.Color), typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(TimeOnly), typeof(DateOnly), typeof(Guid) };
    internal static bool IsPrimitive()
    {
        var type = typeof(T).IsNullable() ? Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T) : typeof(T);
        return type.IsPrimitive || type.IsEnum || handleAsPrimitive.Contains(type);
    }
    
    protected bool Primitive => IsPrimitive();

    protected virtual RenderFragment InternalToolBarContent => null;

    protected IStringLocalizer LocalizerToUse => Localizer ?? _fallbackLocalizer;
    private bool _searchActive;
    private MudTextField<string> _searchBox;
    private bool ShouldAddGrid(IEnumerable<ObjectEditPropertyMeta> meta) => WrapInMudGrid ?? ContainsMudItemInWrapper(meta);
    private string CssClassName => GroupingStyle == GroupingStyle.Flat ? $"mud-ex-object-edit-group-flat {(!GroupsCollapsible ? "mud-ex-hide-expand-btn" : "")}" : string.Empty;
    private IEnumerable<IGrouping<string, ObjectEditPropertyMeta>> GroupedMetaPropertyInfos()
        => MetaInformation?.AllProperties?.EmptyIfNull().Where(m => m.ShouldRender() && IsInFilter(m) && (!AutoHideDisabledFields || m.Settings.IsEditable)).GroupBy(m => m.GroupInfo?.Name);
    private bool ContainsMudItemInWrapper(IEnumerable<ObjectEditPropertyMeta> meta)
        => meta.Where(p => p.RenderData?.Wrapper != null)
            .Select(p => p.RenderData.Wrapper)
            .Recursive(w => w.Wrapper == null ? Enumerable.Empty<IRenderData>() : new[] { w.Wrapper })
            .Any(d => d.ComponentType == typeof(MudItem));

    private string stateKey => $"mud-ex-object-edit-{typeof(T).FullName}";
    protected override async Task OnParametersSetAsync()
    {
        _importButtonColor = ToolbarButtonColor;
        await base.OnParametersSetAsync();
        await CreateMetaIfNotExists();
        if (Value is IEditableObject editable)
            editable.BeginEdit();
        ResetConfirmationMessageBoxOptions ??= new MessageBoxOptions
        {
            Message = LocalizerToUse.TryLocalize("Reset all properties?"),
            Title = LocalizerToUse.TryLocalize("Reset all"),
            CancelText = LocalizerToUse.TryLocalize("Cancel"),
            YesText = LocalizerToUse.TryLocalize("Reset")
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && AutoSaveRestoreState)
            await RestoreState();
        await base.OnAfterRenderAsync(firstRender);
    }
    
    public T GetUpdatedValue()
    {
        foreach (var meta in MetaInformation.AllProperties.Where(p => p?.RenderData?.DisableValueBinding == true))
        {
            var editor = Editors.FirstOrDefault(edit => edit.PropertyMeta == meta);
            if (editor != null)
                meta.RenderData.SetValue(editor.GetCurrentValue());
        }
        return Value;
    }

    private async void SetValue(T value)
    {
        _value = value;
        MetaInformation?.SetValue(value);
        await CreateMetaIfNotExists();
        Invalidate();
    }

    public void Invalidate()
    {
        StateHasChanged();
        Editors.Apply(e => e.Invalidate());
    }

    private async Task CreateMetaIfNotExists()
    {
        RenderDataDefaults.AddRenderDataProvider(_serviceProvider);
        Action<ObjectEditMeta<T>> c = MetaConfigurationAsync != null ? await MetaConfigurationAsync : MetaConfiguration;
        bool metaNeedsConfig = MetaInformation == null; // If not configured or not manually bypassed wer configure the Meta
        MetaInformation ??= (Value ??= typeof(T).CreateInstance<T>()).ObjectEditMeta();
        if (metaNeedsConfig || ConfigureMetaInformationAlways)
        {
            if (_configService != null && ConfigureBehaviourForRegisteredConfigurations == RegisteredConfigurationBehaviour.ExecutedBefore)
                await _configService.ConfigureAsync(MetaInformation);
            c?.Invoke(MetaInformation);
            if (_configService != null && ConfigureBehaviourForRegisteredConfigurations == RegisteredConfigurationBehaviour.ExecutedAfter)
                await _configService.ConfigureAsync(MetaInformation);
        }
    }

    protected virtual Task OnPropertyChange(ObjectEditPropertyMeta property)
    {
        //return Task.WhenAll(Task.Run(() =>
        //{
        //    if (Value != null)
        //        MetaInformation.AllProperties.Apply(p => p?.UpdateConditionalSettings(Value));
        //}), ValueChanged.InvokeAsync(Value));
        if(AutoSaveRestoreState)
            _= Task.Run(SaveState);

        if (Value != null)
            MetaInformation.AllProperties.Apply(p => p?.UpdateConditionalSettings(Value));
        return ValueChanged.InvokeAsync(Value);
    }

    private bool IsInFilter(ObjectEditPropertyMeta propertyMeta)
        => string.IsNullOrWhiteSpace(Filter)
           || propertyMeta.Settings.LabelFor(LocalizerToUse).Contains(Filter, StringComparison.InvariantCultureIgnoreCase)
           || propertyMeta.Settings.DescriptionFor(LocalizerToUse).Contains(Filter, StringComparison.InvariantCultureIgnoreCase)
           || propertyMeta.PropertyInfo.Name.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)
           || propertyMeta.Value?.ToString()?.Contains(Filter, StringComparison.InvariantCultureIgnoreCase) == true
           || propertyMeta.GroupInfo?.Name?.Contains(Filter, StringComparison.InvariantCultureIgnoreCase) == true
           || propertyMeta.RenderData?.Attributes.Values.OfType<string>().Any(x => x.Contains(Filter, StringComparison.InvariantCultureIgnoreCase)) == true;

    private Task FilterKeyPress(KeyboardEventArgs arg)
    {
        if (arg.Key == "Escape")
        {
            if (!string.IsNullOrWhiteSpace(Filter))
                Filter = string.Empty;
            else
                _searchActive = false;
        }
        return Task.CompletedTask;
    }

    private bool _searchBoxBlur = false;
    private Task FilterBoxBlur(FocusEventArgs arg)
    {
        _searchBoxBlur = true;
        _searchActive = false;
        Task.Delay(300).ContinueWith(t => _searchBoxBlur = false);
        return Task.CompletedTask;
    }

    public List<MudExPropertyEdit> Editors = new();
    private T _value;
    public MudExPropertyEdit Ref { set => Editors.Add(value); }

    private List<MudExpansionPanel> _groups = new();
    public MudExpansionPanel ExpansionPanel { set => _groups.Add(value); }

    private async Task OnResetClick(MouseEventArgs arg)
    {
        if (GlobalResetSettings.RequiresConfirmation && dialogService != null && !(await ShowConfirmationBox() ?? false))
            return;
        await Reset();
        StateHasChanged();
    }

    public async Task Reset()
    {
        await Task.WhenAll(Editors.Select(e => e.ResetAsync()));
        if (Value is IEditableObject editable)
            editable.CancelEdit();
    }

    public Task Clear() => Task.WhenAll(Editors.Select(e => e.ClearAsync()));

    public virtual async Task RestoreState()
    {
        string json = await _js.InvokeAsync<string>($"{StateTargetStorage.ToDescriptionString()}.getItem", stateKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            LoadFromJson(json);
            StateHasChanged();
        }
    }

    public virtual async Task SaveState()
    {
        string json = JsonConvert.SerializeObject(Value);
        await _js.InvokeVoidAsync($"{StateTargetStorage.ToDescriptionString()}.setItem", stateKey, json);
    }

    private Task<bool?> ShowConfirmationBox()
    {
        ResetConfirmationDialogOptions ??= new DialogOptionsEx
        {
            ShowAtCursor = true,
            CursorPositionOrigin = ToolBarActionAlignment == ActionAlignment.Right ? Origin.CenterRight : Origin.CenterLeft,
            Animations = new[] { AnimationType.Pulse },
            DragMode = MudDialogDragMode.Simple,
            CloseButton = false
        };
        return _js != null
            ? dialogService.ShowMessageBoxEx(ResetConfirmationMessageBoxOptions, ResetConfirmationDialogOptions)
            : dialogService.ShowMessageBox(ResetConfirmationMessageBoxOptions, ResetConfirmationDialogOptions);
    }

    private void ToggleSearchBox()
    {
        if (_searchBoxBlur)
            return;
        _searchActive = !_searchActive;
        _searchBox.FocusAsync();
    }

    private string ToolbarStyle()
    {
        var res = string.Empty;
        if (StickyToolbar && !string.IsNullOrWhiteSpace(StickyToolbarTop))
            res += $"top: {StickyToolbarTop};";
        if (ToolbarColor != Color.Inherit)
            res += $"background-color: {ToolbarColor.CssVarDeclaration()};";
        return res;
    }

    private Task ExpandCollapse()
    {
        var collapse = _groups[0].IsExpanded;
        _groups.ForEach(g =>
        {
            if (collapse)
                g.Collapse();
            else
                g.Expand();
        });
        return Task.CompletedTask;
    }

    private IDictionary<string, object> GetAttributesForPrimitive()
    {
        string[] ignore = { nameof(Value), nameof(Localizer), nameof(ValueChanged), nameof(MetaConfiguration), nameof(MetaConfigurationAsync), nameof(MetaInformation) };
        var props = typeof(MudExObjectEdit<ModelForPrimitive<T>>).GetProperties().Select(p => p.Name); // Only allow props on this type and not on derived types
        var res = (from prop in GetType().GetProperties().Where(p => props.Contains(p.Name)) 
            let attr = prop.GetCustomAttribute<ParameterAttribute>() 
            where attr != null && !ignore.Contains(prop.Name) && prop.CanWrite 
            select prop).ToDictionary(prop => prop.Name, prop => prop.GetValue(this));
        var primitiveWrapper = new ModelForPrimitive<T>(Value);
        res.AddOrUpdate(nameof(Primitive), false);
        res.AddOrUpdate(nameof(Value), primitiveWrapper);
        res.AddOrUpdate(nameof(ValueChanged), RuntimeHelpers.TypeCheck(
            EventCallback.Factory.Create(
                this,
                EventCallback.Factory.CreateInferred(
                    this, x =>
                    {
                        Value = x.Value;
                        ValueChanged.InvokeAsync(Value);
                    },
                    primitiveWrapper
                )
            )
        ));

        return res;
    }

    private async Task Export()
    {
        await BeforeExport.InvokeAsync(Value);
        IsInternalLoading = true;
        try
        {
            string json = JsonConvert.SerializeObject(Value, Formatting.Indented);
            var url = await DataUrl.GetDataUrlAsync(await Task.Run(() => Encoding.UTF8.GetBytes(json)), "application/json");
            await _js.InvokeVoidAsync("MudBlazorExtensions.downloadFile", new
            {
                Url = url,
                FileName = !string.IsNullOrWhiteSpace(ExportFileName) ? ExportFileName : $"{Value.GetType().Name}_{DateTime.Now}.json",
                MimeType = "application/json"
            });
            await AfterExport.InvokeAsync(Value);
        }
        finally
        {
            IsInternalLoading = false;
        }
    }

    private async Task Import(InputFileChangeEventArgs e)
    {
        if (e.File.ContentType != "application/json")
        {
            ErrorMessage = $"Invalid file type: {e.File.ContentType} only JSON files are supported";
            return;
        }
        try
        {
            var buffer = new byte[e.File.Size];
            await e.File.OpenReadStream(e.File.Size).ReadAsync(buffer);
            var json = Encoding.UTF8.GetString(buffer);
            LoadFromJson(json);
            await ImportSuccessUI();
            await AfterImport.InvokeAsync(Value);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsInternalLoading = false;
        }
    }

    private async Task ImportSuccessUI()
    {
        var icon = ImportIcon;
        _importButtonColor = Color.Success;
        ImportIcon = Icons.Material.Filled.Check;
        StateHasChanged();
        await Task.Delay(2500);
        _importButtonColor = ToolbarButtonColor;
        ImportIcon = icon;
    }

    private void LoadFromJson(string json)
    {
        var obj = JsonConvert.DeserializeObject(json, Value.GetType());
        Value = (T) obj;
    }
}