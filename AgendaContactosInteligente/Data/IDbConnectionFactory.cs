using System.Data;

namespace AgendaContactosInteligente.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}