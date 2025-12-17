using E_Commerce.DTOs;
using E_Commerce.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace E_Commerce.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return View(registerDto);

            try
            {
                var token = await _authService.RegisterAsync(registerDto);

                // Store JWT in cookie
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // set true in production HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Parse JWT to get role
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                TempData["Success"] = "Registration successful!";

                // Redirect based on role
                if (roleClaim == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(registerDto);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Register(RegisterDto registerDto)
        //{
        //    if (!ModelState.IsValid)
        //        return View(registerDto);

        //    try
        //    {
        //        var token = await _authService.RegisterAsync(registerDto);
        //        Response.Cookies.Append("AuthToken", token, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            SameSite = SameSiteMode.Strict,
        //            Expires = DateTimeOffset.UtcNow.AddHours(1)
        //        });

        //        TempData["Success"] = "Registration successful!";
        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return View(registerDto);
        //    }
        //}

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(loginDto);

            try
            {
                var token = await _authService.LoginAsync(loginDto);

                // Store JWT in cookie
                Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // true in production HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Parse JWT to get role
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                TempData["Success"] = "Login successful!";

                // Redirect based on role
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (roleClaim == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(loginDto);
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> Login(LoginDto loginDto, string returnUrl = null)
        //{
        //    if (!ModelState.IsValid)
        //        return View(loginDto);

        //    try
        //    {
        //        var token = await _authService.LoginAsync(loginDto);
        //        Response.Cookies.Append("AuthToken", token, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            SameSite = SameSiteMode.Strict,
        //            Expires = DateTimeOffset.UtcNow.AddHours(1)
        //        });

        //        TempData["Success"] = "Login successful!";

        //        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        //            return Redirect(returnUrl);

        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", ex.Message);
        //        return View(loginDto);
        //    }
        //}

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            TempData["Success"] = "Logged out successfully";
            return RedirectToAction("Index", "Home");
        }
    }
}
