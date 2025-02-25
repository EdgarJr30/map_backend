﻿using Microsoft.AspNetCore.Http;
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
    public class AutoresController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AutoresController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public class Autor
        {
            public int id { get; set; }
            public required string nombre { get; set; }
            public required string nacionalidad { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string query = @"
                        SELECT id, nombre, nacionalidad 
                        FROM db_map_library.autores;
                        ";

            List<Autor> autores = new List<Autor>();
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
                                Autor autor = new Autor()
                                {
                                    id = myReader.GetInt32("id"),
                                    nombre = myReader.GetString("nombre"),
                                    nacionalidad = myReader.GetString("nacionalidad"),
                                };
                                autores.Add(autor);
                            }
                        }
                    }
                }

                return new JsonResult(autores);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Autor autor)
        {
            string query = @"
                        INSERT INTO db_map_library.autores (nombre, nacionalidad) 
                        VALUES (@nombre, @nacionalidad);
                        ";

            string sqlDataSource = _configuration.GetConnectionString("LibraryAppConnectionString");

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(sqlDataSource))
                {
                    await mySqlConnection.OpenAsync();

                    using (MySqlCommand sqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@nombre", autor.nombre);
                        sqlCommand.Parameters.AddWithValue("@nacionalidad", autor.nacionalidad);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Autor agregado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Autor autor)
        {
            string query = @"
                        UPDATE db_map_library.autores 
                        SET nombre = @nombre, nacionalidad = @nacionalidad
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
                        sqlCommand.Parameters.AddWithValue("@nombre", autor.nombre);
                        sqlCommand.Parameters.AddWithValue("@nacionalidad", autor.nacionalidad);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult("Autor actualizado correctamente.");
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
                        DELETE FROM db_map_library.autores 
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

                return new JsonResult("Autor eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
