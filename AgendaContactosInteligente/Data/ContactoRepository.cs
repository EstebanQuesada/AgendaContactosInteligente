using System.Data;
using AgendaContactosInteligente.Models;
using Dapper;

namespace AgendaContactosInteligente.Data;

public class ContactoRepository : IContactoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ContactoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Contacto>> ListAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Contacto>(
            "dbo.usp_Contacto_List",
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<Contacto?> GetByIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var contacto = await connection.QueryFirstOrDefaultAsync<Contacto>(
            "dbo.usp_Contacto_GetById",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);

        if (contacto is null)
            return null;

        var telefonosTask = GetTelefonosByContactoIdAsync(contactoId);
        var correosTask = GetCorreosByContactoIdAsync(contactoId);
        var direccionTask = GetDireccionByContactoIdAsync(contactoId);
        var notasTask = GetNotasByContactoIdAsync(contactoId);

        await Task.WhenAll(telefonosTask, correosTask, direccionTask, notasTask);

        contacto.Telefonos = telefonosTask.Result.ToList();
        contacto.Correos = correosTask.Result.ToList();
        contacto.Direccion = direccionTask.Result;
        contacto.Notas = notasTask.Result.ToList();

        return contacto;
    }

    public async Task<int> CreateAsync(Contacto contacto)
    {
        using var connection = _connectionFactory.CreateConnection();

        var created = await connection.QuerySingleAsync<Contacto>(
            "dbo.usp_Contacto_Create",
            new
            {
                contacto.Nombre,
                contacto.Apellido,
                contacto.EsFavorito
            },
            commandType: CommandType.StoredProcedure);

        return created.ContactoID;
    }

    public async Task UpdateAsync(Contacto contacto)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Contacto_Update",
            new
            {
                contacto.ContactoID,
                contacto.Nombre,
                contacto.Apellido,
                contacto.EsFavorito
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Contacto_Delete",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Telefono>> GetTelefonosByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Telefono>(
            "dbo.usp_Telefono_ListByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<int> CreateTelefonoAsync(Telefono telefono)
    {
        using var connection = _connectionFactory.CreateConnection();

        var created = await connection.QuerySingleAsync<Telefono>(
            "dbo.usp_Telefono_Create",
            new
            {
                telefono.ContactoID,
                telefono.Numero,
                telefono.Tipo
            },
            commandType: CommandType.StoredProcedure);

        return created.TelefonoID;
    }

    public async Task UpdateTelefonoAsync(Telefono telefono)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Telefono_Update",
            new
            {
                telefono.TelefonoID,
                telefono.Numero,
                telefono.Tipo
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteTelefonoAsync(int telefonoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Telefono_Delete",
            new { TelefonoID = telefonoId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Correo>> GetCorreosByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Correo>(
            "dbo.usp_Correo_ListByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<int> CreateCorreoAsync(Correo correo)
    {
        using var connection = _connectionFactory.CreateConnection();

        var created = await connection.QuerySingleAsync<Correo>(
            "dbo.usp_Correo_Create",
            new
            {
                correo.ContactoID,
                correo.Email
            },
            commandType: CommandType.StoredProcedure);

        return created.CorreoID;
    }

    public async Task UpdateCorreoAsync(Correo correo)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Correo_Update",
            new
            {
                correo.CorreoID,
                correo.Email
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteCorreoAsync(int correoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Correo_Delete",
            new { CorreoID = correoId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Direccion?> GetDireccionByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Direccion>(
            "dbo.usp_Direccion_GetByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task UpsertDireccionAsync(Direccion direccion)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Direccion_UpsertByContactoId",
            new
            {
                direccion.ContactoID,
                direccion.Provincia,
                direccion.Canton,
                direccion.Distrito,
                direccion.DireccionExacta
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteDireccionByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Direccion_DeleteByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Nota>> GetNotasByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Nota>(
            "dbo.usp_Nota_ListByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<int> CreateNotaAsync(Nota nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        var created = await connection.QuerySingleAsync<Nota>(
            "dbo.usp_Nota_Create",
            new
            {
                nota.ContactoID,
                nota.Contenido
            },
            commandType: CommandType.StoredProcedure);

        return created.NotaID;
    }

    public async Task UpdateNotaAsync(Nota nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Nota_Update",
            new
            {
                nota.NotaID,
                nota.Contenido
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteNotaAsync(int notaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Nota_Delete",
            new { NotaID = notaId },
            commandType: CommandType.StoredProcedure);
    }
}