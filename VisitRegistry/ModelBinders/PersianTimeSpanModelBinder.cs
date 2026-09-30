using Microsoft.AspNetCore.Mvc.ModelBinding;
using VisitRegistry.Services;

namespace VisitRegistry.ModelBinders;

/// <summary>
/// مدل‌بینر ساعت که مقادیر ۲۴ ساعته را با ارقام فارسی یا لاتین می‌پذیرد.
/// Accepts HH:mm (and HH:mm:ss) written with Latin, Persian or Arabic digits.
/// </summary>
public class PersianTimeSpanModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (result == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

        var raw = result.FirstValue;
        if (string.IsNullOrWhiteSpace(raw))
        {
            // ورودی خالی معادل مقدار پیش‌فرض مدل است (مثلاً ساعت فعلی).
            return Task.CompletedTask;
        }

        if (PersianDateService.TryParseTime(raw, out var time))
        {
            bindingContext.Result = ModelBindingResult.Success(time);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            "ساعت را به‌صورت ۲۴ ساعته و مانند ۰۸:۳۰ وارد کنید.");

        return Task.CompletedTask;
    }
}
