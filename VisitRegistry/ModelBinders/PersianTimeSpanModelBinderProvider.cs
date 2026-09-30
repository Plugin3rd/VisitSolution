using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VisitRegistry.ModelBinders;

/// <summary>
/// ثبت <see cref="PersianTimeSpanModelBinder"/> به‌عنوان بایندر پیش‌فرض برای TimeSpan.
/// </summary>
public class PersianTimeSpanModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.UnderlyingOrModelType == typeof(TimeSpan)
            ? new PersianTimeSpanModelBinder()
            : null;
    }
}
