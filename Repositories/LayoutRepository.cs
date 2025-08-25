using BTL.Web.Data;
using BTL.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace BTL.Web.Repositories
{
    public class LayoutRepository : ILayoutRepository
    {
        private readonly DatabaseContext _context;

        public LayoutRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<LayoutResponse?> GetLayoutAsync(string? layoutName = null)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_LoadLayout", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (!string.IsNullOrEmpty(layoutName))
            {
                command.Parameters.AddWithValue("@LayoutName", layoutName);
            }

            var layoutResponse = new LayoutResponse();
            var tables = new List<LayoutTableData>();

            using var reader = await command.ExecuteReaderAsync();

            // Đọc thông tin layout
            if (await reader.ReadAsync())
            {
                layoutResponse.layout_id = reader.GetInt32("layout_id");
                layoutResponse.layout_name = reader.GetString("layout_name");
                layoutResponse.grid_size = reader.GetInt32("grid_size");
                layoutResponse.created_date = reader.GetDateTime("created_date");
                layoutResponse.updated_date = reader.GetDateTime("updated_date");
            }
            else
            {
                return null; // Không tìm thấy layout
            }

            // Đọc chi tiết layout
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(new LayoutTableData
                {
                    id = reader.GetInt32("ban_id"),
                    name = reader.GetString("so_hieu"),
                    capacity = reader.GetInt32("suc_chua"),
                    type = reader.GetInt32("loai_ban_id"),
                    position = new LayoutPosition
                    {
                        x = (float)reader.GetDouble("position_x"),
                        y = (float)reader.GetDouble("position_y")
                    }
                });
            }

            layoutResponse.tables = tables;
            layoutResponse.table_count = tables.Count;

            return layoutResponse;
        }

        public async Task<List<LayoutResponse>> GetLayoutListAsync()
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_GetLayoutList", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            var layouts = new List<LayoutResponse>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                layouts.Add(new LayoutResponse
                {
                    layout_id = reader.GetInt32("layout_id"),
                    layout_name = reader.GetString("layout_name"),
                    grid_size = reader.GetInt32("grid_size"),
                    created_date = reader.GetDateTime("created_date"),
                    updated_date = reader.GetDateTime("updated_date"),
                    table_count = reader.GetInt32("table_count")
                });
            }

            return layouts;
        }

        public async Task<int> SaveLayoutAsync(LayoutSaveRequest request)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_SaveLayout", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@LayoutName", request.layout_name);
            command.Parameters.AddWithValue("@GridSize", request.grid_size);

            // Chuyển đổi dữ liệu tables thành JSON
            var layoutData = new
            {
                tables = request.tables
            };
            command.Parameters.AddWithValue("@LayoutData", JsonSerializer.Serialize(layoutData));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> DeleteLayoutAsync(int layoutId)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand("sp_DeleteLayout", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@LayoutId", layoutId);

            await command.ExecuteNonQueryAsync();
            return true;
        }

        public async Task<bool> LayoutExistsAsync(string layoutName)
        {
            using var connection = _context.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(
                "SELECT COUNT(1) FROM Layout WHERE layout_name = @LayoutName AND is_active = 1",
                connection);

            command.Parameters.AddWithValue("@LayoutName", layoutName);

            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
    }
}
