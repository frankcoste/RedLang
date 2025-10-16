grammar RedLang;

options { tokenVocab=ExprLexer; }

program
    : (declaration | functionDecl | statement)* EOF
    ;

declaration
    : DECLARE IDENT COLON type (EQUAL expression)? SEMI
    ;

literal
    : INT_LIT
    | FLOAT_LIT
    | STRING_LIT
    | TRUE
    | FALSE
    | NULL
    ;

arguments 
    : expression (COMMA expression)*
    ;

callExpr
    : IDENT LPAREN arguments? RPAREN
    ;

arrayAccess
    : IDENT LBRACK expression RBRACK
    ;

primary
    : literal
    | callExpr          // PRIMERO: tiene prioridad sobre IDENT
    | arrayAccess       // SEGUNDO: también tiene prioridad
    | IDENT             // TERCERO: variable simple
    | LPAREN expression RPAREN
    ;

unary
    : (MINUS | NOT) unary
    | primary
    ;

factor
    : unary ((STAR | SLASH | PERCENT) unary)*
    ;

term
    : factor ((PLUS | MINUS) factor)*
    ;

comparison
    : term ((GT | LT | GTEQ | LTEQ) term)*
    ;

equality
    : comparison ((EQEQ | NOTEQ) comparison)*
    ;

logicAnd
    : equality (AND equality)*
    ;

logicOr
    : logicAnd (OR logicAnd)*
    ;

expression
    : logicOr
    ;

readStmt
    : ASK LPAREN IDENT RPAREN SEMI
    ;

printStmt
    : SHOW LPAREN expression RPAREN SEMI
    ;

assignment
    : SET IDENT EQUAL expression SEMI?
    ;

forStmt
    : LOOP LPAREN (declaration | assignment)? expression? 
    SEMI assignment? RPAREN block
    ;

whileStmt
    : REPEAT LPAREN expression RPAREN block
    ;

ifStmt
    : CHECK LPAREN expression RPAREN block (OTHERWISE block)?
    ;

block
    : LBRACE statement* RBRACE
    ;

statement
    : block
    | assignment
    | ifStmt
    | whileStmt
    | forStmt
    | returnStmt
    | printStmt
    | readStmt
    | arrayAssignment
    | callExpr
    ;

returnStmt
    : GIVE expression SEMI
    ;

param
    : IDENT COLON type
    ;

parameters
    : param (COMMA param)*
    ;

functionDecl
    : FUNC IDENT LPAREN parameters? RPAREN COLON type block
    ;
    
type
    : BASETYPE QUESTION?
    | ARRAY LBRACK BASETYPE RBRACK
    ;

arrayAssignment
    : SET arrayAccess EQUAL expression SEMI
    ;

readFileStmt
    : READFILE LPAREN STRING_LIT RPAREN SEMI
    ;

writeFileStmt
    : WRITEFILE LPAREN STRING_LIT COMMA expression RPAREN SEMI
    ;