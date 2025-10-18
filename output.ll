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
@str.40 = private unnamed_addr constant [43 x i8] c"--- Recorriendo un Arreglo de N\C3\BAmeros ---\00", align 1
@str.41 = private unnamed_addr constant [34 x i8] c"Recorriendo con 'repeat' (while):\00", align 1
@str.42 = private unnamed_addr constant [30 x i8] c"Recorriendo con 'loop' (for):\00", align 1
@str.43 = private unnamed_addr constant [28 x i8] c"--- Expresi\C3\B3n compleja ---\00", align 1
@str.44 = private unnamed_addr constant [37 x i8] c"Resultado de la expresi\C3\B3n compleja:\00", align 1
@str_true.45 = private unnamed_addr constant [5 x i8] c"true\00", align 1
@str_false.46 = private unnamed_addr constant [6 x i8] c"false\00", align 1

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
  %printcall17 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.11)
  store i32 30, ptr %age, align 4
  %printcall18 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.12)
  %age19 = load i32, ptr %age, align 4
  %printcall20 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %age19)
  %arrayidx = getelementptr i32, ptr %scores, i32 0
  store i32 100, ptr %arrayidx, align 4
  %printcall21 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.13)
  %arrayidx22 = getelementptr i32, ptr %scores, i32 0
  %arrayload = load i32, ptr %arrayidx22, align 4
  %printcall23 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload)
  %age24 = load i32, ptr %age, align 4
  %addtmp = add i32 %age24, 5
  %arrayidx25 = getelementptr i32, ptr %scores, i32 1
  store i32 %addtmp, ptr %arrayidx25, align 4
  %printcall26 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.14)
  %arrayidx27 = getelementptr i32, ptr %scores, i32 1
  %arrayload28 = load i32, ptr %arrayidx27, align 4
  %printcall29 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload28)
  %printcall30 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.15)
  %x = alloca double, align 8
  store double 1.000000e+01, ptr %x, align 8
  %y = alloca double, align 8
  store double 3.000000e+00, ptr %y, align 8
  %result = alloca double, align 8
  %x31 = load double, ptr %x, align 8
  %fmultmp = fmul double %x31, 2.500000e+00
  %y32 = load double, ptr %y, align 8
  %fdivtmp = fdiv double %y32, 2.000000e+00
  %faddtmp = fadd double %fmultmp, %fdivtmp
  store double %faddtmp, ptr %result, align 8
  %printcall33 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.16)
  %result34 = load double, ptr %result, align 8
  %printcall35 = call i32 (ptr, ...) @printf(ptr @fmt.7, double %result34)
  %flag = alloca i1, align 1
  %x36 = load double, ptr %x, align 8
  %y37 = load double, ptr %y, align 8
  %fcmptmp = fcmp oge double %x36, %y37
  %x38 = load double, ptr %x, align 8
  %feqtmp = fcmp oeq double %x38, 0.000000e+00
  %nottmp = xor i1 %feqtmp, true
  %andtmp = and i1 %fcmptmp, %nottmp
  store i1 %andtmp, ptr %flag, align 1
  %printcall39 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.17)
  %flag40 = load i1, ptr %flag, align 1
  %bool_str41 = select i1 %flag40, ptr @str_true.18, ptr @str_false.19
  %printcall42 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str41)
  %isEqual = alloca i1, align 1
  %x43 = load double, ptr %x, align 8
  %y44 = load double, ptr %y, align 8
  %fnetmp = fcmp one double %x43, %y44
  %ortmp = or i1 %fnetmp, false
  store i1 %ortmp, ptr %isEqual, align 1
  %printcall45 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.20)
  %isEqual46 = load i1, ptr %isEqual, align 1
  %bool_str47 = select i1 %isEqual46, ptr @str_true.21, ptr @str_false.22
  %printcall48 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str47)
  %printcall49 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.23)
  %age50 = load i32, ptr %age, align 4
  %cmptmp = icmp sge i32 %age50, 18
  br i1 %cmptmp, label %then, label %else

then:                                             ; preds = %entry
  %printcall51 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.24)
  br label %ifcont

else:                                             ; preds = %entry
  %printcall52 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.25)
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  %printcall53 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.26)
  %counter = alloca i32, align 4
  store i32 3, ptr %counter, align 4
  br label %whilecond

whilecond:                                        ; preds = %whileloop, %ifcont
  %counter54 = load i32, ptr %counter, align 4
  %cmptmp55 = icmp sgt i32 %counter54, 0
  br i1 %cmptmp55, label %whileloop, label %afterwhile

whileloop:                                        ; preds = %whilecond
  %printcall56 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.27)
  %counter57 = load i32, ptr %counter, align 4
  %printcall58 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %counter57)
  %counter59 = load i32, ptr %counter, align 4
  %subtmp = sub i32 %counter59, 1
  store i32 %subtmp, ptr %counter, align 4
  br label %whilecond

afterwhile:                                       ; preds = %whilecond
  %printcall60 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.28)
  %j = alloca i32, align 4
  store i32 0, ptr %j, align 4
  br label %forcond

forcond:                                          ; preds = %forinc, %afterwhile
  %j61 = load i32, ptr %j, align 4
  %cmptmp62 = icmp slt i32 %j61, 3
  br i1 %cmptmp62, label %forloop, label %afterfor

forloop:                                          ; preds = %forcond
  %printcall63 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.29)
  %j64 = load i32, ptr %j, align 4
  %printcall65 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %j64)
  br label %forinc

forinc:                                           ; preds = %forloop
  %j66 = load i32, ptr %j, align 4
  %addtmp67 = add i32 %j66, 1
  store i32 %addtmp67, ptr %j, align 4
  br label %forcond

afterfor:                                         ; preds = %forcond
  %printcall68 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.30)
  %total = alloca i32, align 4
  %calltmp = call i32 @add(i32 7, i32 8)
  store i32 %calltmp, ptr %total, align 4
  %printcall69 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.31)
  %total70 = load i32, ptr %total, align 4
  %printcall71 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %total70)
  %positive = alloca i1, align 1
  %calltmp72 = call i1 @isPositive(i32 -5)
  store i1 %calltmp72, ptr %positive, align 1
  %printcall73 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.32)
  %positive74 = load i1, ptr %positive, align 1
  %bool_str75 = select i1 %positive74, ptr @str_true.33, ptr @str_false.34
  %printcall76 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str75)
  %printcall77 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.35)
  %empty = alloca i32, align 4
  store i32 0, ptr %empty, align 4
  %empty78 = load i32, ptr %empty, align 4
  %eqtmp = icmp eq i32 %empty78, 0
  br i1 %eqtmp, label %then79, label %ifcont80

then79:                                           ; preds = %afterfor
  %printcall81 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.36)
  br label %ifcont80

ifcont80:                                         ; preds = %then79, %afterfor
  %printcall82 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.37)
  %first = alloca i32, align 4
  %arrayidx83 = getelementptr i32, ptr %scores, i32 0
  %arrayload84 = load i32, ptr %arrayidx83, align 4
  store i32 %arrayload84, ptr %first, align 4
  %printcall85 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.38)
  %first86 = load i32, ptr %first, align 4
  %printcall87 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %first86)
  %arrayidx88 = getelementptr i32, ptr %scores, i32 0
  %arrayload89 = load i32, ptr %arrayidx88, align 4
  %arrayidx90 = getelementptr i32, ptr %scores, i32 1
  %arrayload91 = load i32, ptr %arrayidx90, align 4
  %calltmp92 = call i32 @add(i32 %arrayload89, i32 %arrayload91)
  %arrayidx93 = getelementptr i32, ptr %scores, i32 2
  store i32 %calltmp92, ptr %arrayidx93, align 4
  %printcall94 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.39)
  %arrayidx95 = getelementptr i32, ptr %scores, i32 2
  %arrayload96 = load i32, ptr %arrayidx95, align 4
  %printcall97 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload96)
  %printcall98 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.40)
  %numberList = alloca [100 x i32], align 4
  store [100 x i32] zeroinitializer, ptr %numberList, align 4
  %arrayidx99 = getelementptr i32, ptr %numberList, i32 0
  store i32 11, ptr %arrayidx99, align 4
  %arrayidx100 = getelementptr i32, ptr %numberList, i32 1
  store i32 22, ptr %arrayidx100, align 4
  %arrayidx101 = getelementptr i32, ptr %numberList, i32 2
  store i32 33, ptr %arrayidx101, align 4
  %arrayidx102 = getelementptr i32, ptr %numberList, i32 3
  store i32 44, ptr %arrayidx102, align 4
  %listSize = alloca i32, align 4
  store i32 4, ptr %listSize, align 4
  %printcall103 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.41)
  %i_while = alloca i32, align 4
  store i32 0, ptr %i_while, align 4
  br label %whilecond104

whilecond104:                                     ; preds = %whileloop105, %ifcont80
  %i_while107 = load i32, ptr %i_while, align 4
  %listSize108 = load i32, ptr %listSize, align 4
  %cmptmp109 = icmp slt i32 %i_while107, %listSize108
  br i1 %cmptmp109, label %whileloop105, label %afterwhile106

whileloop105:                                     ; preds = %whilecond104
  %i_while110 = load i32, ptr %i_while, align 4
  %arrayidx111 = getelementptr i32, ptr %numberList, i32 %i_while110
  %arrayload112 = load i32, ptr %arrayidx111, align 4
  %printcall113 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload112)
  %i_while114 = load i32, ptr %i_while, align 4
  %addtmp115 = add i32 %i_while114, 1
  store i32 %addtmp115, ptr %i_while, align 4
  br label %whilecond104

afterwhile106:                                    ; preds = %whilecond104
  %printcall116 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.42)
  %i_for = alloca i32, align 4
  store i32 0, ptr %i_for, align 4
  br label %forcond117

forcond117:                                       ; preds = %forinc119, %afterwhile106
  %i_for121 = load i32, ptr %i_for, align 4
  %listSize122 = load i32, ptr %listSize, align 4
  %cmptmp123 = icmp slt i32 %i_for121, %listSize122
  br i1 %cmptmp123, label %forloop118, label %afterfor120

forloop118:                                       ; preds = %forcond117
  %i_for124 = load i32, ptr %i_for, align 4
  %arrayidx125 = getelementptr i32, ptr %numberList, i32 %i_for124
  %arrayload126 = load i32, ptr %arrayidx125, align 4
  %printcall127 = call i32 (ptr, ...) @printf(ptr @fmt.5, i32 %arrayload126)
  br label %forinc119

forinc119:                                        ; preds = %forloop118
  %i_for128 = load i32, ptr %i_for, align 4
  %addtmp129 = add i32 %i_for128, 1
  store i32 %addtmp129, ptr %i_for, align 4
  br label %forcond117

afterfor120:                                      ; preds = %forcond117
  %printcall130 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.43)
  %complex = alloca i1, align 1
  store i1 false, ptr %complex, align 1
  %printcall131 = call i32 (ptr, ...) @printf(ptr @fmt, ptr @str.44)
  %complex132 = load i1, ptr %complex, align 1
  %bool_str133 = select i1 %complex132, ptr @str_true.45, ptr @str_false.46
  %printcall134 = call i32 (ptr, ...) @printf(ptr @fmt, ptr %bool_str133)
  ret i32 0
}

declare i32 @scanf(ptr, ...)
