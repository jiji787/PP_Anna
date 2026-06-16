using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static string connectionString = "Host=localhost;Port=5432;Database=Золотце;Username=postgres;Password=postgres890";
    static UserService userService = new UserService(connectionString);

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=+++= Управление базой данных Золотце +===+\n");

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n Выберите раздел:");
            Console.WriteLine("1 - Пользователи");
            Console.WriteLine("2 - Товары");
            Console.WriteLine("3 - Заказы");
            Console.WriteLine("4 - Отчёты");
            Console.WriteLine("5 - Выход");
            Console.Write("Ваш выбор (введите цифру напимер 1 и Enter): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": UserMenu(); break;
                case "2": ProductMenu(); break;
                case "3": OrderMenu(); break;
                case "4": ReportMenu(); break;
                case "5": exit = true; Console.WriteLine("Программа завершена."); break;
                default: Console.WriteLine("Неверный выбор или ввод."); break;
            }
        }
    }

    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    static decimal GetProductPrice(int productId)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = "SELECT price FROM products WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", productId);
        object result = cmd.ExecuteScalar();
        return result == null ? 0 : Convert.ToDecimal(result);
    }

    static void ExecuteNonQuery(string sql, Action<NpgsqlCommand> addParameters)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        addParameters(cmd);
        cmd.ExecuteNonQuery();
    }

    // ПОЛЬЗОВАТЕЛИ
    static void UserMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine("\n в таблице Пользователи");
            Console.WriteLine("1 - Показать всех");
            Console.WriteLine("2 - Добавить");
            Console.WriteLine("3 - Обновить");
            Console.WriteLine("4 - Удалить");
            Console.WriteLine("5 - Поиск по имени");
            Console.WriteLine("6 - Статистика");
            Console.WriteLine("7 - Назад");
            Console.Write("Ваш выбор (введите цифру напимер 1 и Enter): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": userService.ShowUsers(); break;
                case "2": userService.AddUser(); break;
                case "3": userService.UpdateUser(); break;
                case "4": userService.DeleteUser(); break;
                case "5": userService.FindUserByName(); break;
                case "6": userService.ShowStatistics(); break;
                case "7": back = true; break;
                default: Console.WriteLine("Неверный выбор или ввод."); break;
            }
        }
    }

    static void ShowUsers()
    {
        using var conn = new NpgsqlConnection(connectionString);
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

    static void AddUser()
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

    static void UpdateUser()
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

    static void DeleteUser()
    {
        ShowUsers();
        Console.Write("ID пользователя для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }

        string sql = "DELETE FROM users WHERE id = @id;";
        ExecuteNonQuery(sql, cmd => cmd.Parameters.AddWithValue("@id", id));
        Console.WriteLine("Пользователь удалён.");
    }

    static void FindUserByName()
    {
        Console.Write("Имя или часть имени: ");
        string search = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(search)) { Console.WriteLine("Пустой запрос."); return; }

        using var conn = new NpgsqlConnection(connectionString);
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

    static void ShowStatistics()
    {
        using var conn = new NpgsqlConnection(connectionString);
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

    // ТОВАРЫ
    static void ProductMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine("\n в таблице Товары");
            Console.WriteLine("1 - Показать все");
            Console.WriteLine("2 - Добавить");
            Console.WriteLine("3 - Обновить");
            Console.WriteLine("4 - Удалить");
            Console.WriteLine("5 - Поиск по названию");
            Console.WriteLine("6 - Сортировка");
            Console.WriteLine("7 - Назад");
            Console.Write("Ваш выбор(введите цифру напимер 1 и Enter): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ShowProducts(); break;
                case "2": AddProduct(); break;
                case "3": UpdateProduct(); break;
                case "4": DeleteProduct(); break;
                case "5": FindProductByName(); break;
                case "6": SortProductsMenu(); break;
                case "7": back = true; break;
                default: Console.WriteLine("Неверный выбор."); break;
            }
        }
    }

    static void ShowProducts(string orderBy = "id")
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = $"SELECT id, name, price, description FROM products ORDER BY {orderBy};";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n Список товаров:");
        Console.WriteLine("ID / Название / Цена / Описание");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["price"]:F2} | {reader["description"]}");
            count++;
        }
        Console.WriteLine(count == 0 ? "Товаров нет." : $"Всего: {count}");
    }

    static void AddProduct()
    {
        Console.Write("Название: ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name)) { Console.WriteLine("Название не может быть пустым."); return; }

        Console.Write("Цена: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price < 0)
        {
            Console.WriteLine("Некорректная цена.");
            return;
        }

        Console.Write("Описание: ");
        string desc = Console.ReadLine();

        string sql = "INSERT INTO products (name, price, description) VALUES (@name, @price, @desc);";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@desc", desc);
        });
        Console.WriteLine("Товар добавлен.");
    }

    static void UpdateProduct()
    {
        ShowProducts();
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }

        Console.Write("Новое название (пусто = без изменений): ");
        string name = Console.ReadLine();
        Console.Write("Новая цена (пусто = без изменений): ");
        string priceInput = Console.ReadLine();
        decimal? price = null;
        if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal p)) price = p;

        Console.Write("Новое описание (пусто = без изменений): ");
        string desc = Console.ReadLine();

        var updates = new List<string>();
        if (!string.IsNullOrWhiteSpace(name)) updates.Add("name = @name");
        if (price.HasValue) updates.Add("price = @price");
        if (!string.IsNullOrWhiteSpace(desc)) updates.Add("description = @desc");
        if (updates.Count == 0) { Console.WriteLine("Нет данных для обновления."); return; }

        string sql = $"UPDATE products SET {string.Join(", ", updates)} WHERE id = @id;";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@id", id);
            if (!string.IsNullOrWhiteSpace(name)) cmd.Parameters.AddWithValue("@name", name);
            if (price.HasValue) cmd.Parameters.AddWithValue("@price", price.Value);
            if (!string.IsNullOrWhiteSpace(desc)) cmd.Parameters.AddWithValue("@desc", desc);
        });
        Console.WriteLine("Товар обновлён.");
    }

    static void DeleteProduct()
    {
        ShowProducts();
        Console.Write("ID товара для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Неверный ID."); return; }

        string sql = "DELETE FROM products WHERE id = @id;";
        ExecuteNonQuery(sql, cmd => cmd.Parameters.AddWithValue("@id", id));
        Console.WriteLine("Товар удалён.");
    }

    static void FindProductByName()
    {
        Console.Write("Название или часть названия: ");
        string search = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(search)) { Console.WriteLine("Пустой запрос."); return; }

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = "SELECT id, name, price, description FROM products WHERE name ILIKE @name ORDER BY id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", $"%{search}%");
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\nРезультаты поиска:");
        Console.WriteLine("ID | Название | Цена | Описание");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["price"]:F2} | {reader["description"]}");
            count++;
        }
        Console.WriteLine(count == 0 ? "Ничего не найдено." : $"Найдено: {count}");
    }

    static void SortProductsMenu()
    {
        Console.WriteLine("\n Сортировка товаров:");
        Console.WriteLine("1 - По названию (А-Я)");
        Console.WriteLine("2 - По названию (Я-А)");
        Console.WriteLine("3 - По цене (возрастание)");
        Console.WriteLine("4 - По цене (убывание)");
        Console.Write("Ваш выбор (введите цифру напимер 1 и Enter): ");
        string choice = Console.ReadLine();

        switch (choice)
        {
            case "1": ShowProducts("name"); break;
            case "2": ShowProducts("name DESC"); break;
            case "3": ShowProducts("price"); break;
            case "4": ShowProducts("price DESC"); break;
            default: Console.WriteLine("Неверный выбор или ввод."); break;
        }
    }

    // ЗАКАЗЫ
    static void OrderMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine("\n в таблице Заказы");
            Console.WriteLine("1 - Список заказов");
            Console.WriteLine("2 - Детали заказа");
            Console.WriteLine("3 - Создать заказ");
            Console.WriteLine("4 - Добавить товар в заказ");
            Console.WriteLine("5 - Изменить статус");
            Console.WriteLine("6 - Удалить заказ");
            Console.WriteLine("7 - Назад");
            Console.Write("Ваш выбор (введите цифру напимер 1 и Enter): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ShowOrders(); break;
                case "2": ShowOrderDetails(); break;
                case "3": CreateOrder(); break;
                case "4": AddItemToOrder(); break;
                case "5": UpdateOrderStatus(); break;
                case "6": DeleteOrder(); break;
                case "7": back = true; break;
                default: Console.WriteLine("Неверный выбор или ввод."); break;
            }
        }
    }

    static void ShowOrders()
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = @"
            SELECT o.id, u.name, o.order_date, o.status
            FROM orders o
            JOIN users u ON o.user_id = u.id
            ORDER BY o.id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n Список заказов:");
        Console.WriteLine("ID / Покупатель / Дата / Статус");
        int count = 0;
        while (reader.Read())
        {
            Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["order_date"]} | {reader["status"]}");
            count++;
        }
        if (count == 0) Console.WriteLine("Нет заказов.");
    }

    static void ShowOrderDetails()
    {
        Console.Write("ID заказа: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Неверный ID."); return; }

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = @"
            SELECT p.name, oi.quantity, oi.price_at_time, (oi.quantity * oi.price_at_time) as total
            FROM order_items oi
            JOIN products p ON oi.product_id = p.id
            WHERE oi.order_id = @orderId;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@orderId", orderId);
        using var reader = cmd.ExecuteReader();

        Console.WriteLine($"\n Детали заказа №{orderId}:");
        Console.WriteLine("Товар / Количество / Цена за ед. / Сумма");
        decimal totalOrder = 0;
        while (reader.Read())
        {
            string name = reader["name"].ToString();
            int qty = reader.GetInt32(1);
            decimal price = reader.GetDecimal(2);
            decimal sum = reader.GetDecimal(3);
            Console.WriteLine($"{name} | {qty} | {price:F2} | {sum:F2}");
            totalOrder += sum;
        }
        Console.WriteLine($"Общая сумма: {totalOrder:F2}");
    }

    static void CreateOrder()
    {
        ShowUsers();
        Console.Write("ID пользователя: ");
        if (!int.TryParse(Console.ReadLine(), out int userId)) { Console.WriteLine("Неверный ID."); return; }

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string insertOrder = "INSERT INTO orders (user_id, status) VALUES (@userId, 'новый') RETURNING id;";
        using var cmdOrder = new NpgsqlCommand(insertOrder, conn);
        cmdOrder.Parameters.AddWithValue("@userId", userId);
        int newOrderId = (int)cmdOrder.ExecuteScalar();
        Console.WriteLine($"Создан заказ №{newOrderId}");

        bool addMore = true;
        while (addMore)
        {
            ShowProducts();
            Console.Write("ID товара (0 - закончить): ");
            if (!int.TryParse(Console.ReadLine(), out int productId) || productId == 0) break;

            Console.Write("Количество: ");
            if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("Некорректное количество.");
                continue;
            }

            decimal price = GetProductPrice(productId);
            if (price == 0) { Console.WriteLine("Товар не найден."); continue; }

            string insertItem = "INSERT INTO order_items (order_id, product_id, quantity, price_at_time) VALUES (@oid, @pid, @qty, @price);";
            using var itemCmd = new NpgsqlCommand(insertItem, conn);
            itemCmd.Parameters.AddWithValue("@oid", newOrderId);
            itemCmd.Parameters.AddWithValue("@pid", productId);
            itemCmd.Parameters.AddWithValue("@qty", quantity);
            itemCmd.Parameters.AddWithValue("@price", price);
            itemCmd.ExecuteNonQuery();
            Console.WriteLine("Товар добавлен.");

            Console.Write("Добавить ещё? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y") addMore = false;
        }
        Console.WriteLine("Заказ сформирован.");
    }

    static void AddItemToOrder()
    {
        ShowOrders();
        Console.Write("ID заказа: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Неверный ID."); return; }

        ShowProducts();
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int productId)) { Console.WriteLine("Неверный ID товара."); return; }
        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Некорректное количество.");
            return;
        }

        decimal price = GetProductPrice(productId);
        if (price == 0) { Console.WriteLine("Товар не найден."); return; }

        string sql = "INSERT INTO order_items (order_id, product_id, quantity, price_at_time) VALUES (@oid, @pid, @qty, @price);";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@oid", orderId);
            cmd.Parameters.AddWithValue("@pid", productId);
            cmd.Parameters.AddWithValue("@qty", quantity);
            cmd.Parameters.AddWithValue("@price", price);
        });
        Console.WriteLine("Товар добавлен в заказ.");
    }

    static void UpdateOrderStatus()
    {
        ShowOrders();
        Console.Write("ID заказа: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Неверный ID."); return; }
        Console.Write("Новый статус (новый, оплачен, отправлен, завершён): ");
        string status = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(status)) { Console.WriteLine("Статус не может быть пустым."); return; }

        string sql = "UPDATE orders SET status = @status WHERE id = @id;";
        ExecuteNonQuery(sql, cmd =>
        {
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@id", orderId);
        });
        Console.WriteLine("Статус обновлён.");
    }

    static void DeleteOrder()
    {
        ShowOrders();
        Console.Write("ID заказа для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Неверный ID."); return; }

        string sql = "DELETE FROM orders WHERE id = @id;";
        ExecuteNonQuery(sql, cmd => cmd.Parameters.AddWithValue("@id", orderId));
        Console.WriteLine("Заказ удалён.");
    }

    // ОТЧЁТЫ
    static void ReportMenu()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine("\n в таблице Отчёты");
            Console.WriteLine("1 - Топ-5 дорогих товаров");
            Console.WriteLine("2 - Сумма покупок по клиентам");
            Console.WriteLine("3 - Количество заказов по статусам");
            Console.WriteLine("4 - Назад");
            Console.Write("Ваш выбор(введите цифру напимер 1 и Enter): ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": TopExpensiveProducts(); break;
                case "2": SalesByCustomer(); break;
                case "3": OrdersByStatus(); break;
                case "4": back = true; break;
                default: Console.WriteLine("Неверный выбор или ввод."); break;
            }
        }
    }

    static void TopExpensiveProducts()
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = "SELECT name, price FROM products ORDER BY price DESC LIMIT 5;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\nТоп-5 дорогих товаров:");
        while (reader.Read()) Console.WriteLine($"{reader["name"]} | {reader["price"]:F2}");
    }

    static void SalesByCustomer()
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = @"
            SELECT u.name, COALESCE(SUM(oi.quantity * oi.price_at_time), 0) AS total_spent
            FROM users u
            LEFT JOIN orders o ON u.id = o.user_id
            LEFT JOIN order_items oi ON o.id = oi.order_id
            GROUP BY u.id, u.name
            ORDER BY total_spent DESC;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\n Сумма покупок по клиентам:");
        while (reader.Read()) Console.WriteLine($"{reader["name"]} | {reader["total_spent"]:F2}");
    }

    static void OrdersByStatus()
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        string sql = "SELECT status, COUNT(*) FROM orders GROUP BY status ORDER BY status;";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        Console.WriteLine("\n Количество заказов по статусам:");
        while (reader.Read()) Console.WriteLine($"{reader["status"]} | {reader["count"]}");
    }
}
