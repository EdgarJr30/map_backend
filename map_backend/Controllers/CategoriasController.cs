using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace map_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CategoriasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class Categoria
        {
            public int id { get; set; }
            public required string nombre { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string query = @"
                        SELECT id, nombre
                        FROM db_map_library.categorias;
                        ";

            List<Categoria> categorias = new List<Categoria>();
            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        using (MySqlDataReader myReader = (MySqlDataReader)await sqlCommand.ExecuteReaderAsync())
                        {
                            while (await myReader.ReadAsync())
                            {
                                Categoria categoria = new Categoria()
                                {
                                    id = myReader.GetInt32("id"),
                                    nombre = myReader.GetString("nombre"),
                                };
                                categorias.Add(categoria);
                            }
                        }
                    }
                }

                return new JsonResult(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Categoria categoria)
        {
            string query = @"
                        INSERT INTO db_map_library.categorias (nombre) 
                        VALUES (@nombre);
                        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@nombre", categoria.nombre);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Categoria agregado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Categoria categoria)
        {
            string query = @"
                        UPDATE db_map_library.categorias 
                        SET nombre = @nombre
                        WHERE id = @id;
                        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@id", id);
                        sqlCommand.Parameters.AddWithValue("@nombre", categoria.nombre);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Categoria actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string query = @"
                        DELETE FROM db_map_library.categorias 
                        WHERE id = @id;
                        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@id", id);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Categoria eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
