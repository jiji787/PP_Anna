-- Создание роли для приложения (ограниченные права)
CREATE ROLE app_user WITH LOGIN PASSWORD 'app_password';
GRANT CONNECT ON DATABASE "Золотце" TO app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user;

-- Создание роли для администратора (полные права)
CREATE ROLE admin_user WITH LOGIN PASSWORD 'admin_password' SUPERUSER;

-- Создание роли для резервного копирования (только чтение)
CREATE ROLE backup_user WITH LOGIN PASSWORD 'backup_password';
GRANT CONNECT ON DATABASE "Золотце" TO backup_user;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO backup_user;