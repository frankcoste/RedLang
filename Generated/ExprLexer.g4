lexer grammar ExprLexer;
// Palabras reservadas
DECLARE   : 'declare' ;
SET       : 'set' ;
CHECK     : 'check' ;
OTHERWISE : 'otherwise' ;
REPEAT    : 'repeat' ;
LOOP      : 'loop' ;
FUNC      : 'func' ;
GIVE      : 'give' ;
SHOW      : 'show' ;
ASK       : 'ask' ;
TRUE      : 'true' ;
FALSE     : 'false' ;
NULL      : 'null' ;
AND       : 'and' ;
OR        : 'or' ;
NOT       : 'not' ;
ARRAY : 'array' ;
READFILE : 'readfile' ;
WRITEFILE : 'writefile' ;

// Operadores y puntuación
COLON     : ':' ;
EQUAL     : '=' ;
SEMI      : ';' ;
QUESTION  : '?' ;
PLUS      : '+' ;
MINUS     : '-' ;
STAR      : '*' ;
SLASH     : '/' ;
PERCENT   : '%' ;
GT        : '>' ;
LT        : '<' ;
GTEQ      : '>=' ;
LTEQ      : '<=' ;
EQEQ      : '==' ;
NOTEQ     : '!=' ;
LPAREN    : '(' ;
RPAREN    : ')' ;
LBRACE    : '{' ;
RBRACE    : '}' ;
COMMA     : ',' ;
LBRACK : '[' ;
RBRACK : ']' ;

// Tipos básicos
BASETYPE  : 'i' | 'f' | 's' | 'b' ;

// Identificadores y literales
IDENT     : LETTER (LETTER | DIGIT | '_')* ;
INT_LIT   : DIGIT+ ;
FLOAT_LIT : DIGIT+ '.' DIGIT+ ;
STRING_LIT
    : '"' (~["\\\r\n] | ESC)* '"'
    | '\'' (~['\\\r\n] | ESC)* '\''
    ;

fragment ESC
    : '\\' [nrtbf"'\\]
    ;

// Fragmentos
fragment DIGIT : [0-9] ;
fragment LETTER: [A-Za-z] ;

// Ignorar espacios
WS : [ \t\r\n]+ -> skip ;

// Comentarios
COMMENT     : '/*' .*? '*/' -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;
