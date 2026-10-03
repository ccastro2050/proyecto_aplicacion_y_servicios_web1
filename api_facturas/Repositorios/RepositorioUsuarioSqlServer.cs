// ============================================================
// RepositorioUsuarioSqlServer — la capa de DATOS de usuario.
//
// AQUI LA CONTRASEÑA SE GUARDA TAL CUAL, Y ES A PROPOSITO.
//
// Esto esta mal, y esta mal deliberadamente. Conviene leer por que antes de
// copiarlo a cualquier otro sitio:
//
//   EL HASH ES LA PRIMERA DE LAS TRES COSAS QUE LLEGAN CON LA VERSION 3.
//   Ponerlo aqui seria anticipar —la constitucion lo prohibe— y ademas le
//   quitaria a la v3 su primera leccion: el estudiante no veria POR QUE hay
//   que arreglarlo si ya estuviera arreglado.
//
// LO QUE LA v3 HACE CON ESTO:
//
//   * Agrega `BCrypt.Net-Next` al proyecto y calcula el hash AQUI, justo
//     antes de persistir — con costo 12.
//   * Vuelve a sembrar las ocho filas de la base con hash, EN EL SCRIPT.
//   * Y agrega la comprobación de la contraseña, que hoy no existe: en esta
//     versión el usuario es una fila más y no hay con qué identificarse.
//
// Y la columna ya esta lista para recibirlo: es VARCHAR(200) y no 20, porque
// un hash de bcrypt ocupa 60 caracteres.
//
// LO QUE SI ESTA BIEN DESDE ESTA VERSION, y no es anticipar la v3: NINGUN
// SELECT proyecta la columna `contrasena`. El modelo `Usuario` tiene solo el
// email, asi que ni la clave ni su hash pueden viajar en una respuesta. Una
// respuesta no debe traer un secreto que nadie pidio, y eso vale siempre.
//
// SQL a mano + Dapper (Art. 2). Dialecto SQL Server.
// ============================================================

using ApiFacturas.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiFacturas.Repositorios;

public class RepositorioUsuarioSqlServer : IRepositorioUsuario
{
    private readonly string _cadenaConexion;

    public RepositorioUsuarioSqlServer(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    private SqlConnection CrearConexion() => new(_cadenaConexion);

    public async Task<List<Usuario>> ObtenerTodosAsync(int limite)
    {
        // SOLO email: la contraseña no sale, ni ahora que es texto plano ni
        // cuando la v3 la convierta en hash.
        const string sql = @"SELECT TOP (@limite) email FROM usuario ORDER BY email";
        await using var conexion = CrearConexion();
        return (await conexion.QueryAsync<Usuario>(sql, new { limite })).ToList();
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        const string sql = @"SELECT email FROM usuario WHERE email = @email";
        await using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<Usuario>(sql, new { email });
    }

    public async Task CrearAsync(string email, string contrasena)
    {
        // La v3 pondrá aquí el hash. Hoy entra tal cual.
        const string sql = @"INSERT INTO usuario (email, contrasena)
                             VALUES (@email, @contrasena)";
        await using var conexion = CrearConexion();
        await conexion.ExecuteAsync(sql, new { email, contrasena });
    }

    public async Task<int> ActualizarContrasenaAsync(string email, string contrasena)
    {
        const string sql = @"UPDATE usuario SET contrasena = @contrasena
                             WHERE email = @email";
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, new { contrasena, email });
    }

    public async Task<int> EliminarAsync(string email)
    {
        // Si el usuario tiene roles asignados, la clave foránea lo impide.
        const string sql = "DELETE FROM usuario WHERE email = @email";
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, new { email });
    }
}
