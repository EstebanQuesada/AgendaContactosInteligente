using Microsoft.Data.SqlClient;

namespace AgendaContactosInteligente.Helpers;

public static class DbExceptionHelper
{
    public static string GetFriendlyMessage(Exception ex)
    {
        if (ex is SqlException sqlEx)
            return sqlEx.Message;

        return "Ha ocurrido un error inesperado. Intente nuevamente.";
    }
}