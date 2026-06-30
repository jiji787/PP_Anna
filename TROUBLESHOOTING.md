# Типовые ошибки и их решения

## 1. Ошибка подключения к PostgreSQL
**Сообщение:** `Npgsql.NpgsqlException: Exception while connecting to PostgreSQL`  
**Причина:** PostgreSQL не запущен или неправильные учётные данные.  
**Решение:**  
- Запустите службу: `net start postgresql-x64-18` (или через `services.msc`).  
- Проверьте строку подключения в `Program.cs` (она должна быть: `Host=localhost;Port=5432;Database=Золотце;Username=postgres;Password=postgres890`).

## 2. Ошибка "Subscribed topic not available" (Kafka)
**Сообщение:** `Confluent.Kafka.ConsumeException: Subscribed topic not available`  
**Причина:** Топик `test-topic` ещё не создан.  
**Решение:**  
- Consumer автоматически переподписывается, подождите несколько секунд.  
- Или создайте топик вручную:  
```bash
docker exec -it kafka kafka-topics.sh --create --topic test-topic --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1
```

## 3. Ошибка "Could not resolve host: github.com" (Git)
**Причина:** Проблемы с DNS или прокси.  
**Решение:**  
- Сбросьте прокси: `git config --global --unset http.proxy`  
- Очистите DNS: `ipconfig /flushdns`

## 4. Ошибка компиляции CS0105 (дублирующийся using)
**Сообщение:** `warning CS0105: The using directive for 'PP_Anna.Api.Services' appeared previously`  
**Решение:** Удалите лишнюю строку `using PP_Anna.Api.Services;` в `Program.cs` (в проекте Web API).

## 5. Ошибка при запуске Docker (контейнер не найден)
**Причина:** Образ не скачался или имя неверное.  
**Решение:** Используйте проверенные образы, например:  
- Redis: `redis/redis-stack:latest`  
- Vault: `hashicorp/vault:latest`  
- Kafka: `wurstmeister/kafka:latest` (с Zookeeper)

## 6. Ошибка внешнего ключа при создании заказа
**Сообщение:** `23503: INSERT или UPDATE в таблице "orders" нарушает ограничение внешнего ключа "orders_user_id_fkey"`  
**Причина:** Указан несуществующий `user_id`.  
**Решение:** Проверьте, что пользователь с таким ID существует в таблице `users` (используйте пункт меню "Показать всех").

## 7. Ошибка при сборке с дублирующимися проектами
**Причина:** Наличие папок `__EfCoreDemo` и `__EfCoreDemo.Tests` или `PP_Anna.Api` мешает сборке консольного приложения.  
**Решение:** Переместите их во временную папку (например, `_old`) или удалите, если они не нужны для консольного проекта.
