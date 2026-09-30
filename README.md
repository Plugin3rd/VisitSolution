# سامانه ثبت مراجعات

سامانه وب کنترل تردد مراجعه‌کنندگان با C#، ASP.NET Core MVC و SQL Server.

## فناوری‌ها
- .NET 10
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- فونت فارسی Vazirmatn (به‌صورت محلی در `wwwroot/fonts`)
- بومی‌سازی استاندارد `fa-IR` با تقویم هجری شمسی
- رابط کاربری کاملاً راست‌چین و ریسپانسیو

## اجرا
1. در `appsettings.json` مقدار `ConnectionStrings:DefaultConnection` را متناسب با SQL Server تغییر دهید.
2. SDK دات‌نت 10 را نصب کنید.
3. در پوشه پروژه اجرا کنید:

```bash
dotnet restore
dotnet run
```

در اجرای اول، دیتابیس با `EnsureCreated` ساخته می‌شود و نقش‌ها و یک کاربر مدیر ایجاد می‌گردند.

## کاربر اولیه
نام کاربری و رمز عبور از بخش `SeedAdmin` در `appsettings.json` خوانده می‌شود.
قبل از استقرار، حتماً رمز پیش‌فرض را تغییر دهید.

## قابلیت‌های کاربردی
- ورود و خروج کاربران سامانه
- نقش‌های مدیر سامانه، اپراتور، مشاهده‌گر
- مدیریت اشخاص و واحدهای سازمانی
- ثبت ورود و خروج با انتخابگر ساعت
- جلوگیری از ثبت ورود شخص غیرفعال
- جلوگیری از ورود شخص دارای بلک‌لیست فعال
- جلوگیری از داشتن دو مراجعه باز همزمان
- ثبت Audit و کنترل همزمانی با `RecordVersion`

## رابط کاربری

### فارسی‌سازی و RTL
- فرهنگ پیش‌فرض `fa-IR` با تقویم هجری شمسی (`UseRequestLocalization`)
- نمایش همه تاریخ‌ها و اعداد با ارقام فارسی (`۱۴۰۵/۰۷/۰۶` و `۰۸:۳۰`)
- چیدمان راست‌چین با خواص منطقی CSS (`inset-inline`، `margin-inline`)
- تاریخ بلند فارسی (مثلاً «دوشنبه ۷ مهر ۱۴۰۵») در سربرگ و داشبورد

### انتخابگرها
- **انتخابگر تاریخ شمسی**: `persian-datepicker` (سازگار با API بوت‌استرپ) در `Views/BlackList/Create|Edit`
- **انتخابگر ساعت**: `bootstrap-timepicker` در `Views/Visits/Create|Exit`
- هر دو به‌صورت محلی در `wwwroot/lib` نگهداری می‌شوند و هیچ درخواستی به بیرون ارسال نمی‌شود
- انتخابگرها با ویژگی‌های `data-jalali-picker` و `data-time-picker` فعال می‌شوند
- مدل‌بیندر اختصاصی `PersianTimeSpanModelBinder` ساعت را با ارقام فارسی، عربی یا لاتین می‌پذیرد

### امکانات دیگر
- پوسته روشن و تاریک با ذخیره در `localStorage` و احترام به تنظیمات سیستم
- منوی کناری off-canvas در موبایل با لایه تیره و بستن با کلید Escape
- منوی کشویی کاربر، آواتار حروف اول نام و نقش نمایش‌داده‌شده
- موتور جدول: مرتب‌سازی ستون‌ها، فیلتر سریع، صفحه‌بندی، انتخاب تعداد در صفحه
- نشان‌های وضعیت رنگی (فعال/غیرفعال، حاضر/خارج‌شده، مؤثر/دائمی)
- حالت «موردی برای نمایش نیست» برای جدول‌های خالی
- دیالوگ تأیید فارسی جایگزین `confirm` مرورگر (بدون مسدود شدن صفحه)
- پیام شناور (Toast) برای پیام‌های موفقیت و خطا با امکان بستن دستی
- نشانگر بارگذاری هنگام ارسال فرم
- جستجوی شخص با debounce و ناوبری با صفحه‌کلید (`js/person-picker.js`)
- فرم‌های دوستونی در دسکتاپ و تک‌ستونی در موبایل
- جدول‌ها به کارت تبدیل می‌شوند در نمایشگرهای کوچک
- استایل چاپ اختصاصی (دکمه «چاپ» در پاورقی جدول)
- دسترس‌پذیری: برچسب‌های ARIA، پیمایش با صفحه‌کلید، احترام به `prefers-reduced-motion`

## ساختار دارایی‌های سمت کاربر

| مسیر | توضیح |
| --- | --- |
| `wwwroot/css/site.css` | پوسته اصلی، متغیرهای طراحی، حالت تاریک، ریسپانسیو |
| `wwwroot/css/pickers.css` | هماهنگ‌سازی ظاهری انتخابگر تاریخ و ساعت |
| `wwwroot/js/site.js` | پوسته، ناوبری، جدول، پیام شناور، دیالوگ، انتخابگرها |
| `wwwroot/js/person-picker.js` | جستجو و انتخاب شخص |
| `wwwroot/fonts/` | فونت Vazirmatn (شش وزن) |
| `wwwroot/lib/` | jQuery، persian-datepicker، bootstrap-timepicker |
| `Views/Shared/_Icons.cshtml` | مجموعه آیکون SVG داخلی |
| `Views/Shared/_SearchBar.cshtml` | نوار جستجوی استاندارد |
| `Views/Shared/_TablePager.cshtml` | پاورقی جدول (شمارش، صفحه‌بندی، چاپ) |
| `Views/Shared/_EmptyState.cshtml` | حالت خالی جدول |

## افزودن انتخابگر تاریخ یا ساعت به فیلد جدید

```html
<!-- انتخابگر تاریخ شمسی -->
<div class="field-with-btn">
    <input asp-for="StartDate" data-jalali-picker placeholder="۱۴۰۵/۰۷/۰۶" autocomplete="off" />
    <button type="button" class="field-btn" aria-label="انتخاب تاریخ">
        <svg aria-hidden="true"><use href="#i-calendar"></use></svg>
    </button>
</div>

<!-- انتخابگر ساعت -->
<div class="field-with-btn">
    <input asp-for="EntryTime" type="text" class="time-input" data-time-picker autocomplete="off" />
    <button type="button" class="field-btn" aria-label="انتخاب ساعت">
        <svg aria-hidden="true"><use href="#i-clock"></use></svg>
    </button>
</div>
```

## مرحله بعد
پیشنهاد می‌شود در مرحله بعد Migrationهای EF Core، گزارش‌ها و خروجی Excel/PDF و آرشیو سوابق اضافه شوند.
