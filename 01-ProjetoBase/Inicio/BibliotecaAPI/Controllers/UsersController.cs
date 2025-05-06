using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public UsersController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar(CredencialUserDto userDto)
    {
        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email
        };

        var resultado = await _userManager.CreateAsync(user, userDto.Password);

        if (resultado.Succeeded)
        {
            var respostaAutenticacao = await GerarTokenJwtAsync(user);
            return Ok(respostaAutenticacao);
        }

        foreach (var error in resultado.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return ValidationProblem();
    }

    private async Task<RespostaAutenticacaoDto> GerarTokenJwtAsync(ApplicationUser usuario)
    {
        var claims = new List<Claim>
        {
            new Claim("email", usuario.Email!),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
            // você pode adicionar mais claims personalizados aqui
        };

        var claimsExistentes = await _userManager.GetClaimsAsync(usuario);
        claims.AddRange(claimsExistentes);

        // Carregando configurações do appsettings.json
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credenciais = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256);

        var expiracao = DateTime.UtcNow.AddHours(2); // duração ideal para access token

        var tokenJwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiracao,
            signingCredentials: credenciais
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenJwt);

        return new RespostaAutenticacaoDto
        {
            Token = token,
            Expiracao = expiracao
        };
    }
}
