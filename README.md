# 🛒 Simple E-Commerce MVC Project

A basic ASP.NET Core 8.0 MVC application using **Entity Framework Core 8.0**, **FluentValidation 12**, and **BCrypt.Net** for secure password handling.

---

## 🚀 Features

### 👨‍💼 Admin
- Add, edit, view, and delete products.

### 👤 User
- **Without Login**
  - Can view products
  - Can add products to cart
- **With Login**
  - Can checkout cart
  - Can comment on products

---

## 🧩 Project Layers

- **Models** → Represent data (User, Product, Order).
- **Interfaces** → Define contracts for services.
- **Services** → Implement business logic (CRUD, authentication).
- **Controllers** → Handle HTTP requests and responses.
- **FluentValidation** → Validate models before persistence.

---

## 🛠️ Tech Stack

- ASP.NET Core MVC (.NET 8.0)
- Entity Framework Core 8.0
- FluentValidation 12
- BCrypt.Net-Next 4.0.3
- SQL Server / SQLite

---

