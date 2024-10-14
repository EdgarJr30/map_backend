using map_backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;
using System.Data;
using System.Collections.Generic;
using static map_backend.Models.AuthModels;

namespace map_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            string query = @"
                            SELECT * FROM db_map_library.usuarios 
                            WHERE email = @Email;";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@Email", email);

                        using (MySqlDataReader myReader = (MySqlDataReader)await sqlCommand.ExecuteReaderAsync())
                        {
                            if (await myReader.ReadAsync())
                            {
                                string storedPasswordHash = myReader.GetString("passwordHash");

                                var passwordHasher = new PasswordHasher<string>();
                                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, storedPasswordHash, password);

                                if (passwordVerificationResult == PasswordVerificationResult.Success)
                                {
                                    var userId = myReader.GetInt32("id");
                                    var username = myReader.GetString("username");
                                    var roleId = myReader.GetInt32("roleId");

                                    var claims = new[]
                                    {
                                new Claim(JwtRegisteredClaimNames.Sub, username),
                                new Claim(JwtRegisteredClaimNames.Email, email),
                                new Claim("role", roleId.ToString()),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            };

                                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Y2xhdmUtc3VwZXItc2VjdXJlLWtleS1zdHJpbmctYmFzZTY0"));
                                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                                    var token = new JwtSecurityToken(
                                        issuer: "http://localhost:12990/",
                                        audience: "http://localhost:5173/",
                                        claims: claims,
                                        expires: DateTime.Now.AddMinutes(30),
                                        signingCredentials: creds);

                                    Models.AuthModels.LoginModel login = new Models.AuthModels.LoginModel()
                                    {
                                        id = userId,
                                        username = username,
                                        email = email,
                                        roleId = roleId,
                                    };

                                    return Ok(new
                                    {
                                        token = new JwtSecurityTokenHandler().WriteToken(token),
                                        expiration = token.ValidTo,
                                        user = login 
                                    });
                                }
                                else
                                {
                                    return Unauthorized(new { message = "Contraseña incorrecta." });
                                }
                            }
                            else
                            {
                                return NotFound(new { message = "Usuario no encontrado." });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }




        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string email, string password, int roleId)
        {
            string checkUserQuery = @"
                                    SELECT COUNT(*) FROM db_map_library.usuarios 
                                    WHERE email = @Email;";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand checkUserCommand = new MySqlCommand(checkUserQuery, mySqlConnection))
                    {
                        checkUserCommand.Parameters.AddWithValue("@Email", email);
                        int userExists = Convert.ToInt32(await checkUserCommand.ExecuteScalarAsync());

                        if (userExists > 0)
                        {
                            return Conflict(new { message = "El usuario ya existe." });
                        }
                    }

                    var passwordHasher = new PasswordHasher<string>();
                    string hashedPassword = passwordHasher.HashPassword(null, password);

                    string insertUserQuery = @"
                                            INSERT INTO db_map_library.usuarios (username, email, passwordHash, roleId) 
                                            VALUES (@Username, @Email, @PasswordHash, @RoleId);";

                    using (MySqlCommand insertUserCommand = new MySqlCommand(insertUserQuery, mySqlConnection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@Username", username);
                        insertUserCommand.Parameters.AddWithValue("@Email", email);
                        insertUserCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        insertUserCommand.Parameters.AddWithValue("@RoleId", roleId);

                        int rowsAffected = await insertUserCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return Ok(new { message = "Registro exitoso." });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error al registrar el usuario." });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

    }
}
