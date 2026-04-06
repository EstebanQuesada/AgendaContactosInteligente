using System.Data;
using AgendaContactosInteligente.Models;
using Dapper;

namespace AgendaContactosInteligente.Data;

public class EtiquetaRepository : IEtiquetaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EtiquetaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Etiqueta>> ListAsync(bool soloActivas = true)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Etiqueta>(
            "dbo.usp_Etiqueta_List",
            new { SoloActivas = soloActivas },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<Etiqueta?> GetByIdAsync(int etiquetaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Etiqueta>(
            "dbo.usp_Etiqueta_GetById",
            new { EtiquetaID = etiquetaId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Etiqueta etiqueta)
    {
        using var connection = _connectionFactory.CreateConnection();

        var created = await connection.QuerySingleAsync<Etiqueta>(
            "dbo.usp_Etiqueta_Create",
            new
            {
                etiqueta.Nombre,
                etiqueta.ColorHex
            },
            commandType: CommandType.StoredProcedure);

        return created.EtiquetaID;
    }

    public async Task UpdateAsync(Etiqueta etiqueta)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Etiqueta_Update",
            new
            {
                etiqueta.EtiquetaID,
                etiqueta.Nombre,
                etiqueta.ColorHex
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteAsync(int etiquetaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_Etiqueta_Delete",
            new { EtiquetaID = etiquetaId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task AssignToContactoAsync(int contactoId, int etiquetaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_ContactoEtiqueta_Add",
            new
            {
                ContactoID = contactoId,
                EtiquetaID = etiquetaId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task RemoveFromContactoAsync(int contactoId, int etiquetaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_ContactoEtiqueta_Remove",
            new
            {
                ContactoID = contactoId,
                EtiquetaID = etiquetaId
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IReadOnlyList<Etiqueta>> GetEtiquetasByContactoIdAsync(int contactoId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Etiqueta>(
            "dbo.usp_ContactoEtiqueta_ListByContactoId",
            new { ContactoID = contactoId },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }

    public async Task<IReadOnlyList<Contacto>> GetContactosAsync(int? etiquetaId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var rows = await connection.QueryAsync<Contacto>(
            "dbo.usp_Contacto_ListWithOptionalEtiqueta",
            new { EtiquetaID = etiquetaId },
            commandType: CommandType.StoredProcedure);

        return rows.ToList();
    }
}