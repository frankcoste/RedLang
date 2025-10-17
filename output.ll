; ModuleID = 'RedLangModule'
source_filename = "RedLangModule"

@str = private unnamed_addr constant [31 x i8] c"Bienvenido Ingrese su nombre: \00", align 1
@fmt = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1
@fmt.1 = private unnamed_addr constant [3 x i8] c"%s\00", align 1
@str.2 = private unnamed_addr constant [34 x i8] c"Gracias por probar mi compilador \00", align 1
@str.3 = private unnamed_addr constant [32 x i8] c"--- Declaraciones iniciales ---\00", align 1
@str.4 = private unnamed_addr constant [14 x i8] c"Edad inicial:\00", align 1
@fmt.5 = private unnamed_addr constant [4 x i8] c"%d\0A\00", align 1
@str.6 = private unnamed_addr constant [16 x i8] c"Precio inicial:\00", align 1
@fmt.7 = private unnamed_addr constant [4 x i8] c"%f\0A\00", align 1
@str.8 = private unnamed_addr constant [8 x i8] c"RedLang\00", align 1
@str.9 = private unnamed_addr constant [16 x i8] c"Nombre inicial:\00", align 1
@str.10 = private unnamed_addr constant [27 x i8] c"Estado inicial (isActive):\00", align 1
@str_true = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str.11 = private unnamed_addr constant [21 x i8] c"--- Asignaciones ---\00", align 1
@str.12 = private unnamed_addr constant [12 x i8] c"Nueva edad:\00", align 1
@str.13 = private unnamed_addr constant [22 x i8] c"Puntaje en scores[0]:\00", align 1
@str.14 = private unnamed_addr constant [33 x i8] c"Puntaje en scores[1] (edad + 5):\00", align 1
@str.15 = private unnamed_addr constant [20 x i8] c"--- Expresiones ---\00", align 1
@str.16 = private unnamed_addr constant [36 x i8] c"Resultado de (x * 2.5) + (y / 2.0):\00", align 1
@str.17 = private unnamed_addr constant [49 x i8] c"Resultado de flag (x >= y) and (not (x == 0.0)):\00", align 1
@str_true.18 = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false.19 = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str.20 = private unnamed_addr constant [40 x i8] c"Resultado de isEqual (x != y) or false:\00", align 1
@str_true.21 = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false.22 = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str.23 = private unnamed_addr constant [39 x i8] c"--- Estructura condicional (check) ---\00", align 1
@str.24 = private unnamed_addr constant [19 x i8] c"Eres mayor de edad\00", align 1
@str.25 = private unnamed_addr constant [19 x i8] c"Eres menor de edad\00", align 1
@str.26 = private unnamed_addr constant [29 x i8] c"--- Bucle while (repeat) ---\00", align 1
@str.27 = private unnamed_addr constant [19 x i8] c"Cuenta regresiva: \00", align 1
@str.28 = private unnamed_addr constant [25 x i8] c"--- Bucle for (loop) ---\00", align 1
@str.29 = private unnamed_addr constant [10 x i8] c"\C3\8Dndice: \00", align 1
@str.30 = private unnamed_addr constant [29 x i8] c"--- Llamadas a funciones ---\00", align 1
@str.31 = private unnamed_addr constant [31 x i8] c"Suma (resultado de add(7, 8)):\00", align 1
@str.32 = private unnamed_addr constant [46 x i8] c"\C2\BFEs positivo? (resultado de isPositive(-5)):\00", align 1
@str_true.33 = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false.34 = private unnamed_addr constant [6 x i8] c"false\00", align 1
@str.35 = private unnamed_addr constant [22 x i8] c"--- Valores nulos ---\00", align 1
@str.36 = private unnamed_addr constant [28 x i8] c"La variable 'empty' es null\00", align 1
@str.37 = private unnamed_addr constant [31 x i8] c"--- Operaciones con arrays ---\00", align 1
@str.38 = private unnamed_addr constant [40 x i8] c"Valor de 'first' (tomado de scores[0]):\00", align 1
@str.39 = private unnamed_addr constant [61 x i8] c"Valor de scores[2] (resultado de add(scores[0], scores[1])):\00", align 1
@str.40 = private unnamed_addr constant [28 x i8] c"--- Expresi\C3\B3n compleja ---\00", align 1
@str.41 = private unnamed_addr constant [37 x i8] c"Resultado de la expresi\C3\B3n compleja:\00", align 1
@str_true.42 = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false.43 = private unnamed_addr constant [6 x i8] c"false\00", align 1

declare i32 @printf(ptr, ...)

define i32 @add(i32 %a, i32 %c) {
entry:
  %a1 = alloca i32, align 4
  store i32 %a, ptr %a1, align 4
  %c2 = alloca i32, align 4
  store i32 %c, ptr %c2, align 4
  %a3 = load i32, ptr %a1, align 4
  %c4 = load i32, ptr %c2, align 4
  %addtmp = add i32 %a3, %c4
  ret i32 %addtmp
}

define i1 @isPositive(i32 %n) {
entry:
  %n1 = alloca i32, align 4
  store i32 %n, ptr %n1, align 4
  %n2 = load i32, ptr %n1, align 4
  %cmptmp = icmp sgt i32 %n2, 0
  br i1 %cmptmp, label %then, label %else

then:                                             ; preds = %entry
  ret i1 true

else:                                             ; preds = %entry
  ret i1 false
}

define i32 @main() {
entry:
  %nombre = alloca ptr, align 8
  store ptr null, ptr %nombre, align 8
  %printcall = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str)
  %nombre_buffer = alloca [1024 x i8], align 1
  %buffer_ptr = getelementptr [1024 x i8], ptr %nombre_buffer, i32 0, i32 0
  %scancall = call i32 (ptr, ...) @scanf(ptr @fmt.1, ptr %buffer_ptr)
  store ptr %buffer_ptr, ptr %nombre, align 8
  %printcall1 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.2)
  %nombre2 = load ptr, ptr %nombre, align 8
  %printcall3 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %nombre2)
  %printcall4 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.3)
  %age = alloca i32, align 4
  store i32 25, ptr %age, align 4
  %printcall5 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.4)
  %age6 = load i32, ptr %age, align 4
  %printcall7 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %age6)
  %price = alloca double, align 8
  store double 1.999000e+01, ptr %price, align 8
  %printcall8 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.6)
  %price9 = load double, ptr %price, align 8
  %printcall10 = call i32 (ptr, ...) @printf(ptr @fmt.7, double %price9)
  %name = alloca ptr, align 8
  store ptr @str.8, ptr %name, align 8
  %printcall11 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.9)
  %name12 = load ptr, ptr %name, align 8
  %printcall13 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %name12)
  %isActive = alloca i1, align 1
  store i1 true, ptr %isActive, align 1
  %printcall14 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.10)
  %isActive15 = load i1, ptr %isActive, align 1
  %bool_str = select i1 %isActive15, ptr @str_true, ptr @str_false
  %printcall16 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str)
  %maybeNumber = alloca i32, align 4
  store i32 0, ptr %maybeNumber, align 4
  %scores = alloca [100 x i32], align 4
  store [100 x i32] zeroinitializer, ptr %scores, align 4
  %nombre17 = alloca ptr, align 8
  store ptr null, ptr %nombre17, align 8
  %printcall18 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.11)
  store i32 30, ptr %age, align 4
  %printcall19 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.12)
  %age20 = load i32, ptr %age, align 4
  %printcall21 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %age20)
  %arrayidx = getelementptr i32, ptr %scores, i32 0
  store i32 100, ptr %arrayidx, align 4
  %printcall22 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.13)
  %arrayidx23 = getelementptr i32, ptr %scores, i32 0
  %arrayload = load i32, ptr %arrayidx23, align 4
  %printcall24 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload)
  %age25 = load i32, ptr %age, align 4
  %addtmp = add i32 %age25, 5
  %arrayidx26 = getelementptr i32, ptr %scores, i32 1
  store i32 %addtmp, ptr %arrayidx26, align 4
  %printcall27 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.14)
  %arrayidx28 = getelementptr i32, ptr %scores, i32 1
  %arrayload29 = load i32, ptr %arrayidx28, align 4
  %printcall30 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload29)
  %printcall31 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.15)
  %x = alloca double, align 8
  store double 1.000000e+01, ptr %x, align 8
  %y = alloca double, align 8
  store double 3.000000e+00, ptr %y, align 8
  %result = alloca double, align 8
  %x32 = load double, ptr %x, align 8
  %fmultmp = fmul double %x32, 2.500000e+00
  %y33 = load double, ptr %y, align 8
  %fdivtmp = fdiv double %y33, 2.000000e+00
  store double 0.000000e+00, ptr %result, align 8
  %printcall34 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.16)
  %result35 = load double, ptr %result, align 8
  %printcall36 = call i32 (ptr, ...) @printf(ptr @fmt.7, double %result35)
  %flag = alloca i1, align 1
  %x37 = load double, ptr %x, align 8
  %y38 = load double, ptr %y, align 8
  %fcmptmp = fcmp oge double %x37, %y38
  %x39 = load double, ptr %x, align 8
  %feqtmp = fcmp oeq double %x39, 0.000000e+00
  store i32 0, ptr %flag, align 4
  %printcall40 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.17)
  %flag41 = load i1, ptr %flag, align 1
  %bool_str42 = select i1 %flag41, ptr @str_true.18, ptr @str_false.19
  %printcall43 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str42)
  %isEqual = alloca i1, align 1
  %x44 = load double, ptr %x, align 8
  %y45 = load double, ptr %y, align 8
  %fnetmp = fcmp one double %x44, %y45
  store i1 false, ptr %isEqual, align 1
  %printcall46 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.20)
  %isEqual47 = load i1, ptr %isEqual, align 1
  %bool_str48 = select i1 %isEqual47, ptr @str_true.21, ptr @str_false.22
  %printcall49 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str48)
  %printcall50 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.23)
  %age51 = load i32, ptr %age, align 4
  %cmptmp = icmp sge i32 %age51, 18
  br i1 %cmptmp, label %then, label %else

then:                                             ; preds = %entry
  %printcall52 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.24)
  br label %ifcont

else:                                             ; preds = %entry
  %printcall53 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.25)
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  %printcall54 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.26)
  %counter = alloca i32, align 4
  store i32 3, ptr %counter, align 4
  br label %whilecond

whilecond:                                        ; preds = %whileloop, %ifcont
  %counter55 = load i32, ptr %counter, align 4
  %cmptmp56 = icmp sgt i32 %counter55, 0
  br i1 %cmptmp56, label %whileloop, label %afterwhile

whileloop:                                        ; preds = %whilecond
  %printcall57 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.27)
  %counter58 = load i32, ptr %counter, align 4
  %printcall59 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %counter58)
  %counter60 = load i32, ptr %counter, align 4
  %subtmp = sub i32 %counter60, 1
  store i32 %subtmp, ptr %counter, align 4
  br label %whilecond

afterwhile:                                       ; preds = %whilecond
  %printcall61 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.28)
  %j = alloca i32, align 4
  store i32 0, ptr %j, align 4
  br label %forcond

forcond:                                          ; preds = %forinc, %afterwhile
  %j62 = load i32, ptr %j, align 4
  %cmptmp63 = icmp slt i32 %j62, 3
  br i1 %cmptmp63, label %forloop, label %afterfor

forloop:                                          ; preds = %forcond
  %printcall64 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.29)
  %j65 = load i32, ptr %j, align 4
  %printcall66 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %j65)
  br label %forinc

forinc:                                           ; preds = %forloop
  %j67 = load i32, ptr %j, align 4
  %addtmp68 = add i32 %j67, 1
  store i32 %addtmp68, ptr %j, align 4
  br label %forcond

afterfor:                                         ; preds = %forcond
  %printcall69 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.30)
  %total = alloca i32, align 4
  %calltmp = call i32 @add(i32 7, i32 8)
  store i32 %calltmp, ptr %total, align 4
  %printcall70 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.31)
  %total71 = load i32, ptr %total, align 4
  %printcall72 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %total71)
  %positive = alloca i1, align 1
  %calltmp73 = call i1 @isPositive(i32 -5)
  store i1 %calltmp73, ptr %positive, align 1
  %printcall74 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.32)
  %positive75 = load i1, ptr %positive, align 1
  %bool_str76 = select i1 %positive75, ptr @str_true.33, ptr @str_false.34
  %printcall77 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str76)
  %printcall78 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.35)
  %empty = alloca i32, align 4
  store i32 0, ptr %empty, align 4
  %empty79 = load i32, ptr %empty, align 4
  %eqtmp = icmp eq i32 %empty79, 0
  br i1 %eqtmp, label %then80, label %ifcont81

then80:                                           ; preds = %afterfor
  %printcall82 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.36)
  br label %ifcont81

ifcont81:                                         ; preds = %then80, %afterfor
  %printcall83 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.37)
  %first = alloca i32, align 4
  %arrayidx84 = getelementptr i32, ptr %scores, i32 0
  %arrayload85 = load i32, ptr %arrayidx84, align 4
  store i32 %arrayload85, ptr %first, align 4
  %printcall86 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.38)
  %first87 = load i32, ptr %first, align 4
  %printcall88 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %first87)
  %arrayidx89 = getelementptr i32, ptr %scores, i32 0
  %arrayload90 = load i32, ptr %arrayidx89, align 4
  %arrayidx91 = getelementptr i32, ptr %scores, i32 1
  %arrayload92 = load i32, ptr %arrayidx91, align 4
  %calltmp93 = call i32 @add(i32 %arrayload90, i32 %arrayload92)
  %arrayidx94 = getelementptr i32, ptr %scores, i32 2
  store i32 %calltmp93, ptr %arrayidx94, align 4
  %printcall95 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.39)
  %arrayidx96 = getelementptr i32, ptr %scores, i32 2
  %arrayload97 = load i32, ptr %arrayidx96, align 4
  %printcall98 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload97)
  %printcall99 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.40)
  %complex = alloca i1, align 1
  store i32 0, ptr %complex, align 4
  %printcall100 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.41)
  %complex101 = load i1, ptr %complex, align 1
  %bool_str102 = select i1 %complex101, ptr @str_true.42, ptr @str_false.43
  %printcall103 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str102)
  ret i32 0
}

declare i32 @scanf(ptr, ...)
