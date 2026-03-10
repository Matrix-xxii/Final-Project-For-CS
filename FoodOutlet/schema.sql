CREATE DATABASE food_outlet;
USE food_outlet;

-- Roles
CREATE TABLE Roles (
    id INT AUTO_INCREMENT PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL
);

-- Registrations
CREATE TABLE Registrations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    registration_name VARCHAR(150),
    birth_of_date DATE,
    email VARCHAR(150),
    password_hash VARCHAR(255),
    address TEXT,
    phone_no VARCHAR(20),
    role_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (role_id) REFERENCES Roles(id)
);

-- Resigns
CREATE TABLE Resigns (
    id INT AUTO_INCREMENT PRIMARY KEY,
    assignment_name VARCHAR(150),
    registration_id INT,
    reason TEXT,
    resign_at DATETIME,
    FOREIGN KEY (registration_id) REFERENCES Registrations(id)
);

-- Payment Methods
CREATE TABLE Payment_Methods (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_name VARCHAR(100),
    note TEXT,
    is_deleted BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Payments
CREATE TABLE Payments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_method_id INT,
    payment_status VARCHAR(50),
    amount DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (payment_method_id) REFERENCES Payment_Methods(id)
);

-- Table Lists
CREATE TABLE Table_Lists (
    id INT AUTO_INCREMENT PRIMARY KEY,
    table_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Categories
CREATE TABLE Categories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    category_name VARCHAR(150),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Recipes
CREATE TABLE Recipes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    recipe_name VARCHAR(150),
    category_id INT,
    recipe_img VARCHAR(255),
    description TEXT,
    price DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES Categories(id)
);

-- Inventories
CREATE TABLE Inventories (
    id INT AUTO_INCREMENT PRIMARY KEY,
    stock_qty INT,
    receipe_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (receipe_id) REFERENCES Recipes(id)
);

-- Orders
CREATE TABLE `Order` (
    id INT AUTO_INCREMENT PRIMARY KEY,
    payment_id INT,
    table_id INT,
    receipe_id INT,
    order_status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (payment_id) REFERENCES Payments(id),
    FOREIGN KEY (table_id) REFERENCES Table_Lists(id),
    FOREIGN KEY (receipe_id) REFERENCES Recipes(id)
);