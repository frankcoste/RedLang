# Sintaxis de RedLang

Guía completa de la sintaxis del lenguaje RedLang.

## Tabla de Contenidos

1. [Tipos de Datos](#tipos-de-datos)
2. [Declaración de Variables](#declaración-de-variables)
3. [Literales](#literales)
4. [Operadores](#operadores)
5. [Expresiones](#expresiones)
6. [Estructuras de Control](#estructuras-de-control)
7. [Funciones](#funciones)
8. [Entrada/Salida](#entradasalida)
9. [Arrays](#arrays)
10. [Comentarios](#comentarios)

---

## Tipos de Datos

RedLang soporta los siguientes tipos básicos:

| Tipo     | Símbolo | Descripción                 | Ejemplo         |
|----------|---------|-----------------------------|-----------------|
| Entero   |   `i`   | Números enteros de 32 bits  | `42`, `10`      |
| Flotante |   `f`   | Números de punto flotante   | `3.14`, `0.5`   |
| String   |   `s`   | Cadenas de texto            | `"Hola"`        |
| Booleano |   `b`   | Valores lógicos             | `true`, `false` |

### Tipos Especiales

- **Nullable**: Agregar `?` después del tipo permite valores nulos
```redlang
  declare valor: i? = null;
```

- **Arrays**: Colecciones de elementos del mismo tipo
```redlang
  declare numeros: array[i];
```

---

## Declaración de Variables

### Sintaxis Básica
```redlang
declare nombre_variable: tipo;
declare nombre_variable: tipo = valor_inicial;
```

### Ejemplos
```redlang
// Declaración sin inicialización (valor por defecto)
declare edad: i;

// Declaración con inicialización
declare nombre: s = "Juan";
declare pi: f = 3.14159;
declare activo: b = true;

// Variable nullable
declare resultado: i? = null;
```

---

## Literales

### Enteros
```redlang
declare num: i = 42;
```

### Flotantes
```redlang
declare precio: f = 19.99;
declare temperatura: f = 5.5;
```

### Strings
```redlang
declare mensaje: s = "Hola Mundo";
declare vacio: s = "";
```

### Booleanos
```redlang
declare verdadero: b = true;
declare falso: b = false;
```

### Null
```redlang
declare opcional: i? = null;
```

---

## Operadores

### Aritméticos

| Operador | Descripción | Ejemplo | Resultado |
|----------|-------------|---------|-----------|
| `+` | Suma | `5 + 3` | `8` |
| `-` | Resta | `5 - 3` | `2` |
| `*` | Multiplicación | `5 * 3` | `15` |
| `/` | División | `6 / 2` | `3` |
| `%` | Módulo | `7 % 3` | `1` |

### Relacionales

| Operador | Descripción | Ejemplo | Resultado |
|----------|-------------|---------|-----------|
| `>` | Mayor que | `5 > 3` | `true` |
| `<` | Menor que | `5 < 3` | `false` |
| `>=` | Mayor o igual | `5 >= 5` | `true` |
| `<=` | Menor o igual | `3 <= 5` | `true` |
| `==` | Igual a | `5 == 5` | `true` |
| `!=` | Diferente de | `5 != 3` | `true` |

### Lógicos

| Operador | Descripción | Ejemplo |
|----------|-------------|---------|
| `and` | AND lógico | `true and false` → `false` |
| `or` | OR lógico | `true or false` → `true` |
| `not` | NOT lógico | `not true` → `false` |

### Unarios

| Operador | Descripción | Ejemplo |
|----------|-------------|---------|
| `-` | Negación aritmética | `-5` → `-5` |
| `not` | Negación lógica | `not true` → `false` |

---

## Expresiones

### Precedencia de Operadores

De mayor a menor precedencia:

1. Paréntesis `()`
2. Unarios: `-`, `not`
3. Multiplicativos: `*`, `/`, `%`
4. Aditivos: `+`, `-`
5. Relacionales: `>`, `<`, `>=`, `<=`
6. Igualdad: `==`, `!=`
7. AND lógico: `and`
8. OR lógico: `or`

### Ejemplos
```redlang
// Aritmética
declare resultado: i = (5 + 3) * 2;  // 16

// Comparaciones
declare es_mayor: b = 10 > 5;  // true

// Lógica
declare valido: b = (x > 0) and (x < 100);

// Mixto
declare calc: i = -5 + 3 * 2;  // 1
```

---

## Estructuras de Control

### Condicionales (if)
```redlang
check (condición) {
    // código si verdadero
}

check (condición) {
    // código si verdadero
} otherwise {
    // código si falso
}
```

**Ejemplo:**
```redlang
declare edad: i = 18;

check (edad >= 18) {
    show("Mayor de edad");
} otherwise {
    show("Menor de edad");
}
```

### Bucle While
```redlang
repeat (condición) {
    // código a repetir
}
```

**Ejemplo:**
```redlang
declare contador: i = 0;

repeat (contador < 5) {
    show(contador);
    set contador = contador + 1;
}
```

### Bucle For
```redlang
loop (inicialización; condición; incremento) {
    // código a repetir
}
```

**Ejemplo:**
```redlang
loop (declare i: i = 0; i < 10; set i = i + 1) {
    show(i);
}
```

---

## Funciones

### Declaración
```redlang
func nombre_funcion(parametro1: tipo1, parametro2: tipo2) : tipo_retorno {
    // cuerpo de la función
    give valor_retorno;
}
```

### Ejemplos

**Función simple:**
```redlang
func suma(a: i, b: i) : i {
    give a + b;
}
```

**Función sin parámetros:**
```redlang
func obtener_pi() : f {
    give 3.14159;
}
```

**Función con múltiples operaciones:**
```redlang
func factorial(n: i) : i {
    check (n <= 1) {
        give 1;
    } otherwise {
        give n * factorial(n - 1);
    }
}
```

### Llamada a Funciones
```redlang
declare resultado: i = suma(5, 3);
declare fact: i = factorial(5);

// Como statement
suma(10, 20);
```

---

## Entrada/Salida

### Salida (show)
```redlang
show(expresión);
```

**Ejemplos:**
```redlang
show("Hola Mundo");
show(42);
show(3.14);
show(true);

declare nombre: s = "Ana";
show(nombre);

show(5 + 3);  // Muestra: 8
```

### Entrada (ask)
```redlang
ask(variable);
```

**Ejemplo:**
```redlang
declare edad: i;
show("Ingresa tu edad: ");
ask(edad);
show(edad);
```

---

## Arrays

### Declaración
```redlang
declare nombre: array[tipo];
```

### Acceso a Elementos
```redlang
nombre[índice]
```

### Asignación de Elementos
```redlang
set nombre[índice] = valor;
```

### Ejemplo Completo
```redlang
// Declarar array
declare numeros: array[i];

// Asignar valores
set numeros[0] = 10;
set numeros[1] = 20;
set numeros[2] = 30;

// Acceder a elementos
declare primero: i = numeros[0];
show(primero);  // 10

// Iterar sobre array
loop (declare i: i = 0; i < 3; set i = i + 1) {
    show(numeros[i]);
}
```

---

## Comentarios

### Comentario de Línea
```redlang
// Este es un comentario de una línea
declare x: i = 5;  // comentario al final de línea
```

### Comentario de Bloque
```redlang
/*
  Este es un comentario
  de múltiples líneas
*/
declare y: i = 10;
```

---

## Operaciones de Archivos

### Lectura de Archivos
```redlang
readfile("ruta/archivo.txt");
```

### Escritura de Archivos
```redlang
writefile("ruta/archivo.txt", contenido);
```

**Ejemplo:**
```redlang
declare contenido: s;
readfile("datos.txt");

declare mensaje: s = "Hola desde RedLang";
writefile("salida.txt", mensaje);
```

---

## Ejemplos Completos

### Programa Básico
```redlang
declare num1: i;
declare num2: i;

show("Ingresa el primer número: ");
ask(num1);

show("Ingresa el segundo número: ");
ask(num2);

func suma(a: i, b: i) : i {
    give a + b;
}

declare resultado: i = suma(num1, num2);
show("La suma es: ");
show(resultado);
```

### Factorial Recursivo
```redlang
func factorial(n: i) : i {
    check (n <= 1) {
        give 1;
    } otherwise {
        give n * factorial(n - 1);
    }
}

declare num: i = 5;
declare fact: i = factorial(num);
show("Factorial de 5: ");
show(fact);
```

### Cálculo de Precio con Descuento
```redlang
declare precio: f = 100.0;
declare descuento: f = 0.15;

func aplicar_descuento(precio: f, desc: f) : f {
    give precio - (precio * desc);
}

declare precio_final: f = aplicar_descuento(precio, descuento);
show("Precio final: ");
show(precio_final);
```

### Números Pares del 1 al 10
```redlang
loop (declare i: i = 1; i <= 10; set i = i + 1) {
    check (i % 2 == 0) {
        show(i);
    }
}
```

---

## Palabras Reservadas
```
declare    set        check      otherwise
repeat     loop       func       give
show       ask        true       false
null       and        or         not
array      readfile   writefile
```

## Símbolos Especiales
```
:  =  ;  ?  +  -  *  /  %
>  <  >= <= == !=
(  )  {  }  ,  [  ]
```

---

## Reglas de Nomenclatura

- Los identificadores deben comenzar con una letra
- Pueden contener letras, dígitos y guiones bajos
- No pueden ser palabras reservadas
- Son sensibles a mayúsculas/minúsculas

**Válidos:**
```redlang
edad, nombre_completo, valorX, precio_total, i, j
```

**Inválidos:**
```redlang
1edad, declare, nombre-completo, @valor
```

---

## Notas Importantes

1. **Punto y coma**: Todas las declaraciones deben terminar con `;`
2. **Tipado estático**: Las variables deben tener tipo declarado
3. **Bloques**: Los bloques de código se delimitan con `{ }`
4. **Return**: Las funciones deben usar `give` para retornar valores
5. **Conversiones**: No hay conversión automática de tipos
