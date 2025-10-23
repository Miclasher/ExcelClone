grammar LabCalculator;

compileUnit : expression EOF;

expression
    : op=(ADD|SUBTRACT) expression                    #UnaryExpr
    | expression op=(MULTIPLY | DIVIDE | MOD | DIV) expression #MultiplicativeExpr
    | expression op=(ADD | SUBTRACT) expression       #AdditiveExpr
    | expression op=(GT | GTE | LT | LTE | EQ | NEQ) expression #ComparisonExpr
    | op=(MAX|MIN) LPAREN (expression (COMMA expression)*)? RPAREN #MinMaxExpr
    | LPAREN expression RPAREN                        #ParenthesizedExpr
    | atom                                            #AtomExpr
    ;

atom
    : NUMBER          #NumberAtom
    | BOOLEAN         #BooleanAtom
    | IDENTIFIER      #CellReferenceAtom
    ;


NUMBER   : INT ('.' INT+)? | '.' INT+ ;
BOOLEAN  : 'TRUE' | 'FALSE' | 'true' | 'false';
IDENTIFIER : [a-zA-Z]+[0-9]+;

INT      : [0-9]+;

MULTIPLY : '*';
DIVIDE   : '/';
SUBTRACT : '-';
ADD      : '+';
MOD      : 'mod';
DIV      : 'div';

MAX      : 'mmax';
MIN      : 'mmin';

GT       : '>';
GTE      : '>=';
LT       : '<';
LTE      : '<=';
EQ       : '=';
NEQ      : '<>';

LPAREN   : '(';
RPAREN   : ')';
COMMA    : ',';

WS       : [ \t\r\n] -> channel(HIDDEN);

