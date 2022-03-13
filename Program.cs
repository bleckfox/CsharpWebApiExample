using Microsoft.Data.Sqlite;
using Tele2Task.Controllers;
using Tele2Task.Models;

/// <summary>
/// ��� ������ �������, ���� ��� ����� ���� ������, �� �������� ���
/// � �������� �������. � ����� �������, ����� ��� ���������� ����� �� �������, � �� ������������� ����������.
/// ���� ���� ���� ������ ��� ���������� � ��������, �� ��������� ������ ��� �� �����.
/// �������� ����� ������� ������, ��� �������� ��������� � ��� ��� ��������.
/// � ���� ����� � ����� ������.
/// </summary>

// ���������, ��� ���� ������ ����������, ����� �������
string db_name = "citizens.db";
string db_path = Path.GetFullPath(db_name);

List<Citizens> citizens = new List<Citizens>();

if (File.Exists(db_path))
{
    // ��������� ���� �� ������ � ����, ����� �������� ������ � �������� � ����
    using (var connection = new SqliteConnection($"Data source = {db_name}"))
    {
        // ��������� �����������
        connection.Open();
        // ������ ������
        ReadData("SELECT * FROM Citizens", connection, db_name);
        // ��������� �����������
        connection.Close();
    }
}
else
{
    using (var connection = new SqliteConnection($"Data source = {db_name}"))
    {
        connection.Open();
        SqliteCommand command = new SqliteCommand();
        // �������� �������
        string createTable = "CREATE TABLE Citizens(" +
            "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
            "Citizen_id NVARCHAR," +
            "Name NVARCHAR, " +
            "Sex NVARCHAR, " +
            "Age TINYINT )";
        command.Connection = connection;
        command.CommandText = createTable;
        command.ExecuteNonQuery();

        // ������ ������
        WriteData(db_name);

        // ������ ������
        // ��� ������ ������� ������ �������, � ����� �������� ������ �������� �� ������������� ����������
        ReadData("SELECT * FROM Citizens", connection, db_name);

        connection.Close();
    }
}

// ������� ������ �� ���� ������
void ReadData(string query, SqliteConnection connection, string db_name)
{
    SqliteCommand command = new SqliteCommand(query, connection);
    try
    {
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            // ���� ��� �����, ������� ����������, �� �� ��������� �����������, � ������� ������
            // ���������� ������� �������� command
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    citizens.Add(
                        new()
                        {
                            id = reader.GetString(1),
                            name = reader.GetString(2),
                            sex = reader.GetString(3),
                            age = reader.GetInt32(4)
                        });
                }
            }
            else
                throw new Exception("Has no rows");
        }
    }
    catch (Exception e)
    {
        WriteData(db_name);
    }
}

// ������� ���������� ������ json � ����
void WriteData(string db_name)
{
    // �������� ������ json
    var citizens = from c in CitizensController.Get()
                   select c;
    string saveQuery;
    using (var connection = new SqliteConnection($"Data source = {db_name}"))
    {
        connection.Open();
        foreach (var c in citizens)
        {
            saveQuery = $"INSERT INTO Citizens(Citizen_id, Name, Sex, Age)" +
                $"VALUES(\"{c.id}\", \"{c.name}\", \"{c.sex}\", {c.age})";
            using (SqliteCommand command = new SqliteCommand())
            {
                command.Connection = connection;
                command.CommandText = saveQuery;
                command.ExecuteNonQuery();
            }
        }
        connection.Close();
    }
        
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/citizens", () =>
{
    if (citizens == null) return Results.NotFound(new { message = "��������� �� ������" });
    return Results.Json(citizens);
});

app.MapGet("/citizens/{id}", (string id) =>
{
    // �������� ������������ �� id
    var citizen = from c in citizens
                  where c.id == id
                  select c;
    if (citizens == null) return Results.NotFound(new { message = "��������� �� ������" });
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/from/{x}", (int x) =>
{
    var citizen = from c in citizens
                  where c.age >= x
                  select c;
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (citizen == null) return Results.NotFound(new { message = "��������� �� ������" });
    // ���� ������������ ������, ���������� ���
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/to/{x}", (int x) =>
{
    var citizen = from c in citizens
                  where c.age <= x
                  select c;
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (citizen == null) return Results.NotFound(new { message = "��������� �� ������" });
    // ���� ������������ ������, ���������� ���
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/from/{x}/to/{y}", (int x, int y) =>
{
    var citizen = from c in citizens
                  where c.age >= x && c.age <= y
                  select c;
    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (citizen == null) return Results.NotFound(new { message = "��������� �� ������" });
    // ���� ������������ ������, ���������� ���
    return Results.Json(citizen);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
