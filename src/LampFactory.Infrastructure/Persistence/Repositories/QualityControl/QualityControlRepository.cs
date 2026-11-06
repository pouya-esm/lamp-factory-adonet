using LampFactory.Infrastructure.Persistence;
using QualityControlEntity = LampFactory.Domain.QualityControl.QualityControl;
using LampFactory.Application.Abstractions.Persistence.QualityControl;

using LampFactory.Infrastructure.Security;
using Microsoft.Data.SqlClient;

namespace LampFactory.Infrastructure.Persistence.Repositories.QualityControl
{
    public class QualityControlRepository : IQualityControlRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public QualityControlRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<int> CreateAsync(QualityControlEntity entity)
        {
            string query = @"INSERT INTO QualityControl (QualityControlType, InspectionType, SampleRatio, BatchNumber, QualityControlDate, QualityControlAcceptance, ProductionDocumentID, CreatedBy)
                            VALUES (@QualityControlType, @InspectionType, @SampleRatio, @BatchNumber, @QualityControlDate, @QualityControlAcceptance, @ProductionDocumentID, @CreatedBy);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@QualityControlType", entity.QualityControlType.ToString());
                cmd.Parameters.AddWithValue("@InspectionType", (object?)entity.InspectionType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SampleRatio", (object?)entity.SampleRatio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BatchNumber", (object?)entity.BatchNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@QualityControlDate", (object?)entity.QualityControlDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@QualityControlAcceptance", (object?)entity.QualityControlAcceptance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductionDocumentID", entity.ProductionDocumentID);
                cmd.Parameters.AddWithValue("@CreatedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }

        public async Task<List<QualityControlEntity>> GetAllAsync()
        {
            var list = new List<QualityControlEntity>();
            string query = "SELECT QualityControlID, QualityControlType, InspectionType, SampleRatio, BatchNumber, QualityControlDate, QualityControlAcceptance, ProductionDocumentID, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM QualityControl";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new QualityControlEntity
                        {
                            QualityControlID = reader.GetInt32(0),
                            QualityControlType = Enum.Parse<QualityControlType>(reader.GetString(1)),
                            InspectionType = reader.IsDBNull(2) ? null : reader.GetString(2),
                            SampleRatio = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            BatchNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            QualityControlDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            QualityControlAcceptance = reader.IsDBNull(6) ? null : reader.GetBoolean(6),
                            ProductionDocumentID = reader.GetInt32(7),
                            CreatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                            CreatedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            ModifiedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                            ModifiedBy = reader.IsDBNull(11) ? null : reader.GetInt32(11)
                        });
                    }
                }
            }
            return list;
        }

        public async Task<QualityControlEntity?> GetByIdAsync(int id)
        {
            string query = "SELECT QualityControlID, QualityControlType, InspectionType, SampleRatio, BatchNumber, QualityControlDate, QualityControlAcceptance, ProductionDocumentID, CreatedAt, CreatedBy, ModifiedAt, ModifiedBy FROM QualityControl WHERE QualityControlID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new QualityControlEntity
                        {
                            QualityControlID = reader.GetInt32(0),
                            QualityControlType = Enum.Parse<QualityControlType>(reader.GetString(1)),
                            InspectionType = reader.IsDBNull(2) ? null : reader.GetString(2),
                            SampleRatio = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                            BatchNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                            QualityControlDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                            QualityControlAcceptance = reader.IsDBNull(6) ? null : reader.GetBoolean(6),
                            ProductionDocumentID = reader.GetInt32(7),
                            CreatedAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                            CreatedBy = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                            ModifiedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                            ModifiedBy = reader.IsDBNull(11) ? null : reader.GetInt32(11)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<bool> RemoveAsync(int id)
        {
            string query = "DELETE FROM QualityControl WHERE QualityControlID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> UpdateAsync(QualityControlEntity entity)
        {
            string query = @"UPDATE QualityControl
                            SET QualityControlType = @QualityControlType, InspectionType = @InspectionType,
                                SampleRatio = @SampleRatio, BatchNumber = @BatchNumber, QualityControlDate = @QualityControlDate,
                                QualityControlAcceptance = @QualityControlAcceptance,
                                ProductionDocumentID = @ProductionDocumentID,
                                ModifiedAt = GETDATE(), ModifiedBy = @ModifiedBy
                            WHERE QualityControlID = @Id";

            using (var conn = _context.CreateConnection())
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", entity.QualityControlID);
                cmd.Parameters.AddWithValue("@QualityControlType", entity.QualityControlType.ToString());
                cmd.Parameters.AddWithValue("@InspectionType", (object?)entity.InspectionType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SampleRatio", (object?)entity.SampleRatio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BatchNumber", (object?)entity.BatchNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@QualityControlDate", (object?)entity.QualityControlDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@QualityControlAcceptance", (object?)entity.QualityControlAcceptance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProductionDocumentID", entity.ProductionDocumentID);
                cmd.Parameters.AddWithValue("@ModifiedBy", _currentUserService.ActorId);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }
    }
}
