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
    public class LibrosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LibrosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class Libro
        {
            public int id { get; set; }
            public required string titulo { get; set; }
            public required string descripcion { get; set; }
            public required DateTime fecha_publicacion { get; set; }
            public required string imagen { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string query = @"
                        SELECT id, titulo, descripcion, fecha_publicacion, imagen
                        FROM db_map_library.libros;
                        ";

            List<Libro> libros = new List<Libro>();
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
                                Libro libro = new Libro()
                                {
                                    id = myReader.GetInt32("id"),
                                    titulo = myReader.GetString("titulo"),
                                    descripcion = myReader.GetString("descripcion"),
                                    fecha_publicacion = myReader.GetDateTime("fecha_publicacion"),
                                    imagen = myReader.GetString("imagen"),
                                };
                                libros.Add(libro);
                            }
                        }
                    }
                }

                return new JsonResult(libros);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(Libro libro)
        {
            string query = @"
                        INSERT INTO db_map_library.libros (titulo, descripcion, fecha_publicacion, imagen) 
                        VALUES (@titulo, @descripcion, @fecha_publicacion, @imagen);
                        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@titulo", libro.titulo);
                        sqlCommand.Parameters.AddWithValue("@descripcion", libro.descripcion);
                        sqlCommand.Parameters.AddWithValue("@imagen", libro.imagen);
                        sqlCommand.Parameters.AddWithValue("@fecha_publicacion", libro.fecha_publicacion);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Libro agregado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Libro libro)
        {
            string query = @"
                        UPDATE db_map_library.libros 
                        SET titulo = @titulo, descripcion = @descripcion, fecha_publicacion = @fecha_publicacion, imagen = @imagen
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
                        sqlCommand.Parameters.AddWithValue("@titulo", libro.titulo);
                        sqlCommand.Parameters.AddWithValue("@descripcion", libro.descripcion);
                        sqlCommand.Parameters.AddWithValue("@fecha_publicacion", libro.fecha_publicacion);
                        sqlCommand.Parameters.AddWithValue("@imagen", libro.imagen);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Libro actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            string query = @"
                        DELETE FROM db_map_library.libros 
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

                return new JsonResult("Libro eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
