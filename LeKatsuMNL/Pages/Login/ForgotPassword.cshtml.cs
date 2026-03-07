using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LeKatsuMNL.Data;
using LeKatsuMNL.Models;

namespace LeKatsuMNL.Pages.Login
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly LeKatsuDb _context;

        public ForgotPasswordModel(LeKatsuDb context)
        {
            _context = context;
        }

        // 1 = Enter Email, 2 = Verify OTP, 3 = Reset Password
        [BindProperty]
        public int CurrentPhase { get; set; } = 1;

        [BindProperty]
        public string RecoveryEmail { get; set; }

        [BindProperty]
        public string UserOtp { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }
        public string GeneratedOtp { get; set; }
        public bool TriggerEmailJsDispatch { get; set; } = false;

        [BindProperty]
        public string VerifiedOtpCode { get; set; }

        public void OnGet()
        {
            CurrentPhase = 1;
        }

        public async Task<IActionResult> OnPostRequestOtpAsync()
        {
            CurrentPhase = 1; // Default
            if (string.IsNullOrWhiteSpace(RecoveryEmail))
            {
                ErrorMessage = "Please enter an email address.";
                return Page();
            }

            // Check if email exists in Admin Account
            var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Email == RecoveryEmail);
            var manager = await _context.BranchManagers.FirstOrDefaultAsync(m => m.Email == RecoveryEmail);
            var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.Email == RecoveryEmail);

            if (admin == null && manager == null && staff == null)
            {
                // To prevent email enumeration attacks, you can generalize this message,
                // but for debugging purposes during development, we'll be direct.
                ErrorMessage = "Email not found in our records.";
                return Page();
            }

            // Generate 6-digit OTP
            Random rnd = new Random();
            string otp = rnd.Next(100000, 999999).ToString();

            // Store OTP in session (Requires AddSession and UseSession in Program.cs)
            HttpContext.Session.SetString("PasswordRecoveryOtp", otp);
            HttpContext.Session.SetString("PasswordRecoveryEmail", RecoveryEmail);

            GeneratedOtp = otp; // Pass to frontend for EmailJS to pick up
            TriggerEmailJsDispatch = true;
            CurrentPhase = 2; // Move to OTP entry

            return Page();
        }

        public IActionResult OnPostVerifyOtpAsync()
        {
            CurrentPhase = 2;
            
            var storedOtp = HttpContext.Session.GetString("PasswordRecoveryOtp");
            var storedEmail = HttpContext.Session.GetString("PasswordRecoveryEmail");

            if (string.IsNullOrEmpty(storedOtp) || storedEmail != RecoveryEmail)
            {
                CurrentPhase = 1;
                ErrorMessage = "OTP session expired or invalid. Please request a new code.";
                return Page();
            }

            if (UserOtp != storedOtp)
            {
                ErrorMessage = "Invalid OTP code. Please try again.";
                return Page();
            }

            // OTP Verified! Move to Phase 3
            CurrentPhase = 3;
            VerifiedOtpCode = storedOtp; // Carry this forward to ensure they don't jump straight to Phase 3
            
            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            CurrentPhase = 3;

            var storedOtp = HttpContext.Session.GetString("PasswordRecoveryOtp");
            
            // Double check security to prevent direct POST requests to this phase
            if (string.IsNullOrEmpty(storedOtp) || VerifiedOtpCode != storedOtp)
            {
                CurrentPhase = 1;
                ErrorMessage = "Unauthorized request. Please start over.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match or are empty.";
                return Page();
            }

            // Find the user again and update password
            var admin = await _context.AdminAccounts.FirstOrDefaultAsync(a => a.Email == RecoveryEmail);
            if (admin != null)
            {
                admin.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }
            else
            {
                var manager = await _context.BranchManagers.FirstOrDefaultAsync(m => m.Email == RecoveryEmail);
                if (manager != null)
                {
                    manager.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }
                else
                {
                    var staff = await _context.StaffInformations.FirstOrDefaultAsync(s => s.Email == RecoveryEmail);
                    if (staff != null)
                    {
                        staff.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                    }
                    else
                    {
                         ErrorMessage = "User account no longer exists.";
                         return Page();
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Clear the session so OTP can't be reused
            HttpContext.Session.Remove("PasswordRecoveryOtp");
            HttpContext.Session.Remove("PasswordRecoveryEmail");

            // Redirect back to login upon success
            return RedirectToPage("/Login/login");
        }
    }
}
