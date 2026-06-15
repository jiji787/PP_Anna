CREATE DATABASE Золотце;
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    age INTEGER,
    city VARCHAR(100),
    email VARCHAR(100) UNIQUE
);

CREATE TABLE  products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    description TEXT
);

CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'новый'
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    price_at_time DECIMAL(10,2) NOT NULL
);
-- 3. Заполнение таблиц
INSERT INTO users (name, age, city, email) VALUES
    ('Яна', 20, 'Москва', 'yana@example.com'),
    ('Иван', 25, 'Санкт-Петербург', 'ivan@example.com'),
    ('Мария', 22, 'Казань', 'maria@example.com'),
    ('Ольга', 28, 'Екатеринбург', 'olga@example.com'),
    ('Дмитрий', 35, 'Москва', 'dmitry@example.com'),
    ('Елена', 30, 'Новосибирск', 'elena@example.com'),
    ('Сергей', 27, 'Ростов-на-Дону', 'sergey@example.com'),
    ('Татьяна', 33, 'Нижний Новгород', 'tatiana@example.com'),
    ('Алексей', 29, 'Челябинск', 'alexey@example.com'),
    ('Наталья', 24, 'Самара', 'natalia@example.com');

INSERT INTO products (name, price, description) VALUES
    ('Ноутбук', 55000.00, 'Мощный игровой ноутбук'),
    ('Мышь', 1500.00, 'Беспроводная мышь'),
    ('Клавиатура', 3000.00, 'Механическая клавиатура'),
    ('Монитор', 20000.00, '27-дюймовый 4K монитор'),
    ('Наушники', 4500.00, 'Игровые наушники с микрофоном'),
    ('Веб-камера', 3500.00, 'Full HD веб-камера'),
    ('Внешний SSD', 8000.00, '1TB внешний твердотельный накопитель'),
    ('Коврик для мыши', 800.00, 'Игровой коврик большого размера'),
    ('USB-хаб', 1200.00, '4-портовый USB 3.0 хаб'),
    ('Блок питания', 6500.00, '600W блок питания для ПК');

INSERT INTO orders (user_id, status) VALUES
    ((SELECT id FROM users WHERE name = 'Яна'), 'оплачен'),
    ((SELECT id FROM users WHERE name = 'Иван'), 'отправлен'),
    ((SELECT id FROM users WHERE name = 'Мария'), 'новый'),
    ((SELECT id FROM users WHERE name = 'Ольга'), 'оплачен'),
    ((SELECT id FROM users WHERE name = 'Дмитрий'), 'отправлен'),
    ((SELECT id FROM users WHERE name = 'Елена'), 'новый'),
    ((SELECT id FROM users WHERE name = 'Сергей'), 'оплачен'),
    ((SELECT id FROM users WHERE name = 'Татьяна'), 'отправлен'),
    ((SELECT id FROM users WHERE name = 'Алексей'), 'новый'),
    ((SELECT id FROM users WHERE name = 'Наталья'), 'оплачен');

-- Заказ №1 (Яна): ноутбук + мышь
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 
       CASE WHEN p.name = 'Ноутбук' THEN 1 ELSE 2 END,
       p.price
FROM orders o, products p
WHERE o.id = 1 AND p.name IN ('Ноутбук', 'Мышь');

-- Заказ №2 (Иван): клавиатура + наушники
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 2 AND p.name IN ('Клавиатура', 'Наушники');

-- Заказ №3 (Мария): монитор + веб-камера
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 3 AND p.name IN ('Монитор', 'Веб-камера');

-- Заказ №4 (Ольга): внешний SSD + коврик
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 
       CASE WHEN p.name = 'Внешний SSD' THEN 1 ELSE 2 END,
       p.price
FROM orders o, products p
WHERE o.id = 4 AND p.name IN ('Внешний SSD', 'Коврик для мыши');

-- Заказ №5 (Дмитрий): USB-хаб + блок питания
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 5 AND p.name IN ('USB-хаб', 'Блок питания');

-- Заказ №6 (Елена): мышь + клавиатура + наушники
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 6 AND p.name IN ('Мышь', 'Клавиатура', 'Наушники');

-- Заказ №7 (Сергей): монитор + внешний SSD
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 7 AND p.name IN ('Монитор', 'Внешний SSD');

-- Заказ №8 (Татьяна): ноутбук + коврик + веб-камера
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 
       CASE WHEN p.name = 'Ноутбук' THEN 1 ELSE 1 END,
       p.price
FROM orders o, products p
WHERE o.id = 8 AND p.name IN ('Ноутбук', 'Коврик для мыши', 'Веб-камера');

-- Заказ №9 (Алексей): блок питания + USB-хаб + мышь
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 9 AND p.name IN ('Блок питания', 'USB-хаб', 'Мышь');

-- Заказ №10 (Наталья): наушники + клавиатура
INSERT INTO order_items (order_id, product_id, quantity, price_at_time)
SELECT o.id, p.id, 1, p.price
FROM orders o, products p
WHERE o.id = 10 AND p.name IN ('Наушники', 'Клавиатура');

CREATE TABLE IF NOT EXISTS "OrderStatuses" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(50) NOT NULL
);

INSERT INTO "OrderStatuses" ("Id", "Name") VALUES
(1, 'новый'),
(2, 'оплачен'),
(3, 'отправлен'),
(4, 'завершён')
ON CONFLICT ("Id") DO NOTHING;