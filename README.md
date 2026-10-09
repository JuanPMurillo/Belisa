# 📦 Belisa — Sistema de Punto de Venta

**Belisa** es una aplicación de escritorio desarrollada en C# y .NET, diseñada para apoyar la administración de un negocio mediante la gestión de inventario, compras y proveedores.

El proyecto nació de una necesidad real: desarrollar una herramienta adaptada a las operaciones de un negocio. Su desarrollo me permitió aplicar conocimientos de programación orientada a objetos, bases de datos relacionales, arquitectura en capas y desarrollo de interfaces de escritorio.

## ✨ Características principales

- 📦 **Gestión de inventario:** organización y administración de los productos del negocio.
- 🛒 **Gestión de compras:** funcionalidades para administrar las operaciones de compra.
- 🤝 **Gestión de proveedores:** registro y administración de la información de los proveedores.
- 🗄️ **Persistencia de datos:** integración con una base de datos SQL Server.
- 📊 **Manejo de archivos Excel:** soporte para la generación, lectura y modificación de archivos `.xlsx` mediante ClosedXML.
- 🧩 **Arquitectura modular:** separación de responsabilidades para facilitar el mantenimiento y la evolución del sistema.

> Las funcionalidades disponibles corresponden a la implementación actual del proyecto.

## 🛠️ Tecnologías utilizadas

| Tecnología | Propósito |
|---|---|
| C# | Lenguaje principal de desarrollo |
| .NET | Plataforma de desarrollo de la aplicación |
| Windows Forms | Interfaz gráfica de escritorio |
| SQL Server | Gestión y almacenamiento de datos |
| ADO.NET / SQL | Acceso y comunicación con la base de datos |
| ClosedXML | Manipulación de archivos Excel (`.xlsx`) |
| Visual Studio | Entorno de desarrollo |
| Git y GitHub | Control de versiones y alojamiento del código |

## 🏗️ Arquitectura del proyecto

Belisa utiliza una arquitectura en capas que separa la interfaz de usuario, las entidades, las reglas de negocio y el acceso a datos. Esta organización ayuda a mantener el código estructurado y facilita la modificación de sus componentes.

```text
Belisa/
├── Belisa/             # Presentación e interfaz de usuario
├── BelisaEntidad/      # Entidades y modelos de datos
├── BelisaNegocio/      # Lógica y reglas de negocio
├── BelisaDatos/        # Acceso a datos y consultas SQL
└── Belisa.slnx         # Solución de Visual Studio
```

### Responsabilidades de cada capa

- **Belisa — Presentación:** contiene los formularios y los elementos de la interfaz con los que interactúa el usuario.
- **BelisaEntidad — Entidades:** define las clases que representan los datos utilizados por el sistema.
- **BelisaNegocio — Lógica de negocio:** centraliza las operaciones y validaciones necesarias para procesar la información.
- **BelisaDatos — Acceso a datos:** gestiona las operaciones de comunicación con la base de datos, incluidas las consultas y las operaciones CRUD.

Esta separación permite distribuir las responsabilidades entre los módulos y reducir el acoplamiento entre la interfaz, la lógica de negocio y la persistencia de datos.

## ⚙️ Requisitos previos

Para compilar y ejecutar el proyecto, necesitarás:

- **Sistema operativo:** Windows.
- **IDE:** [Visual Studio](https://visualstudio.microsoft.com/).
- **Componentes de desarrollo:** la carga de trabajo de desarrollo de escritorio de .NET y el SDK o entorno de ejecución correspondiente al framework objetivo del proyecto.
- **Base de datos:** Microsoft SQL Server, con una instancia accesible y la base de datos requerida por la aplicación.
- **Dependencias:** paquetes NuGet utilizados por la solución, incluido ClosedXML.

> Consulta la configuración de los proyectos para identificar la versión exacta de .NET y los componentes necesarios para tu entorno.

## 🚀 Instalación y configuración

### 1. Clonar el repositorio

Abre una terminal y ejecuta:

```bash
git clone https://github.com/JuanPMurillo/Belisa.git
cd Belisa
```

### 2. Abrir el proyecto

Abre `Belisa.slnx` desde Visual Studio. Si utilizas una versión o configuración que no admita ese formato, verifica si el repositorio incluye otra solución compatible.

### 3. Configurar SQL Server

Asegúrate de que SQL Server esté instalado y funcionando. Configura la base de datos requerida por la aplicación y verifica que su estructura coincida con las consultas y operaciones implementadas en el proyecto.

Si la aplicación utiliza `App.config` para almacenar la cadena de conexión, actualiza la configuración correspondiente con los datos de tu instancia.

Ejemplo ilustrativo:

```xml
<connectionStrings>
    <add name="CadenaConexion"
         connectionString="Data Source=TU_SERVIDOR;Initial Catalog=BelisaDB;Integrated Security=True;"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

**Importante:** `TU_SERVIDOR` y `BelisaDB` son valores de ejemplo. Sustitúyelos por el nombre de tu instancia y el nombre real de tu base de datos. La cadena debe coincidir con el nombre y el proveedor que espera el código de la aplicación.

### 4. Restaurar dependencias

Visual Studio puede restaurar los paquetes NuGet automáticamente al abrir o compilar la solución. Si el proyecto admite la restauración mediante CLI, puedes ejecutar:

```bash
dotnet restore
```

### 5. Compilar y ejecutar

1. Selecciona el proyecto `Belisa` como proyecto de inicio.
2. Compila la solución y revisa si hay errores de dependencias o configuración.
3. Ejecuta la aplicación desde Visual Studio.

La ejecución correcta depende de que el entorno de .NET, la base de datos y la cadena de conexión estén configurados adecuadamente.

## 📚 Dependencias destacadas

- [**ClosedXML**](https://github.com/ClosedXML/ClosedXML): biblioteca para crear, leer y modificar documentos de Excel en formato `.xlsx` desde .NET, sin requerir Microsoft Excel instalado para las operaciones habituales de la biblioteca.

Consulta los archivos de proyecto y la configuración de NuGet para conocer las versiones exactas de las dependencias.

## 🎯 Objetivo del proyecto

Belisa representa la aplicación práctica de conocimientos de ingeniería de software para resolver una necesidad concreta.

Durante su desarrollo trabajé en la organización de una aplicación de escritorio, la separación de responsabilidades mediante capas, la integración con SQL Server y la implementación de funcionalidades orientadas a la administración de un negocio.

El proyecto también refleja mi interés por crear software útil, mantenible y adaptado a las necesidades de sus usuarios.

## 📌 Estado del proyecto

Belisa cuenta con una versión funcional que sirve como base para su uso en el negocio familiar. Las posibles mejoras y funcionalidades futuras dependerán de las necesidades que se identifiquen durante su utilización.

## 👨‍💻 Autor

**Juan Pablo Murillo**  
Ingeniero en Sistemas Computacionales

- GitHub: [@JuanPMurillo](https://github.com/JuanPMurillo)
- Repositorio: [JuanPMurillo/Belisa](https://github.com/JuanPMurillo/Belisa)

---

Desarrollado con C#, .NET y SQL Server para resolver una necesidad real mediante software.
