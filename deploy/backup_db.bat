@echo off
set PGPASSWORD=postgres890
set BACKUP_DIR=C:\backups
set DB_NAME=Золотце
set TIMESTAMP=%date:~6,4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set FILENAME=%BACKUP_DIR%\%DB_NAME%_%TIMESTAMP%.sql

if not exist %BACKUP_DIR% mkdir %BACKUP_DIR%

"C:\Program Files\PostgreSQL\18\bin\pg_dump" -h localhost -p 5432 -U postgres -F p -b -v -f "%FILENAME%" %DB_NAME%

echo Бэкап создан: %FILENAME%