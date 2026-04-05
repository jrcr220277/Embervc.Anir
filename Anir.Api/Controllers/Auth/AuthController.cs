using Anir.Data.Identity;
using Anir.Infrastructure.Jwt;
using Anir.Infrastructure.Storage;
using Anir.Shared.Contracts.Auth;
using Anir.Shared.Contracts.User;
using Anir.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;


namespace Anir.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtService _jwtService;
        private readonly IFileStorage _fileStorage;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtService jwtService,
            IFileStorage fileStorage,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _fileStorage = fileStorage;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        // ============================================================
        // ME
        // ============================================================
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ProcessResponse<UserResponse>>> Me(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("ME: No se encontró el Claim NameIdentifier.");
                    return Unauthorized(ProcessResponse<UserResponse>.Fail("No autorizado."));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ME: Usuario no encontrado. UserId={UserId}", userId);
                    return Unauthorized(ProcessResponse<UserResponse>.Fail("Usuario no encontrado."));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var mapped = MapUser(user, roles);

                return Ok(ProcessResponse<UserResponse>.Success(mapped));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en ME.");
                return StatusCode(500, ProcessResponse<UserResponse>.Fail("Ocurrió un error al obtener los datos del usuario."));
            }
        }


        // ============================================================
        // UPDATE PROFILE
        // ============================================================
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ProcessResponse<bool>>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("UpdateProfile: No se encontró el Claim NameIdentifier.");
                    return Unauthorized(ProcessResponse<bool>.Fail("No autorizado."));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("UpdateProfile: Usuario no encontrado. UserId={UserId}", userId);
                    return Unauthorized(ProcessResponse<bool>.Fail("Usuario no encontrado."));
                }

                // Validación básica del request
                if (!ModelState.IsValid)
                {
                    var validation = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(ProcessResponse<bool>.Fail("Datos inválidos.", validation));
                }

                // Actualizar datos
                user.ImagenId = request.ImagenId;
                user.FullName = request.FullName;
                user.ThemeMode = request.ThemeMode;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    );

                    _logger.LogWarning("UpdateProfile: Error actualizando usuario {UserId}: {Errors}",
                        userId, string.Join("; ", result.Errors.Select(e => e.Description)));

                    return BadRequest(ProcessResponse<bool>.Fail("No se pudo actualizar el perfil.", errors));
                }

                _logger.LogInformation("UpdateProfile: Perfil actualizado correctamente para {UserId}", userId);

                return Ok(ProcessResponse<bool>.Success(true, "Perfil actualizado correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado en UpdateProfile.");
                return StatusCode(500, ProcessResponse<bool>.Fail("Ocurrió un error al actualizar el perfil."));
            }
        }


        // ============================================================
        // LOGIN
        // ============================================================
        /// <summary>
        /// Inicia sesión y devuelve token JWT y datos del usuario.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ProcessResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var validation = ModelState
                    .Where(x => x.Value?.Errors.Any() == true)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                return BadRequest(ProcessResponse<LoginResponse>.Fail("Datos inválidos.", validation));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login fallido: email no encontrado {Email}", request.Email);
                return Unauthorized(ProcessResponse<LoginResponse>.Fail("Credenciales inválidas."));
            }

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                _logger.LogWarning("Login fallido: contraseña incorrecta para {Email}", request.Email);
                return Unauthorized(ProcessResponse<LoginResponse>.Fail("Credenciales inválidas."));
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogInformation("Login bloqueado: email no confirmado {Email}", request.Email);
                return Unauthorized(ProcessResponse<LoginResponse>.Fail("Debe confirmar su correo antes de iniciar sesión."));
            }

            if (!user.Active)
            {
                _logger.LogInformation("Login bloqueado: cuenta inactiva {Email}", request.Email);
                return Unauthorized(ProcessResponse<LoginResponse>.Fail("Su cuenta está pendiente de aprobación."));
            }

            // Roles y token EXACTAMENTE como tú los tienes
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            var expirationMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 60;
            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var response = new LoginResponse
            {
                Token = token,
                Expiration = expiration,
                User = MapUser(user, roles)
            };

            return Ok(ProcessResponse<LoginResponse>.Success(
                response,
                "Inicio de sesión exitoso."
            ));
        }


        // ============================================================
        // REGISTER
        // ============================================================
        [HttpPost("register")]
        public async Task<ActionResult<ProcessResponse<RegisterResponse>>> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ============================
                // VALIDACIÓN DEL MODELO
                // ============================
                if (!ModelState.IsValid)
                {
                    var validation = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning("Register: Datos inválidos para {Email}", request.Email);
                    return BadRequest(ProcessResponse<RegisterResponse>.Fail("Datos inválidos.", validation));
                }

                // ============================
                // VALIDAR SI EL EMAIL YA EXISTE
                // ============================
                var exists = await _userManager.FindByEmailAsync(request.Email);
                if (exists != null)
                {
                    _logger.LogInformation("Register fallido: email ya registrado {Email}", request.Email);
                    return BadRequest(ProcessResponse<RegisterResponse>.Fail("El correo ya está registrado."));
                }

                // ============================
                // CREAR USUARIO
                // ============================
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName ?? string.Empty,
                    Active = false, // SIEMPRE false hasta que un admin lo active
                    EmailConfirmed = false,
                    ThemeMode = ThemeMode.Auto,
                    MustChangePassword = false
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    );

                    _logger.LogWarning("Register: Error creando usuario {Email}: {Errors}",
                        request.Email, string.Join("; ", result.Errors.Select(e => e.Description)));

                    return BadRequest(ProcessResponse<RegisterResponse>.Fail("Error creando usuario.", errors));
                }

                // ============================
                // ASIGNAR ROL POR DEFECTO
                // ============================
                var roleResult = await _userManager.AddToRoleAsync(user, "Usuario");
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    );

                    _logger.LogError("Register: Error asignando rol Usuario a {Email}: {Errors}",
                        user.Email, string.Join("; ", roleResult.Errors.Select(e => e.Description)));

                    return BadRequest(ProcessResponse<RegisterResponse>.Fail("Error asignando rol al usuario.", errors));
                }

                // ============================
                // ENVIAR EMAIL DE CONFIRMACIÓN
                // ============================
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                try
                {
                    var encoded = Uri.EscapeDataString(token);
                    var frontend = _configuration["FrontendUrl"];
                    var confirmUrl = $"{frontend}/confirm-email?userId={user.Id}&token={encoded}";

                    await _emailService.SendEmailConfirmationAsync(user.Email!, confirmUrl);

                    _logger.LogInformation("Register: Correo de confirmación enviado a {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Register: Error enviando correo de confirmación a {Email}", user.Email);
                }

                // ============================
                // RESPUESTA
                // ============================
                var response = new RegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    EmailConfirmed = user.EmailConfirmed,
                    Active = user.Active
                };

                _logger.LogInformation("Register: Usuario registrado {Email} (Id: {Id})", user.Email, user.Id);

                return Ok(ProcessResponse<RegisterResponse>.Success(
                    response,
                    "Usuario registrado correctamente. Revise su correo para confirmar la cuenta."
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: Error inesperado registrando usuario {Email}", request.Email);
                return StatusCode(500, ProcessResponse<RegisterResponse>.Fail("Ocurrió un error al registrar el usuario."));
            }
        }



        // ============================================================
        // CONFIRM EMAIL
        // ============================================================
        // Este endpoint es ESPECIAL dentro de ANIR:
        // - Es público (AllowAnonymous)
        // - Se accede desde un enlace enviado por correo
        // - NO lo consume Blazor WASM como JSON
        // - Debe devolver TEXTO PLANO, no ProcessResponse
        // - Debe ser simple, directo y compatible con navegadores
        // ============================================================

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            try
            {
                // ------------------------------------------------------------
                // Validación básica: el enlace debe contener userId y token
                // ------------------------------------------------------------
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("ConfirmEmail: Parámetros inválidos. userId={UserId}, token={Token}", userId, token);
                    return BadRequest("El enlace es inválido o está incompleto.");
                }

                // ------------------------------------------------------------
                // Buscar usuario por ID
                // ------------------------------------------------------------
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ConfirmEmail: Usuario no encontrado. userId={UserId}", userId);
                    return BadRequest("El usuario no existe.");
                }

                // ------------------------------------------------------------
                // Confirmar el correo usando el token recibido
                // ------------------------------------------------------------
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("ConfirmEmail: Token inválido o expirado para {Email}. Errores: {Errors}",
                        user.Email, string.Join("; ", result.Errors.Select(e => e.Description)));

                    return BadRequest("El enlace es inválido o ha expirado.");
                }

                // ------------------------------------------------------------
                // Éxito: correo confirmado
                // ------------------------------------------------------------
                _logger.LogInformation("ConfirmEmail: Correo confirmado correctamente para {Email}", user.Email);

                // IMPORTANTE:
                // Devolvemos TEXTO PLANO porque este endpoint se abre desde un navegador,
                // no desde Blazor WASM. Si devolvemos JSON, el usuario verá JSON en pantalla.
                return Ok("Correo confirmado. Espere a que un administrador active su cuenta.");
            }
            catch (Exception ex)
            {
                // ------------------------------------------------------------
                // Error inesperado
                // ------------------------------------------------------------
                _logger.LogError(ex, "ConfirmEmail: Error inesperado para userId={UserId}", userId);
                return StatusCode(500, "Ocurrió un error al confirmar el correo.");
            }
        }


        // ============================================================
        // CHANGE PASSWORD
        // ============================================================
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ProcessResponse<bool>>> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ============================
                // VALIDACIÓN DEL MODELO
                // ============================
                if (!ModelState.IsValid)
                {
                    var validation = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning("ChangePassword: Datos inválidos.");
                    return BadRequest(ProcessResponse<bool>.Fail("Datos inválidos.", validation));
                }

                // ============================
                // OBTENER USUARIO
                // ============================
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("ChangePassword: No se encontró el Claim NameIdentifier.");
                    return Unauthorized(ProcessResponse<bool>.Fail("No autorizado."));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("ChangePassword: Usuario no encontrado. UserId={UserId}", userId);
                    return Unauthorized(ProcessResponse<bool>.Fail("Usuario no encontrado."));
                }

                // ============================
                // VALIDAR POLÍTICA DE CONTRASEÑA
                // ============================
                foreach (var validator in _userManager.PasswordValidators)
                {
                    var validationResult = await validator.ValidateAsync(_userManager, user, request.NewPassword);
                    if (!validationResult.Succeeded)
                    {
                        var errors = validationResult.Errors.ToDictionary(
                            e => e.Code,
                            e => new[] { e.Description }
                        );

                        _logger.LogWarning("ChangePassword: Nueva contraseña no cumple la política para {UserId}: {Errors}",
                            userId, string.Join("; ", validationResult.Errors.Select(e => e.Description)));

                        return BadRequest(ProcessResponse<bool>.Fail("La nueva contraseña no cumple la política de seguridad.", errors));
                    }
                }

                // ============================
                // CAMBIAR CONTRASEÑA
                // ============================
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    );

                    _logger.LogWarning("ChangePassword: Falló el cambio de contraseña para {UserId}: {Errors}",
                        userId, string.Join("; ", result.Errors.Select(e => e.Description)));

                    return BadRequest(ProcessResponse<bool>.Fail("No se pudo cambiar la contraseña.", errors));
                }

                _logger.LogInformation("ChangePassword: Contraseña actualizada correctamente para {UserId}", userId);

                return Ok(ProcessResponse<bool>.Success(true, "Contraseña actualizada correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePassword: Error inesperado para {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                return StatusCode(500, ProcessResponse<bool>.Fail("Ocurrió un error al cambiar la contraseña."));
            }
        }


        // ============================================================
        // FORGOT PASSWORD
        // ============================================================
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ProcessResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                // ============================
                // VALIDACIÓN DEL MODELO
                // ============================
                if (!ModelState.IsValid)
                {
                    var validation = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning("ForgotPassword: Datos inválidos.");
                    return BadRequest(ProcessResponse<bool>.Fail("Datos inválidos.", validation));
                }

                // ============================
                // BUSCAR USUARIO
                // ============================
                var user = await _userManager.FindByEmailAsync(model.Email);

                // 🔥 Seguridad: NO revelar si el usuario existe o no
                if (user == null)
                {
                    _logger.LogWarning("ForgotPassword: Email no encontrado {Email}", model.Email);
                    return Ok(ProcessResponse<bool>.Success(true, "Si el correo existe, se enviará un enlace para restablecer la contraseña."));
                }

                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("ForgotPassword: Email no confirmado {Email}", model.Email);
                    return BadRequest(ProcessResponse<bool>.Fail("Debe confirmar su correo antes de restablecer la contraseña."));
                }

                if (!user.Active)
                {
                    _logger.LogWarning("ForgotPassword: Cuenta inactiva {Email}", model.Email);
                    return BadRequest(ProcessResponse<bool>.Fail("Su cuenta no está activa."));
                }

                // ============================
                // GENERAR TOKEN
                // ============================
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Convertir token y email a Base64Url
                var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
                var encodedEmail = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(user.Email));

                var frontend = _configuration["FrontendUrl"];
                var url = $"{frontend}/reset-password?email={encodedEmail}&token={encodedToken}";

                // ============================
                // ENVIAR CORREO
                // ============================
                await _emailService.SendPasswordResetAsync(user.Email!, url);

                _logger.LogInformation("ForgotPassword: Correo de restablecimiento enviado a {Email}", user.Email);

                return Ok(ProcessResponse<bool>.Success(true, "Si el correo existe, se enviará un enlace para restablecer la contraseña."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForgotPassword: Error inesperado para {Email}", model.Email);
                return StatusCode(500, ProcessResponse<bool>.Fail("Ocurrió un error al procesar la solicitud."));
            }
        }




        // ============================================================
        // RESET PASSWORD
        // ============================================================
        [HttpPost("reset-password")]
        public async Task<ActionResult<ProcessResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                // ============================
                // VALIDACIÓN DEL MODELO
                // ============================
                if (!ModelState.IsValid)
                {
                    var validation = ModelState
                        .Where(x => x.Value?.Errors.Any() == true)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    _logger.LogWarning("ResetPassword: Datos inválidos.");
                    return BadRequest(ProcessResponse<bool>.Fail("Datos inválidos.", validation));
                }

                // ============================
                // DECODIFICAR EMAIL
                // ============================
                string decodedEmail;
                try
                {
                    decodedEmail = System.Text.Encoding.UTF8.GetString(
                        WebEncoders.Base64UrlDecode(request.Email)
                    );
                }
                catch
                {
                    _logger.LogWarning("ResetPassword: Email inválido o corrupto.");
                    return BadRequest(ProcessResponse<bool>.Fail("El enlace de restablecimiento no es válido."));
                }

                var user = await _userManager.FindByEmailAsync(decodedEmail);
                if (user == null)
                {
                    _logger.LogWarning("ResetPassword: Usuario no encontrado para email {Email}", decodedEmail);
                    return BadRequest(ProcessResponse<bool>.Fail("Usuario no encontrado."));
                }

                // ============================
                // DECODIFICAR TOKEN
                // ============================
                string decodedToken;
                try
                {
                    decodedToken = System.Text.Encoding.UTF8.GetString(
                        WebEncoders.Base64UrlDecode(request.Token)
                    );
                }
                catch
                {
                    _logger.LogWarning("ResetPassword: Token inválido o corrupto para {Email}", decodedEmail);
                    return BadRequest(ProcessResponse<bool>.Fail("El enlace de restablecimiento no es válido."));
                }

                // ============================
                // RESTABLECER CONTRASEÑA
                // ============================
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    );

                    _logger.LogWarning("ResetPassword: Error restableciendo contraseña para {Email}: {Errors}",
                        decodedEmail, string.Join("; ", result.Errors.Select(e => e.Description)));

                    return BadRequest(ProcessResponse<bool>.Fail("No se pudo restablecer la contraseña.", errors));
                }

                _logger.LogInformation("ResetPassword: Contraseña restablecida correctamente para {Email}", decodedEmail);

                return Ok(ProcessResponse<bool>.Success(true, "Contraseña restablecida correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ResetPassword: Error inesperado para email codificado {Email}", request.Email);
                return StatusCode(500, ProcessResponse<bool>.Fail("Ocurrió un error al restablecer la contraseña."));
            }
        }

        // ============================================================
        // Helpers
        // ============================================================
        private UserResponse MapUser(ApplicationUser user, IList<string> roles)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                Active = user.Active,
                Roles = roles?.ToList() ?? new List<string>(),
                ImagenId = user.ImagenId,
                ImagenUrl = string.IsNullOrWhiteSpace(user.ImagenId)
                    ? null
                    : $"{Request.Scheme}://{Request.Host}/{user.ImagenId}",
                ThemeMode = user.ThemeMode
            };
        }


    }
}
