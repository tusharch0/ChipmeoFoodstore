-- ==========================================================
-- 1. DATABASE SETUP
-- ==========================================================
-- Create database separately before running this script:
--   CREATE DATABASE pos_shop;
-- \c pos_shop;

-- ==========================================================
-- 2. CATEGORY / MENU / ADDONS
-- ==========================================================

CREATE TABLE categories (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    image_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE menu_items (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    category_id INT,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    price DECIMAL(10,0) NOT NULL,
    image_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE SET NULL
);

CREATE TABLE addons (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10,0) NOT NULL DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- ==========================================================
-- 3. MENU ITEM ↔ ADDONS
-- ==========================================================

CREATE TABLE menu_item_addons (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    menu_item_id INT NOT NULL,
    addon_id INT NOT NULL,
    price_override DECIMAL(10,0) NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (menu_item_id) REFERENCES menu_items(id) ON DELETE CASCADE,
    FOREIGN KEY (addon_id) REFERENCES addons(id) ON DELETE CASCADE,
    CONSTRAINT UQ_menu_item_addon UNIQUE (menu_item_id, addon_id)
);

-- ==========================================================
-- 4. COMBO
-- ==========================================================

CREATE TABLE combos (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    combo_price DECIMAL(10,0) NOT NULL,
    description VARCHAR(500),
    image_url VARCHAR(500),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE combo_items (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    combo_id INT NOT NULL,
    menu_item_id INT NOT NULL,
    quantity INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (combo_id) REFERENCES combos(id) ON DELETE CASCADE,
    FOREIGN KEY (menu_item_id) REFERENCES menu_items(id) ON DELETE CASCADE,
    CONSTRAINT UQ_combo_item UNIQUE (combo_id, menu_item_id)
);

-- ==========================================================
-- 5. DISCOUNTS
-- ==========================================================

CREATE TABLE discounts (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(10) NOT NULL CHECK (type IN ('percent', 'amount')),
    value DECIMAL(10,2) NOT NULL,
    max_discount_amount DECIMAL(10,0),
    min_order_amount DECIMAL(10,0) DEFAULT 0,
    usage_limit INT,
    used_count INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW()
);

-- ==========================================================
-- 6. TABLES
-- ==========================================================

CREATE TABLE sources (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(50) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

-- ==========================================================
-- 7. RBAC
-- ==========================================================

CREATE TABLE roles (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    default_route VARCHAR(100) DEFAULT '/admin',
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE permissions (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    module VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE role_permissions (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE,
    CONSTRAINT UQ_role_permission UNIQUE (role_id, permission_id)
);

-- ==========================================================
-- 8. EMPLOYEES
-- ==========================================================

CREATE TABLE employees (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    full_name VARCHAR(100) NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100),
    phone VARCHAR(20),
    avatar_url VARCHAR(500),
    role_id INT NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (role_id) REFERENCES roles(id)
);

-- ==========================================================
-- 14. CUSTOMERS
-- ==========================================================

CREATE TABLE customers (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    full_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    email VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    avatar_url VARCHAR(500),
    points INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT UQ_customers_phone UNIQUE (phone),
    CONSTRAINT UQ_customers_email UNIQUE (email)
);

-- ==========================================================
-- 9. ORDERS
-- ==========================================================

CREATE TABLE orders (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_code VARCHAR(20) NOT NULL UNIQUE,
    source_id INT,
    employee_id INT,
    customer_id INT,
    discount_id INT,
    subtotal_amount DECIMAL(10,0) DEFAULT 0,
    discount_amount DECIMAL(10,0) DEFAULT 0,
    vat_amount DECIMAL(10,0) DEFAULT 0,
    total_amount DECIMAL(10,0) DEFAULT 0,
    qr_payment_url TEXT,
    paid_at TIMESTAMP,
    status VARCHAR(20) DEFAULT 'pending'
        CHECK (status IN ('pending', 'confirmed', 'preparing', 'ready', 'served', 'paid', 'cancelled')),
    note VARCHAR(500),
    printed_at TIMESTAMP,
    updated_by INT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (source_id) REFERENCES sources(id),
    FOREIGN KEY (employee_id) REFERENCES employees(id),
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (discount_id) REFERENCES discounts(id),
    FOREIGN KEY (updated_by) REFERENCES employees(id)
);

-- ==========================================================
-- 10. ORDER ITEMS
-- ==========================================================

CREATE TABLE order_items (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_id INT NOT NULL,
    menu_item_id INT,
    combo_id INT,
    menu_item_name VARCHAR(255) NOT NULL,
    unit_price DECIMAL(10,0) NOT NULL,
    quantity INT DEFAULT 1,
    total_price DECIMAL(10,0) NOT NULL,
    note VARCHAR(255),
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (menu_item_id) REFERENCES menu_items(id) ON DELETE SET NULL,
    FOREIGN KEY (combo_id) REFERENCES combos(id) ON DELETE SET NULL
);

-- ==========================================================
-- 11. ORDER STATUS HISTORY
-- ==========================================================

CREATE TABLE order_status_history (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_id INT NOT NULL,
    from_status VARCHAR(20),
    to_status VARCHAR(20) NOT NULL,
    changed_by INT,
    changed_at TIMESTAMP DEFAULT NOW(),
    note VARCHAR(500),
    FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    FOREIGN KEY (changed_by) REFERENCES employees(id)
);

CREATE TABLE order_item_addons (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_item_id INT NOT NULL,
    addon_id INT NOT NULL,
    addon_name VARCHAR(100) NOT NULL,
    quantity INT DEFAULT 1,
    unit_price DECIMAL(10,0) NOT NULL,
    total_price DECIMAL(10,0) NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (order_item_id) REFERENCES order_items(id) ON DELETE CASCADE,
    FOREIGN KEY (addon_id) REFERENCES addons(id)
);

-- ==========================================================
-- 12. PAYMENTS
-- ==========================================================

CREATE TABLE payments (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_id INT NOT NULL,
    amount DECIMAL(10,0) NOT NULL,
    method VARCHAR(20) NOT NULL CHECK (method IN ('cash', 'qr', 'zalopay', 'momo', 'card')),
    reference_code VARCHAR(100),
    status VARCHAR(20) DEFAULT 'success'
        CHECK (status IN ('pending', 'success', 'failed', 'refunded')),
    paid_at TIMESTAMP DEFAULT NOW(),
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (order_id) REFERENCES orders(id)
);

-- ==========================================================
-- 13. PAYMENT SETTINGS
-- ==========================================================

CREATE TABLE payment_settings (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    bank_id VARCHAR(50) NOT NULL,
    bank_account VARCHAR(50) NOT NULL,
    bank_name VARCHAR(100) NOT NULL,
    bank_account_name VARCHAR(200),
    template VARCHAR(20) DEFAULT 'compact2',
    is_active BOOLEAN DEFAULT TRUE,
    is_default BOOLEAN DEFAULT FALSE
);

CREATE UNIQUE INDEX UQ_payment_settings_default
ON payment_settings(is_default)
WHERE is_default = TRUE;

-- ==========================================================
-- 15. BLOG
-- ==========================================================

CREATE TABLE blog_posts (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    title VARCHAR(255) NOT NULL,
    slug VARCHAR(255) NOT NULL UNIQUE,
    excerpt VARCHAR(500),
    content TEXT,
    thumbnail_url VARCHAR(500),
    status VARCHAR(20) DEFAULT 'draft' CHECK (status IN ('draft', 'published', 'archived')),
    author_id INT,
    published_at TIMESTAMP,
    meta_title VARCHAR(100),
    meta_description VARCHAR(200),
    focus_keyword VARCHAR(100),
    keywords VARCHAR(500),
    canonical_url VARCHAR(500),
    og_image_url VARCHAR(500),
    reading_time INT DEFAULT 0,
    word_count INT DEFAULT 0,
    seo_score INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (author_id) REFERENCES employees(id) ON DELETE SET NULL
);

-- ==========================================================
-- 15B. BLOG TAGS
-- ==========================================================

CREATE TABLE tags (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    slug VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255),
    color VARCHAR(7) DEFAULT '#f59e0b',
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE blog_post_tags (
    post_id INT NOT NULL,
    tag_id INT NOT NULL,
    PRIMARY KEY (post_id, tag_id),
    FOREIGN KEY (post_id) REFERENCES blog_posts(id) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE
);

-- ==========================================================
-- 16. MEDIA (IMAGES/FILES)
-- ==========================================================

CREATE TABLE media (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    file_name VARCHAR(255) NOT NULL,
    folder VARCHAR(100) NOT NULL DEFAULT 'misc',
    file_url VARCHAR(500) NOT NULL,
    file_type VARCHAR(50) NOT NULL,
    file_size BIGINT,
    alt_text VARCHAR(255),
    uploaded_by_employee INT,
    uploaded_by_customer INT,
    entity_type VARCHAR(50),
    entity_id INT,
    created_at TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (uploaded_by_employee) REFERENCES employees(id) ON DELETE SET NULL,
    FOREIGN KEY (uploaded_by_customer) REFERENCES customers(id) ON DELETE SET NULL
);

CREATE INDEX IX_media_folder ON media(folder);
CREATE INDEX IX_media_entity ON media(entity_type, entity_id);

-- ==========================================================
-- 17. INDEXES
-- ==========================================================

CREATE INDEX IX_orders_status ON orders(status);
CREATE INDEX IX_orders_created_at ON orders(created_at);
CREATE INDEX IX_orders_source_id ON orders(source_id);
CREATE INDEX IX_orders_employee_id ON orders(employee_id);

CREATE INDEX IX_order_items_order_id ON order_items(order_id);
CREATE INDEX IX_order_items_menu_item_id ON order_items(menu_item_id);

CREATE INDEX IX_menu_items_category_id ON menu_items(category_id);

CREATE INDEX IX_employees_username ON employees(username);
CREATE INDEX IX_employees_role_id ON employees(role_id);

CREATE INDEX IX_discounts_code ON discounts(code);
CREATE INDEX IX_discounts_is_active ON discounts(is_active);

-- Composite Indexes for Performance
CREATE INDEX IX_orders_status_created_at ON orders(status, created_at);
CREATE INDEX IX_orders_source_status ON orders(source_id, status);
CREATE INDEX IX_order_items_order_menu ON order_items(order_id, menu_item_id);
CREATE INDEX IX_menu_items_category_active ON menu_items(category_id, is_active);
CREATE INDEX IX_order_status_history_order ON order_status_history(order_id, changed_at);
CREATE INDEX IX_employees_role_active ON employees(role_id, is_active);
CREATE INDEX IX_payments_order_status ON payments(order_id, status);

-- Partial Indexes
CREATE INDEX IX_orders_active ON orders(id) WHERE status IN ('pending', 'preparing', 'paid');
CREATE INDEX IX_payments_success ON payments(order_id) WHERE status = 'success';
CREATE INDEX IX_menu_items_active_price ON menu_items(price) WHERE is_active = TRUE;

-- Blog index
CREATE INDEX IX_blog_posts_slug ON blog_posts(slug);
CREATE INDEX IX_blog_posts_status ON blog_posts(status);
CREATE INDEX IX_customers_phone ON customers(phone);
CREATE INDEX IX_customers_email ON customers(email);
CREATE INDEX IX_tags_slug ON tags(slug);
CREATE INDEX IX_blog_post_tags_tag ON blog_post_tags(tag_id);


