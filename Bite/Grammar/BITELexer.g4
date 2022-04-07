lexer grammar BITELexer;



@members {
    int nesting = 0;
}

DeclareModule: 'module';  
DeclareClass: 'class';
DeclareStruct: 'struct';
DeclareClassInstance: 'new';
DeclareFunction: 'function';
DeclareVariable: 'var';
DeclareGetter: 'get';
DeclareSetter: 'set';
DeclareForLoop: 'for';
DeclareWhileLoop: 'while';
DeclareStatic: 'static';  
DeclareAbstract: 'abstract'; 
DeclarePublic: 'public';   
DeclarePrivate: 'private'; 
ControlFlowIf: 'if';
ControlFlowElse: 'else';
FunctionReturn: 'return';
Break: 'break';
NullReference: 'null';
ThisReference: 'this';
UsingDirective: 'using';
ImportDirective: 'import';

AssignOperator: '=';
PlusAssignOperator: '+=';
MinusAssignOperator: '-=';
MultiplyAssignOperator: '*=';
DivideAssignOperator: '/=';
ModuloAssignOperator: '%=';
BitwiseAndAssignOperator: '&=';
BitwiseOrAssignOperator: '|=';
BitwiseXorAssignOperator: '^=';
BitwiseLeftShiftAssignOperator: '<<=';
BitwiseRightShiftAssignOperator: '>>=';

LogicalOrOperator: '||';
LogicalAndOperator: '&&';

UnequalOperator: '!=';
EqualOperator: '==';

GreaterOperator: '>';
ShiftRightOperator: '>>';
GreaterEqualOperator: '>=';
SmallerOperator: '<';
ShiftLeftOperator: '<<';
SmallerEqualOperator: '<=';

MinusOperator: '-';
MinusMinusOperator: '--';
PlusOperator: '+';
PlusPlusOperator: '++';
DivideOperator: '/';
MultiplyOperator: '*';

LogicalNegationOperator: '!';
DotOperator: '.';
QuestionMarkOperator: '?';
ColonOperator: ':';
ReferenceOperator: '->';

ModuloOperator: '%';

ComplimentOperator: '~';

BitwiseAndOperator:'&';
BitwiseXorOperator:'^';                            
BitwiseOrOperator:'|';
  
OpeningRoundBracket: '(';
ClosingRoundBracket: ')';
OpeningCurlyBracket: '{';
ClosingCurlyBracket: '}';
SquarebracketLeft : '[';
SquarebracketRight: ']';

CommaSeperator: ',';
SemicolonSeperator: ';';

DollarOperator:'$';

BooleanLiteral: False_ | True_;
False_: 'false';
True_: 'true';

IntegerLiteral:
	DecimalLiteral;

FloatingLiteral:
	Fractionalconstant Exponentpart?
	| Digitsequence Exponentpart;


StringLiteral
  : UnterminatedStringLiteral '"'
  ;

UnterminatedStringLiteral
  : '"' (~["\\\r\n] | '\\' (. | EOF))*
  ;

fragment DIGIT: [0-9];

DecimalLiteral: (DIGIT)+;

fragment NONZERODIGIT: [1-9];


fragment Fractionalconstant:
	Digitsequence? '.' Digitsequence
	| Digitsequence '.';

fragment Exponentpart:
	'e' SIGN? Digitsequence
	| 'E' SIGN? Digitsequence;

fragment SIGN: [+-];

fragment Digitsequence: DIGIT ('\''? DIGIT)*;



Identifier: [a-zA-Z_][a-zA-Z0-9_]* ;

COMMENT:   '/*' .*? '*/'    -> skip; 
WS  :   [ \r\t\u000C\n]+ -> skip;

LINE_COMMENT: '//' ~[\r\n]* '\r'? '\n' -> skip;

DQUOTE: '"';
/*DOLLAR_DQUOTE: '$"' -> pushMode(IN_STRING);
LPAR: '{' {
    nesting++;
    PushMode(DEFAULT_MODE);
};
RPAR: '}' {
    if (nesting > 0) {
        nesting--;
        PopMode();
    }
};


mode IN_STRING;

TEXT: ~[\\"]+ ;
LPAR_IN_String: '{' {
    nesting++;
    PushMode(DEFAULT_MODE);
};
ESCAPE_SEQUENCE: '{' . ;
DQUOTE_IN_STRING: '"' -> type(DQUOTE), popMode;*/
	



