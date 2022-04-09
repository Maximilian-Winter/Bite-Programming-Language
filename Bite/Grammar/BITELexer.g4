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

CURLY_L: '{' -> pushMode(DEFAULT_MODE);
CURLY_R: '}' -> popMode; // When we see this, revert to the previous context.

OPEN_STRING: '"' -> pushMode(STRING); // Switch context

// Define rules on how tokens are recognized within a string.
// Note that complex escapes, like Unicode, are not illustrated here.
mode STRING;

ENTER_EXPR_INTERP: '${' -> pushMode(DEFAULT_MODE); // When we see this, start parsing program tokens.

ID_INTERP: '$'[A-Za-z_][A-Za-z0-9_]*;
ESCAPED_DOLLAR: '\\$';
ESCAPED_QUOTE: '\\"';
TEXT: ~('$'|'\n'|'"')+; // This doesn't cover escapes, FYI.

CLOSE_STRING: '"' -> popMode; // Revert to the previous mode; our string is closed.
	



