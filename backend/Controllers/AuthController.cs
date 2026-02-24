using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;

namespace DefaultNamespace;

[ApiController]
[Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (registerDTO.Password != registerDTO.ConfirmPassword)
            {
                return BadRequest("Las contraseñas no coinciden");
            }
        
            var user = await _authService.Register(registerDTO.Username, registerDTO.Password, registerDTO.Email);
            if (user == null)
            {
                return BadRequest("El usuario ya existe");
            }
            return Ok(user);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var token = await _authService.Login(loginDTO.Email, loginDTO.Password);
            if (token == null)
            {
                return BadRequest("Credenciales incorrectas");
            }
            return Ok(token);
        }
    }
