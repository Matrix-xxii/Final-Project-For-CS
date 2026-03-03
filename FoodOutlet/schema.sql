-- ===============================
-- ROLES
-- ===============================
CREATE TABLE roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL
);

-- ===============================
-- REGISTRATIONS
-- ===============================
CREATE TABLE registrations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    registration_name VARCHAR(150) NOT NULL,
    birth_of_date DATE,
    email VARCHAR(150) UNIQUE,
    phone_no VARCHAR(20), -- ✅ Added phone number field
    password_hash VARCHAR(255),
    address TEXT,
    role_id INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES roles(id)
);

-- ===============================
-- RESIGNS
-- ===============================
CREATE TABLE resigns (
    id INT AUTO_INCREMENT PRIMARY KEY,
    assignment_name VARCHAR(150),
    registration_id INT,
    reason TEXT,
    resign_at DATETIME,
    FOREIGN KEY (registration_id) REFERENCES registrations(id)
);

-- ===============================
-- PAYMENT METHODS
-- ===============================
CREATE TABLE payment_methods (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_name VARCHAR(100),
    note TEXT,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===============================
-- PAYMENTS
-- ===============================
CREATE TABLE payments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_method_id INT,
    payment_status VARCHAR(50),
    amount DECIMAL(10,2),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id)
);

-- ===============================
-- TABLE LISTS
-- ===============================
CREATE TABLE table_lists (
    id INT AUTO_INCREMENT PRIMARY KEY,
    table_name VARCHAR(100),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===============================
-- CATEGORIES
-- ===============================
CREATE TABLE categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(100),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ===============================
-- RECIPES
-- ===============================
CREATE TABLE recipes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    recipe_name VARCHAR(150),
    category_id INT,
    price DECIMAL(10,2),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES categories(id)
);

-- ===============================
-- INVENTORIES
-- ===============================
CREATE TABLE inventories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stock_qty INT,
    recipe_id INT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (recipe_id) REFERENCES recipes(id)
);

-- ===============================
-- ORDERS
-- ===============================
CREATE TABLE orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_id INT,
    table_id INT,
    recipe_id INT,
    order_status VARCHAR(50),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (payment_id) REFERENCES payments(id),
    FOREIGN KEY (table_id) REFERENCES table_lists(id),
    FOREIGN KEY (recipe_id) REFERENCES recipes(id)
);