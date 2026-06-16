using Npgsql;
using System;
using System.Collections.Generic;

class UserService
{
    private readonly string _connectionString;

    public UserService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private void ExecuteNonQuery(string sql, Action<NpgsqlCommand> addParameters)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        addParameters(cmd);
        cmd.ExecuteNonQuery();
    }

    public void ShowUsers()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        string sql = "SELECT id, name, age, city, email FROM users ORDER BY id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\n Список пользователей:");
        Console.WriteLine("ID / Имя / Возраст / Город / Email");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["age"]} | {reader["city"]} | {reader["email"]}");
            count++;
        }
        Console.WriteLine(count == 0 ? "Нет пользователей." : $"Всего: {count}");
    }

    public void AddUser()
    {
        Console.Write("Имя: ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Имя не может быть пустым."); return; }

        Console.Write("Возраст: ");
        if (!int.TryParse(Console.ReadLine(), out int age)) { Console.WriteLine("Некорректный возраст."); return; }

        Console.Write("Город: ");
        string city = Console.ReadLine();
        Console.Write("Email: ");
        string email = Console.ReadLine();

        string sql = "INSERT INTO users (name, age, city, email) VALUES (@name, @age, @city, @email);";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@age", age);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@email", email);
        });
        Console.WriteLine("Пользователь добавлен.");
    }

    public void UpdateUser()
    {
        ShowUsers();
        Console.Write("ID пользователя: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }

        Console.Write("Новое имя (пусто = без изменений): ");
        string name = Console.ReadLine();
        Console.Write("Новый возраст (пусто = без изменений): ");
        string ageInput = Console.ReadLine();
        int? age = null;
        if (!string.IsNullOrWhiteSpace(ageInput) && int.TryParse(ageInput, out int a)) age = a;

        Console.Write("Новый город: ");
        string city = Console.ReadLine();
        Console.Write("Новый email: ");
        string email = Console.ReadLine();

        var updates = new List<string>();
        if (!string.IsNullOrWhiteSpace(name)) updates.Add("name = @name");
        if (age.HasValue) updates.Add("age = @age");
        if (!string.IsNullOrWhiteSpace(city)) updates.Add("city = @city");
        if (!string.IsNullOrWhiteSpace(email)) updates.Add("email = @email");
        if (updates.Count == 0) { Console.WriteLine("Нет данных для обновления."); return; }

        string sql = $"UPDATE users SET {string.Join(", ", updates)} WHERE id = @id;";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(name)) cmd.Parameters.AddWithValue("@name", name);
            if (age.HasValue) cmd.Parameters.AddWithValue("@age", age.Value);
            if (!string.IsNullOrWhiteSpace(city)) cmd.Parameters.AddWithValue("@city", city);
            if (!string.IsNullOrWhiteSpace(email)) cmd.Parameters.AddWithValue("@email", email);
        });
        Console.WriteLine("Данные обновлены.");
    }

    public void DeleteUser()
    {
        ShowUsers();
        Console.Write("ID пользователя для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }

        string sql = "DELETE FROM users WHERE id = @id;";
        ExecuteNonQuery(sql, cmd => cmd.Parameters.AddWithValue("@id", id));
        Console.WriteLine("Пользователь удалён.");
    }

    public void FindUserByName()
    {
        Console.Write("Имя или часть имени: ");
        string search = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(search)) { Console.WriteLine("Пустой запрос."); return; }

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        string sql = "SELECT id, name, age, city, email FROM users WHERE name ILIKE @name ORDER BY id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", $"%{search}%");
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n Результаты поиска:");
        Console.WriteLine("ID / Имя / Возраст / Город / Email");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["age"]} | {reader["city"]} | {reader["email"]}");
            count++;
        }
        Console.WriteLine(count == 0 ? "Ничего не найдено." : $"Найдено: {count}");
    }

    public void ShowStatistics()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        string sql = "SELECT COUNT(*) as total, AVG(age) as avg_age FROM users;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            int total = reader.GetInt32(0);
            double avg = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
            Console.WriteLine($"\n Статистика: всего {total}, средний возраст {avg:F2}");
        }
    }
}