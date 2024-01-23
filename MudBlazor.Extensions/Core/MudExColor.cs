﻿using Microsoft.JSInterop;
using MudBlazor.Extensions.Attribute;
using MudBlazor.Extensions.Helper;
using MudBlazor.Utilities;
using Newtonsoft.Json;
using OneOf;
using DC = System.Drawing.Color;

namespace MudBlazor.Extensions.Core;

/// <summary>
/// MudExColor is a readonly struct that provides a convenient and type-safe way to deal with color values in the context of MudBlazor.Extensions components.
/// </summary>
[HasDocumentation("MudExColor.md")]
public readonly struct MudExColor
{
    private readonly OneOf<Color, MudColor, DC, string, uint> _value;

    /// <summary>
    /// MudExColor constructor, builds the color from various potential inputs.
    /// </summary>
    public MudExColor(OneOf<Color, MudColor, DC, string, uint> value)
    {
        if (value.IsT3 && value.AsT3.Equals("transparent", StringComparison.InvariantCultureIgnoreCase))
            value = Transparent._value;
        _value = value.IsT3 switch
        {
            true when Enum.TryParse(value.AsT3, out Color color) => color,
            true when MudExColorUtils.TryParseFromHtmlColorName(value.AsT3, out var dc) => dc.ToMudColor(),
            _ => value
        };

        try
        {
            var str = ToString(value);
            SuggestedFormat = GetSuggestedFormat(str);
        }
        catch
        {
            SuggestedFormat = MudColorOutputFormats.Hex;
        }
    }

    /// <summary>
    /// The suggested output format for the color.
    /// </summary>
    public MudColorOutputFormats SuggestedFormat { get; }

    /// <summary>
    /// The color value.
    /// </summary>
    public object Value => _value.Value;

    /// <summary>
    /// Determines if the value is of type Color.
    /// </summary>
    public bool IsColor => _value.IsT0;

    /// <summary>
    /// Determines if the value is of type MudColor.
    /// </summary>
    public bool IsMudColor => _value.IsT1;
    
    /// <summary>
    /// Determines if the value is of type DrawingColor.
    /// </summary>
    public bool IsDrawingColor => _value.IsT2;

    /// <summary>
    /// Determines if the value is of type String.
    /// </summary>
    public bool IsString => _value.IsT3;

    /// <summary>
    /// Determines if the value is of type Integer.
    /// </summary>
    public bool IsInt => _value.IsT4;

    /// <summary>
    /// Returns the value as Color type.
    /// </summary>
    [JsonIgnore]
    public Color AsColor => _value.AsT0;

    /// <summary>
    /// Returns the value as MudColor type.
    /// </summary>
    [JsonIgnore]
    public MudColor AsMudColor => _value.AsT1;

    /// <summary>
    /// Returns the value as DrawingColor type.
    /// </summary>
    [JsonIgnore]
    public DC AsDrawingColor => _value.AsT2;

    /// <summary>
    /// Returns the value as String type.
    /// </summary>
    [JsonIgnore]
    public string AsString => _value.AsT3;

    /// <summary>
    /// Returns the value as Integer type.
    /// </summary>
    [JsonIgnore]
    public uint AsInt => _value.AsT4;

    /// <summary>
    /// Executes a function depending on the type of the value.
    /// </summary>
    public TResult Match<TResult>(Func<Color, TResult> f0, Func<MudColor, TResult> f1, Func<DC, TResult> f2, Func<string, TResult> f3, Func<uint, TResult> f4) => _value.Match(f0, f1, f2, f3, f4);

    /// <summary>
    /// Executes an action depending on the type of the value.
    /// </summary>
    public void Switch(Action<Color> f0, Action<MudColor> f1, Action<DC> f2, Action<string> f3, Action<uint> f4) => _value.Switch(f0, f1, f2, f3, f4);


    // Implicit conversions
    /// <summary>
    /// Implicit conversion from Color type to MudExColor.
    /// </summary>
    public static implicit operator MudExColor(Color c) => new(c);

    /// <summary>
    /// Implicit conversion from MudColor type to MudExColor.
    /// </summary>
    public static implicit operator MudExColor(MudColor c) => c is null ? Default : new MudExColor(c);

    /// <summary>
    /// Implicit conversion from DC type to MudExColor.
    /// </summary>
    public static implicit operator MudExColor(DC s) => new(s);

    /// <summary>
    /// Implicit conversion from String type to MudExColor.
    /// </summary>
    public static implicit operator MudExColor(string s) => string.IsNullOrEmpty(s) ? Default : new MudExColor(s);

    /// <summary>
    /// Implicit conversion from Integer type to MudExColor.
    /// </summary>
    public static implicit operator MudExColor(uint i) => new(i);

    // Constants representing common colors
    public static MudExColor Transparent => new(Color.Transparent);
    public static MudExColor Default => Transparent;
    public static MudExColor Info => new(Color.Info);
    public static MudExColor Warning => new(Color.Warning);
    public static MudExColor Error => new(Color.Error);
    public static MudExColor Success => new(Color.Success);
    public static MudExColor Inherit => new(Color.Inherit);
    public static MudExColor Dark => new(Color.Dark);
    public static MudExColor Primary => new(Color.Primary);
    public static MudExColor Secondary => new(Color.Secondary);
    public static MudExColor Tertiary => new(Color.Tertiary);
    public static MudExColor Surface => new(Color.Surface);
    public static MudExColor AppBarBackground => new("--mud-palette-appbar-background");
    public static MudExColor DrawerBackground => new("--mud-palette-drawer-background");
    public static MudExColor TextPrimary => new("--mud-palette-primary-text");
    public static MudExColor TextSecondary => new("--mud-palette-secondary-text");
    public static MudExColor TextDrawer => new("--mud-palette-drawer-text");
    public static MudExColor TextDisabled => new("--mud-palette-text-disabled");
    public static MudExColor TextActionDisabled => new("--mud-palette-action-disabled");
    public static MudExColor ActionDisabledBackground => new("--mud-palette-action-disabled-background");

    /// <summary>
    /// Determines if the value equals the provided Color.
    /// </summary>
    public bool Is(Color c) => _value.Value.Equals(c);

    /// <summary>
    /// Determines if the value equals the provided String.
    /// </summary>
    public bool Is(string c) => _value.Value.Equals(c);

    /// <summary>
    /// Determines if the value equals the provided MudColor.
    /// </summary>
    public bool Is(MudColor c) => _value.Value.Equals(c);

    /// <summary>
    /// Determines if the value equals the provided Integer.
    /// </summary>
    public bool Is(uint c) => _value.Value.Equals(c);

    /// <summary>
    /// Determines if the value equals the provided MudColor.
    /// </summary>
    public bool Is(DC c) => _value.Value.Equals(c);

    /// <summary>
    /// Converts this MudExColor object to its string representation.
    /// </summary>
    /// <returns>A string that represents this MudExColor object.</returns>
    public override string ToString() => ToString(_value);

    /// <summary>
    /// Converts this MudExColor object to a CSS string.
    /// </summary>
    /// <returns>A CSS string that represents this MudExColor object.</returns>
    public string ToCssStringValue() => ToCssStringValue(SuggestedFormat);

    /// <summary>
    /// Creates a css compatible string representation of the color.
    /// </summary>
    public string ToCssStringValue(MudColorOutputFormats format)
        => Match(
            color => color.CssVarDeclaration(),
            mudColor => mudColor.ToString(format),
            dc => dc.ToMudColor().ToString(format),
            s =>
            {
                if (s.StartsWith("--"))
                    s = $"var({s})";
                return s.ToLower().StartsWith("var") ? s : new MudColor(s).ToString(format);
            },
            i => FromUInt(i).ToString(format)
        );

    /// <summary>
    /// Creates a MudColor independent of what the underlying type is.
    /// </summary>
    public Task<MudColor> ToMudColorAsync(IJSRuntime jSRuntime)
        => Match(
            color => color.ToMudColorAsync(jSRuntime),
            Task.FromResult,
            dc => Task.FromResult(dc.ToMudColor()),
            s => Task.FromResult(new MudColor(s)),
            i => Task.FromResult(FromUInt(i))
        );

    /// <summary>
    /// Creates a MudColor independent of what the underlying type is.
    /// </summary>
    public MudColor ToMudColor()
        => Match(
            color => default,
            mc => mc,
            dc => dc.ToMudColor(),
            s => new MudColor(s),
            i => FromUInt(i)
        );

    /// <summary>
    /// Static helper method to list colors from current Theme
    /// </summary>
    public static async Task<IEnumerable<(string Name, MudColor Color)>> GetColorsFromThemeAsync(IJSRuntime jSRuntime, int count = 10)
    {
        var themeColors = await jSRuntime.GetCssColorVariablesAsync();
        return themeColors
            //.Where(c => !c.Key.Contains("background", StringComparison.InvariantCultureIgnoreCase) && !c.Key.Contains("surface", StringComparison.InvariantCultureIgnoreCase) && !c.Value.IsBlack() && !c.Value.IsWhite() && c.Value.APercentage >= 1.0)
            .Select(x => (x.Key, x.Value))
            .Distinct()
            .Take(count);
    }


    internal static MudColor FromInt(int value)
    {
        return new MudColor((byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF), (byte)((value >> 24) & 0xFF));
    }

    internal static MudColor FromUInt(uint value)
    {
        return new MudColor((byte)((value >> 16) & 0xFF),
            (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF), (byte)((value >> 24) & 0xFF));
    }

    private static string ToString(OneOf<Color, MudColor, DC, string, uint> oneOf)
        => oneOf.Match(
            color => color.ToString(),
            mudColor => mudColor.ToString(),
            dc => dc.ToString(),
            s => s,
            i => i.ToString()
        );

    /// <summary>
    /// returns the suggested format for a color string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static MudColorOutputFormats GetSuggestedFormat(string str)
    {
        if (str.ToLower().StartsWith("rgba"))
            return MudColorOutputFormats.RGBA;
        if (str.ToLower().StartsWith("rgb"))
            return MudColorOutputFormats.RGB;
        if (str.ToLower().StartsWith("#") && str.Length == 9)
            return MudColorOutputFormats.HexA;
        return MudColorOutputFormats.Hex;
    }
}
