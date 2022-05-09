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
        [ValidateDNTCaptcha(
            ErrorMessage = "Please Enter Valid Captcha",
            CaptchaGeneratorLanguage = Language.English,
            CaptchaGeneratorDisplayMode = DisplayMode.ShowDigits)]
        public IActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (!_validatorService.HasRequestValidCaptchaEntry(Language.English, DisplayMode.ShowDigits))
                {
                    this.ModelState.AddModelError(_captchaOptions.Value.CaptchaComponent.CaptchaInputName, "Please Enter Valid Captcha.");
                    return View("Login");
                }
            }
            ViewBag.message = "Login Successfull!";
            return View();
        }
    }
}
