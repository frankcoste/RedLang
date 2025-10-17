# RedLang Compiler

Compilador de RedLang a código nativo mediante LLVM IR.

## Requisitos

- .NET 8.0 SDK
- LLVM 20.1.2 (con clang)
- ANTLR 4.13.2
- Sistema operativo **Linux** (no compatible actualmente con Windows, ya que LLVM no está operativo en ese entorno), sin embargo puede ejecutarlo con WSL sin problemas.


## Instalación

1. Clonar el repositorio:
```bash
git clone https://github.com/tu-usuario/redlang.git
cd redlang
```

2. Restaurar dependencias:
```bash
dotnet restore
```

3. Compilar el proyecto:
```bash
dotnet build
```

## Uso

1. Crear un archivo con código RedLang (ej. `programa.red`)

2. Compilar y ejecutar:
```bash
dotnet run
```

El compilador:
- Lee el archivo `CodigoRedlang.txt`
- Genera `output.ll` (LLVM IR)
- Compila a `output.exe` (ejecutable nativo)

## Ejemplo de Código
```redlang
declare x: i = 10;
declare y: i = 20;

func suma(a: i, b: i) : i {
    give a + b;
}

declare resultado: i = suma(x, y);
show(resultado);
```

## Estructura del Proyecto
```
MiProyectoANTLR/
├── Generated/              # Archivos generados por ANTLR
│   ├── ExprLexer.g4       # Gramática del lexer
│   ├── RedLang.g4         # Gramática del parser
│   └── *.cs               # Clases generadas
├── LLVMCodeGeneratorVisitor.cs  # Generador de código LLVM
├── RedLangVisitor.cs      # Intérprete (para pruebas)
├── program.cs             # Punto de entrada
└── CodigoRedlang.txt      # Archivo de código fuente
```

## Documentación

Para la sintaxis completa del lenguaje, consulta [SINTAXIS.md](SINTAXIS.md)

## Contribuciones

Las contribuciones son bienvenidas. Por favor:
1. Fork el proyecto
2. Crea una rama para tu feature
3. Commit tus cambios
4. Push a la rama
5. Abre un Pull Request
