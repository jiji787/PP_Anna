using Npgsql; //библиотека для работы с PostgreSQL

class Program
{
    static string connectionString = "Host=localhost;Port=5432;Database=Золотце;Username=postgres;Password=postgres890";
    // Главный метод, с которого начинается программа
    static void Main(string[] args)
    {
        // Устанавливаем кодировку для вывода русских букв в консоль
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=+++= Подключение к БД Золотце +===+\n");

        bool exit = false;  // Переменная для выхода из цикла

        // Бесконечный цикл, пока exit не станет true
        while (!exit)
        {
            // Выводим меню
            Console.WriteLine("\n Выберите действие:");
            Console.WriteLine("1 - Показать всех пользователей");
            Console.WriteLine("2 - Добавить нового пользователя");
            Console.WriteLine("3 - Выход");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();  // Считываем выбор пользователя

            // Обрабатываем выбор
            switch (choice)
            {
                case "1":
                    ShowUsers();   // Вызов метода показа пользователей
                    break;
                case "2":
                    AddUser();     // Вызов метода добавления пользователя
                    break;
                case "3":
                    exit = true;   // Выход из программы
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    // Метод для вывода всех пользователей из таблицы users
    static void ShowUsers()
    {
        try
        {
            // Создаём подключение к базе данных
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();  // Открываем соединение

            // SQL-запрос на выборку всех пользователей, отсортированных по id
            string sql = "SELECT id, name, age, city, email FROM users ORDER BY id;";

            // Создаём команду с запросом и подключением
            using var cmd = new NpgsqlCommand(sql, conn);

            // Выполняем запрос и получаем результат
            using var reader = cmd.ExecuteReader();

            // Выводим заголовок таблицы
            Console.WriteLine("\n Список пользователей:");
            Console.WriteLine("ID | Имя | Возраст | Город | Email");
            Console.WriteLine("+===+");

            // Читаем все строки результата
            while (reader.Read())
            {
                // reader["имя_колонки"] - получаем значение колонки для текущей строки
                Console.WriteLine($"{reader["id"]} | {reader["name"]} | {reader["age"]} | {reader["city"]} | {reader["email"]}");
            }
        }
        catch (Exception ex)
        {
            // Если произошла ошибка (например, неверный пароль или БД не запущена)
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    /// Метод для добавления нового пользователя в таблицу users
    static void AddUser()
    {
        try
        {
            // Запрашиваем данные у пользователя
            Console.Write("Введите имя: ");
            string name = Console.ReadLine();

            Console.Write("Введите возраст: ");
            int age = int.Parse(Console.ReadLine());  // Преобразуем строку в число

            Console.Write("Введите город: ");
            string city = Console.ReadLine();

            Console.Write("Введите email: ");
            string email = Console.ReadLine();

            // Подключаемся к БД
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            // SQL-запрос на вставку новой строки (параметры передаются отдельно для безопасности)
            string sql = "INSERT INTO users (name, age, city, email) VALUES (@name, @age, @city, @email);";

            using var cmd = new NpgsqlCommand(sql, conn);

            // Передаём значения параметров (защита от SQL-инъекций)
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@age", age);
            cmd.Parameters.AddWithValue("@city", city);
            cmd.Parameters.AddWithValue("@email", email);

            // Выполняем запрос (возвращает количество затронутых строк)
            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
                Console.WriteLine("Пользователь успешно добавлен!");
            else
                Console.WriteLine("Ошибка при добавлении");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}