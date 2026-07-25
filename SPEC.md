# RetroPad — Specification

## Vision

Windows Notepad moderno con persistencia automática y resaltado de sintaxis, estética retro consola DOS/CRT años 80.

## Design Principles

1. Velocidad
2. Simplicidad
3. Bajo consumo de recursos
4. Mantenibilidad
5. Experiencia de edición fluida

## Objectives

- Apertura instantánea (<1s)
- Interfaz minimalista
- Muy bajo consumo de memoria
- Sin extensiones, terminal, Git, IntelliSense, depuración, explorador de archivos, paneles laterales
- Manejo de archivos grandes (100 MB)

## Architecture

Arquitectura limpia / hexagonal. Separación estricta de capas:

```
RetroPad/
├── RetroPad.Core/              # Domain: entidades, interfaces, contratos
├── RetroPad.Application/       # Use Cases: orquestación, servicios de aplicación
├── RetroPad.Infrastructure/    # Adaptadores: persistencia, syntax highlight, formatting
├── RetroPad.UI/                # Shell: WPF, ViewModels, vistas
└── RetroPad.sln
```

### Capas

| Capa | Responsabilidad |
|---|---|
| **Core** | Entidades (Document, Tab, Session, AppConfig), interfaces de puertos (IDocumentRepository, ISessionStore, ISyntaxHighlighter, ICodeFormatter, IConfigStore) |
| **Application** | Servicios que orquestan los puertos: SessionService, DocumentService, FormattingService, ConfigService |
| **Infrastructure** | Implementaciones concretas: FileSessionStore, AvalonEditSyntaxHighlighter, JsonCodeFormatters, JsonConfigStore |
| **UI** | WPF MainWindow, TabViewModel, EditorViewModel, Dialogs, Themes |

## UI Design

### Tema

- Fondo: negro profundo (#000000 o cercano)
- Estética DOS/terminal/CRT
- Sin transparencias, sombras, animaciones

### Tipografía

Seleccionar automáticamente la primera disponible:

1. Cascadia Mono
2. Consolas
3. JetBrains Mono

### Cursor

- Tipo bloque o línea
- Parpadeo clásico

### Paleta de colores

| Elemento | Color |
|---|---|
| Texto normal | Gris claro |
| Palabras reservadas | Azul |
| Strings | Verde |
| Números | Amarillo |
| Comentarios | Gris oscuro |
| Tipos | Celeste |
| Métodos | Blanco |
| Errores | Rojo |

## Functionality

### Pestañas

- Cada pestaña = un documento
- Al cerrar la app: NO preguntar, guardar estado automáticamente
- Restaurar exactamente al reabrir

### Persistencia de sesión

Recordar: texto, posición del cursor, scroll, pestañas abiertas, pestaña activa.

Carpeta: `AppData/Local/RetroPad/Session/`

Formato: archivos JSON por pestaña (Session1.tmp, Session2.tmp, etc.)

NO guardar sobre archivo original automáticamente. Solo al pulsar Guardar.

### Resaltado de sintaxis

Librería consolidada (AvalonEdit incluye esto).

Detección automática por extensión + cambio manual desde menú.

Lenguajes mínimos:

Plain Text, JSON, XML, HTML, CSS, JavaScript, TypeScript, C#, C++, C, Java, Python, Go, Rust, PHP, SQL, PowerShell, Bash, Markdown, YAML, INI, Dockerfile

### Formateo de código

Menú: Formato > Formatear documento

Lenguajes con formateador: JSON, XML, HTML, CSS, JavaScript, TypeScript, C#, Python, SQL, Markdown

Si no hay formateador para un lenguaje: dejar texto intacto.

El formateo corrige indentación, espacios, saltos de línea, llaves, tabulación. Nunca modifica lógica.

### Edición

- Undo / Redo
- Copiar / Pegar / Cortar
- Seleccionar todo
- Buscar / Reemplazar
- Ir a línea
- Duplicar línea
- Eliminar línea
- Mover línea arriba / abajo

### Archivos grandes

Objetivo: 100 MB sin congelar. Usar renderizado virtual de AvalonEdit. Evitar múltiples copias del documento.

## Configuration

Archivo JSON en `AppData/Local/RetroPad/config.json`:

```json
{
  "theme": "retro-dark",
  "fontFamily": "Cascadia Mono",
  "fontSize": 14,
  "tabSize": 4,
  "insertSpaces": true,
  "rememberSession": true,
  "lastDirectory": "",
  "lastLanguage": "PlainText"
}
```

## Dependencies

- **.NET 8** (o .NET 9) + **WPF**
- **AvalonEdit** (editor de texto, resaltado de sintaxis, manejo de archivos grandes)
- **Newtonsoft.Json** (serialización JSON para sesión y config)
- **System.Text.Json** (alternativa si se prefiere evitar dependencia extra)

Evitar dependencias pesadas. No motores de IDE.

## Quality

- Código documentado (XML docs en interfaces y métodos públicos)
- Métodos pequeños (<30 líneas ideal)
- Nombres claros en inglés
- Sin duplicación
- Separación estricta de responsabilidades
