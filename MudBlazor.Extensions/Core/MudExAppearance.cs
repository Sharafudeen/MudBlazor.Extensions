﻿using Microsoft.AspNetCore.Components;
using MudBlazor.Extensions.Attribute;
using MudBlazor.Extensions.Helper;
using Nextended.Core.Extensions;

namespace MudBlazor.Extensions.Core;

/// <summary>
/// The `MudExAppearance` class is a powerful tool that helps to manage CSS and styles of MudBlazor components dynamically. 
/// </summary>
[HasDocumentation("MudExAppearance.md")]
public class MudExAppearance : IMudExClassAppearance, IMudExStyleAppearance, ICloneable
{
    /// <summary>
    /// Class to apply
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// CSS Style string to apply
    /// </summary>
    public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Set to false to overwrite all existing classes and styles
    /// </summary>
    public bool KeepExisting { get; set; } = true;

    /// <summary>
    /// Factory method for an empty instance
    /// </summary>
    public static MudExAppearance Empty() => new();

    /// <summary>
    /// Factory method for an instance filled with some classes
    /// </summary>
    public static MudExAppearance FromCss(MudExCss.Classes cls, params MudExCss.Classes[] other) => FromCss(MudExCssBuilder.From(cls, other));

    /// <summary>
    /// Factory method for an instance filled with some classes from a MudExCssBuilder
    /// </summary>
    public static MudExAppearance FromCss(MudExCssBuilder builder) => Empty().WithCss(builder);

    /// <summary>
    /// Factory method for an instance filled with some styles
    /// </summary>
    public static MudExAppearance FromStyle(object styleObj) => FromStyle(MudExStyleBuilder.FromObject(styleObj));

    /// <summary>
    /// Factory method for an instance filled with some styles
    /// </summary>
    public static MudExAppearance FromStyle(string style) => FromStyle(MudExStyleBuilder.FromStyle(style));

    /// <summary>
    /// Factory method for an instance filled with some styles
    /// </summary>
    public static MudExAppearance FromStyle(MudExStyleBuilder styleBuilder) => Empty().WithStyle(styleBuilder);

    /// <summary>
    /// Factory method for an instance filled with some styles
    /// </summary>
    public static MudExAppearance FromStyle(Action<MudExStyleBuilder> styleBuilderAction) => Empty().WithStyle(styleBuilderAction);

    /// <summary>
    /// Adds style to this appearance from given appearance
    /// </summary>
    public MudExAppearance WithStyle(IMudExStyleAppearance style)
    {
        Style += $" {style.Style.EnsureEndsWith(";")}";
        return this;
    }

    /// <summary>
    /// Adds style to this appearance from given styleObj
    /// </summary>
    public MudExAppearance WithStyle(object styleObj, CssUnit cssUnit = CssUnit.Pixels) => WithStyle(MudExStyleBuilder.FromObject(styleObj, "", cssUnit));

    /// <summary>
    /// Adds style to this appearance from given styleObj
    /// </summary>
    public MudExAppearance WithStyle(object styleObj, string existingStyleToKeep, CssUnit cssUnit = CssUnit.Pixels) => WithStyle(MudExStyleBuilder.FromObject(styleObj, existingStyleToKeep, cssUnit));

    /// <summary>
    /// Adds style to this appearance from given styleString
    /// </summary>    
    public MudExAppearance WithStyle(string styleString) => WithStyle(MudExStyleBuilder.FromStyle(styleString));

    /// <summary>
    /// Adds style to this appearance with passing a fluent Action with a MudExStyleBuilder
    /// </summary>    
    public MudExAppearance WithStyle(Action<MudExStyleBuilder> styleAction)
    {
        var builder = new MudExStyleBuilder();
        styleAction(builder);
        return WithStyle(builder);
    }

    /// <summary>
    /// Adds style to this appearance with passing a async Func with a MudExStyleBuilder
    /// </summary>    
    public async Task<MudExAppearance> WithStyle(Func<MudExStyleBuilder, Task> styleAction)
    {
        var builder = new MudExStyleBuilder();
        await styleAction(builder);
        return WithStyle(builder);
    }


    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(string cls, params string[] other) => WithCss(MudExCssBuilder.From(cls, other));
    
    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(string cls, bool when) => WithCss(MudExCssBuilder.Default.AddClass(cls, when));

    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(MudExCss.Classes cls, bool when) => WithCss(MudExCssBuilder.Default.AddClass(cls, when));

    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(MudExCss.Classes cls, params MudExCss.Classes[] other) => WithCss(MudExCssBuilder.From(cls, other));

    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(IMudExClassAppearance css)
    {
        Class += $" {css.Class}";
        return this;
    }

    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public MudExAppearance WithCss(Action<MudExCssBuilder> cssAction)
    {
        var builder = new MudExCssBuilder();
        cssAction(builder);
        return WithCss(builder);
    }

    /// <summary>
    /// Adds class to this appearance
    /// </summary>    
    public async Task<MudExAppearance> WithCss(Func<MudExCssBuilder, Task> cssAction)
    {
        var builder = new MudExCssBuilder();
        await cssAction(builder);
        return WithCss(builder);
    }


    /// <summary>
    /// Clone this instance
    /// </summary>
    /// <returns></returns>
    public object Clone() => MemberwiseClone();
}
