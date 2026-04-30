# 🎬 CineGo

Aplicación web de películas desarrollada con ASP.NET Core MVC que permite a los usuarios registrarse, explorar contenido y compartir reseñas. Además, incorpora funcionalidades de inteligencia artificial para generar resúmenes y spoilers automáticamente.

---

## 📌 Descripción

**CineGo** es una plataforma pensada para centralizar información sobre películas y facilitar la interacción entre usuarios.

Los usuarios pueden:
- Registrarse e iniciar sesión
- Explorar películas disponibles
- Ver detalles (género, plataforma, descripción, etc.)
- Publicar reseñas y opiniones
- Obtener resúmenes automáticos generados con IA
- Generar spoilers mediante IA

La aplicación también cuenta con un **panel de administración**, desde donde se pueden gestionar:
- Películas
- Géneros
- Plataformas
- Reseñas

---

## 🚀 Funcionalidades principales

- 🔐 Autenticación y autorización de usuarios (registro/login)
- 👥 Sistema de roles (usuarios y administradores)
- 🎬 Catálogo de películas
- 📝 Sistema de reseñas
- 🤖 Generación de resúmenes con IA
- ⚠️ Generación de spoilers con IA
- 🛠️ Panel de administración (CRUD completo)

---

## 🧠 Inteligencia Artificial

La aplicación integra la API de OpenAI para:

- Generar resúmenes breves de películas
- Crear spoilers automáticos

---

## 🛠️ Tecnologías utilizadas

### Backend
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQL Server

### Seguridad
- ASP.NET Core Identity
  - Registro de usuarios
  - Inicio de sesión
  - Manejo de roles

### Base de datos
- SQL Server
- Code First con Entity Framework

### IA
- OpenAI API
  - Generación de texto (resúmenes y spoilers)
