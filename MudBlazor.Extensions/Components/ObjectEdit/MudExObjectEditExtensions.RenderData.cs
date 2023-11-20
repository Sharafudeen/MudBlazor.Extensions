﻿using MudBlazor.Extensions.Components.ObjectEdit.Options;
using MudBlazor.Extensions.Helper.Internal;
using Nextended.Core.Extensions;

namespace MudBlazor.Extensions.Components.ObjectEdit;

public static partial class MudExObjectEditExtensions
{
    public static IRenderData WrapIn<TWrapperComponent>(this IRenderData renderData, params Action<TWrapperComponent>[] options) where TWrapperComponent : new()
    {
        if (renderData != null)
            renderData.Wrapper = RenderData.For(typeof(TWrapperComponent), PropertyHelper.ValuesDictionary(true, options));
        return renderData?.Wrapper;
    }

    public static IRenderData WrapIn(this IRenderData renderData, IRenderData wrappingRenderData)
    {
        if(renderData != null)
            renderData.Wrapper = wrappingRenderData;
        return wrappingRenderData;
    }

    /// <summary>
    /// Adds a component after the current component    
    /// </summary>
    // example call meta.Property(p => p.Brand).RenderData.AddComponentAfter(RenderData.For<MudTextField<BrandDto>>(f => {}));
    public static IRenderData AddComponentAfter<TComponent>(this IRenderData renderData, params Action<TComponent>[] options) where TComponent : new()
        => renderData.WithAdditionalComponent(RenderData.For(typeof(TComponent), PropertyHelper.ValuesDictionary(true, options)), true);
    public static IRenderData AddComponentBefore<TComponent>(this IRenderData renderData, params Action<TComponent>[] options) where TComponent : new()
        => renderData.WithAdditionalComponent(RenderData.For(typeof(TComponent), PropertyHelper.ValuesDictionary(true, options)), false);
    public static IRenderData AddComponentAfter(this IRenderData renderData, IRenderData afterRenderData) 
        => renderData.WithAdditionalComponent(afterRenderData, true);
    public static IRenderData AddComponentBefore(this IRenderData renderData, IRenderData afterRenderData)
        => renderData.WithAdditionalComponent(afterRenderData, false);

    public static IRenderData WithAdditionalComponent(this IRenderData renderData, IRenderData additionalRenderData, bool renderAfter)
    {
        if (renderData != null)
            (renderAfter ? renderData.RenderDataAfterComponent : renderData.RenderDataBeforeComponent).Add(additionalRenderData);
        return additionalRenderData;
    }

    public static IRenderData WrapInMudItem(this IRenderData renderData, params Action<MudItem>[] options)
        => renderData?.WrapIn(options);
    public static IRenderData WithoutValueBinding(this IRenderData renderData)
        => renderData?.SetProperties(r => r.DisableValueBinding = true);
}