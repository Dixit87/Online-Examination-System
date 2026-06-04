using System.Data;

namespace Online_Examination_System.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
