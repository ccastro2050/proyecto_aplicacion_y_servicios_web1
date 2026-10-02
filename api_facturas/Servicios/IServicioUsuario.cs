// IServicioUsuario — contrato de negocio de usuario.
// ArgumentException → 400 · NoEncontradoExcepcion → 404 · resto → 500.
//
// SIN comprobación de contraseña: identificarse es la v3. Aquí el usuario
// es una fila más, y eso es lo que la v3 viene a cambiar.

using ApiFacturas.Modelos;

namespace ApiFacturas.Servicios;

public interface IServicioUsuario
{
    Task<List<Usuario>> ListarAsync(int limite);
    Task<Usuario> ObtenerAsync(string email);
    Task CrearAsync(string email, string contrasena);
    Task<int> ActualizarContrasenaAsync(string email, string? contrasena);
    Task<int> EliminarAsync(string email);
}
