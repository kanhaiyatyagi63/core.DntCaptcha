# DNTCaptcha.Core

<p align="left">
  <a href="https://github.com/VahidN/DNTCaptcha.Core">
     <img alt="GitHub Actions status" src="https://github.com/VahidN/DNTCaptcha.Core/workflows/.NET%20Core%20Build/badge.svg">
  </a>
</p>

`DNTCaptcha.Core` is a captcha generator and validator for ASP.NET Core applications.

## Install via NuGet

To install DNTCaptcha.Core, run the following command in the Package Manager Console:

```
PM> Install-Package DNTCaptcha.Core
```

You can also view the [package page](http://www.nuget.org/packages/DNTCaptcha.Core/) on NuGet.

## Usage:

- After installing the DNTCaptcha.Core package, add the following definition to the [\_ViewImports.cshtml](/src/DNTCaptcha.TestWebApp/Views/_ViewImports.cshtml) file:

```csharp
@addTagHelper *, DNTCaptcha.Core
```

- Then to use it, add its new tag-helper to [your view](/src/DNTCaptcha.TestWebApp/Views/Home/_LoginFormBody.cshtml):
For bootstrap-5 (you will need Bootstrap Icons for the missing [font-glyphs](https://icons.getbootstrap.com/) too):

```xml
 <dnt-captcha asp-captcha-generator-max="999999"
                             asp-captcha-generator-min="111111"
                             asp-captcha-generator-language="English"
                             asp-captcha-generator-display-mode="ShowDigits"
                             asp-use-relative-urls="true"
                             asp-placeholder="Enter Captcha"
                             asp-validation-error-message="Please enter the security code."
                             asp-font-name="Tahoma"
                             asp-font-size="20"
                             asp-fore-color="#333333"
                             asp-back-color="#ccc"
                             asp-text-box-class="text-box form-control"
                             asp-text-box-template="<span class='input-group-prepend'><span class='form-group-text'></span></span>{0}"
                             asp-validation-message-class="text-danger"
                             asp-refresh-button-class="fas fa-redo btn-sm"
                             asp-use-noise="false" />
```

- To register its default providers, call `services.AddDNTCaptcha();` method in your [Startup class](/src/DNTCaptcha.TestWebApp/Startup.cs).

```csharp
using DNTCaptcha.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add Dnt captcha
IWebHostEnvironment _env = builder.Environment;

builder.Services.AddDNTCaptcha(options =>
{
    options.UseCookieStorageProvider(SameSiteMode.Strict)
    .AbsoluteExpiration(minutes: 7)
    .ShowThousandsSeparators(false)
    .WithNoise(pixelsDensity: 25, linesCount: 3)
    .WithEncryptionKey("This is my secure key!")
    .InputNames(// This is optional. Change it if you don't like the default names.
        new DNTCaptchaComponent
        {
            CaptchaHiddenInputName = "DNT_CaptchaText",
            CaptchaHiddenTokenName = "DNT_CaptchaToken",
            CaptchaInputName = "DNT_CaptchaInputText"
        })
    .Identifier("dnt_Captcha")// This is optional. Change it if you don't like its default name.
    ;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

```

- Now you can add the `ValidateDNTCaptcha` attribute [to your action method](/src/DNTCaptcha.TestWebApp/Controllers/HomeController.cs) to verify the entered security code:

```csharp
[HttpPost, ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(
            ErrorMessage = "Please Enter Valid Captcha",
            CaptchaGeneratorLanguage = Language.English,
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                return View();
            }
            ViewBag.message = "Login Successfull!";
            return View();
        }
```

Or you can use the `IDNTCaptchaValidatorService` directly:

```csharp
using core.captcha.Models;
using DNTCaptcha.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace core.captcha.Controllers
{
    public class DntCaptchaController : Controller
    {
        private readonly IDNTCaptchaValidatorService _validatorService;
        private readonly IOptions<DNTCaptchaOptions> _captchaOptions;

        public DntCaptchaController(IDNTCaptchaValidatorService validatorService,
            IOptions<DNTCaptchaOptions> captchaOptions)
        {
            _validatorService = validatorService;
            _captchaOptions = captchaOptions;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

         [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
            {
                this.ModelState.AddModelError(_captchaOptions.Value.CaptchaComponent.CaptchaInputName, "Please Enter Valid Captcha.");
                return View();
            }
            ViewBag.message = "Login Successfull!";
            return View();
        }
    }
}

```
