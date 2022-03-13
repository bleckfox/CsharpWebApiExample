using Microsoft.Data.Sqlite;
using Tele2Task.Controllers;
using Tele2Task.Models;

/// <summary>
/// При первом запуске, если нет файла базы данных, то создадим его
/// и заполним данными. И сразу прочтем, чтобы уже показывать ответ на запросы, а не перезапускать приложение.
/// Если файл базы данных уже существует и заполнен, то скачивать ничего уже не будем.
/// Заполним этими данными список, для быстрого обращения к ним при запросах.
/// С этой целью и нужен список.
/// </summary>

// Проверяем, что база данных существует, иначе создаем
string db_name = "citizens.db";
string db_path = Path.GetFullPath(db_name);

List<Citizens> citizens = new List<Citizens>();

if (File.Exists(db_path))
{
    // Проверяем есть ли запись в базе, иначе загрузим данным и сохраним в базе
    using (var connection = new SqliteConnection($"Data source = {db_name}"))
    {
        // Открываем подключение
        connection.Open();
        // Читаем данные
        ReadData("SELECT * FROM Citizens", connection, db_name);
        // Закрываем подключение
        connection.Close();
    }
}
else
{
    using (var connection = new SqliteConnection($"Data source = {db_name}"))
    {
        connection.Open();
        SqliteCommand command = new SqliteCommand();
        // Создание таблицы
        string createTable = "CREATE TABLE Citizens(" +
            "id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE," +
            "Citizen_id NVARCHAR," +
            "Name NVARCHAR, " +
            "Sex NVARCHAR, " +
            "Age TINYINT )";
        command.Connection = connection;
        command.CommandText = createTable;
        command.ExecuteNonQuery();

        // Запись данных
        WriteData(db_name);

        // Чтение данных
        // При первом запуске только запишем, и чтобы получить данные пришлось бы перезапустить приложение
        ReadData("SELECT * FROM Citizens", connection, db_name);

        connection.Close();
    }
}

// Функция чтения из базы данных
void ReadData(string query, SqliteConnection connection, string db_name)
{
    SqliteCommand command = new SqliteCommand(query, connection);
    try
    {
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            // Если нет строк, вызвать исключение, но не закрывать подключение, а сделать запись
            // Аргументом функции передаем command
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

// Функция сохранения данных json в базе
void WriteData(string db_name)
{
    // Получаем данные json
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
    if (citizens == null) return Results.NotFound(new { message = "Горожанин не найден" });
    return Results.Json(citizens);
});

app.MapGet("/citizens/{id}", (string id) =>
{
    // получаем пользователя по id
    var citizen = from c in citizens
                  where c.id == id
                  select c;
    if (citizens == null) return Results.NotFound(new { message = "Горожанин не найден" });
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/from/{x}", (int x) =>
{
    var citizen = from c in citizens
                  where c.age >= x
                  select c;
    // если не найден, отправляем статусный код и сообщение об ошибке
    if (citizen == null) return Results.NotFound(new { message = "Горожанин не найден" });
    // если пользователь найден, отправляем его
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/to/{x}", (int x) =>
{
    var citizen = from c in citizens
                  where c.age <= x
                  select c;
    // если не найден, отправляем статусный код и сообщение об ошибке
    if (citizen == null) return Results.NotFound(new { message = "Горожанин не найден" });
    // если пользователь найден, отправляем его
    return Results.Json(citizen);
});

app.MapGet("/citizens/age/from/{x}/to/{y}", (int x, int y) =>
{
    var citizen = from c in citizens
                  where c.age >= x && c.age <= y
                  select c;
    // если не найден, отправляем статусный код и сообщение об ошибке
    if (citizen == null) return Results.NotFound(new { message = "Горожанин не найден" });
    // если пользователь найден, отправляем его
    return Results.Json(citizen);
});

app.UseAuthorization();

app.MapControllers();

app.Run();
