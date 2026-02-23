# Angular Generator

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0) [![Angular](https://img.shields.io/badge/Angular-v17+-dd0031?style=flat-square&logo=angular)](https://angular.io/) 
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-38B2AC?style=flat-square&logo=tailwind-css&logoColor=white)](https://tailwindcss.com/) [![Bootstrap](https://img.shields.io/badge/Bootstrap-563D7C?style=flat-square&logo=bootstrap&logoColor=white)](https://getbootstrap.com/) [![Angular Material](https://img.shields.io/badge/Angular_Material-E91E63?style=flat-square&logo=angular&logoColor=white)](https://material.angular.io/) [![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat-square&logo=css3&logoColor=white)](https://developer.mozilla.org/en-US/docs/Web/CSS)
<<<<<<< HEAD
=======

>>>>>>> 0e18b5ab6a2a08082bb30f402de44e3be415a0fe
A high-performance, professional low-code productivity tool designed to automate the creation of Angular CRUD components. Generate complete frontend modules including **Services, Interfaces, HTML, and TypeScript** in seconds from any data source.
---

## ✨ Key Features

- **🎯 Multi-Source Generation**:
  - **Relational Databases**: SQL Server, MySQL, PostgreSQL.
  - **Legacy Systems**: Specialized support for **AS/400 (IBM i)**.
  - **Modern APIs**: Parse REST API structures via URL.
  - **JSON Raw Data**: Generate code directly from JSON samples.
- **🎨 UI Strategy Pattern**: Supports multiple CSS frameworks:
  - **Bootstrap 5** (High-speed, responsive)
  - **Angular Material** (Premium, standard look)
- **⚡ Offline Ready**: All libraries (Bootstrap, Font Awesome, Prism.js) are localized—no CDN dependencies.
- **👁️ Real-time Preview**: Syntax-highlighted code preview with Prism.js.
- **📦 ZIP Export**: Download your entire generated module as a ready-to-use ZIP package.

---

## 📠 Exclusive IBM AS/400 Support

This generator provides deep integration with **IBM i (AS/400)** legacy systems:
- **Library/Table Separation**: Precise schema discovery by targeting specific Libraries and Tables.
- **Auto-Labeling**: Automatically fetches `COLUMN_TEXT` labels from AS/400 to use as UI field names.
- **Dapper Integration**: Optimized high-speed schema reading using Dapper and OleDb.

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 8.0 (C#)
- **Data Access**: Dapper (High-performance micro-ORM)
- **Frontend**: Vanilla JS (AJAX-driven), CSS3, HTML5
- **Libraries**:
  - **Prism.js**: Code syntax highlighting
  - **JSZip**: In-browser ZIP generation
  - **Font Awesome**: Modern vector icons
  - **Bootstrap/Material**: Modern UI layouts

---

## 🚀 Getting Started

### Prerequisites
- **.NET 8 SDK** installed.
- **Windows OS** (required for AS/400 OLE DB connectivity).
- **IBM i Access Client Solutions (ACS)** (only if connecting to AS/400).

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/lutfee-dl/AngularGenerator
   ```
2. Navigate to the project directory:
   ```bash
   cd AngularGenerator
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
4. Open your browser and navigate to `http://localhost:5200` (or the port specified in your console).

---

## 📖 Usage

1. **Connect**: Click the Database Status icon to configure your connection.
2. **Select Source**: Choose between SQL, API, or JSON.
3. **Configure**: Select your table or paste your API/JSON data.
4. **Customization**:
   - Select CRUD operations (Get, Post, Put, Delete).
   - Select the CSS framework (Bootstrap/Material).
   - Toggle individual fields to include/exclude.
5. **Generate**: Click **Generate Code** and instantly preview your Angular modules.
6. **Download**: Use **Download All (ZIP)** to grab the files.

---

## 📁 Project Structure

```text
├── Controllers/         # API & Routing logic
├── Services/            # Core code-generation & Schema logic
│   ├── Builders/        # Code construction engines (HTML/TS/Service)
│   ├── Strategies/      # CSS Framework rendering patterns
├── Models/              # Data structures & Transfer objects
├── Views/               # Single-page Generator UI
└── wwwroot/             # Localized static assets (Offline-ready)
```

---

*</> Developed with Lutfee*
