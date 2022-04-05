/*
  S.R.S.L
  Simple Runtime Scripting Language
  
  The use of the keyword var to declare a variable
  
  Data Types:
  
  Text Type:	    string, char
  Numeric Types:	int, float, double
  Sequence Types:	list, tuple
  Mapping Type:	dict
  Boolean Type:	bool
  Binary Types:	bytes, bytearray
  
  
  Features and Keywords:
  Control Flow        if, else, while, for
  Namespaces          namespace
  Functions           function
  ReturnObject        return       
  Classes             class
  ClassInheritance    inherits
  This-Reference      this
  PublicMembers       public
  PrivateMembers      private
  AbstractClasses     abstract class
  AbstractFunctions   abstract function

  StaticClasses     static class
  StaticFunctions   static function
  Operators: 
  
  C# like operators with corresponding precedence and associativity
  
  C# Operator Precedence
  Category	                            Operators
  Postfix Increment and Decrement	        ++, --
  Prefix Increment, Decrement and Unary 	++, --, +, -, !, ~
  Multiplicative	                        *, /, %
  Additive	                                +, -
  Shift	                                    <<, >>
  Relational	                            <, <=, >, >=
  Equality	                                ==, !=
  Bitwise AND	                            &
  Bitwise XOR	                            ^
  Bitwise OR	                            |
  Logical AND	                            &&
  Logical OR	                            ||
  Ternary	                                ?:
  Assignment	                            =, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=
  
  C# Associativity of operators
  Category	                            Operators	                                    Associativity
  Postfix Increment and Decrement	        ++, --	                                        Left to Right
  Prefix Increment, Decrement and Unary	    ++, --, +, -, !, ~	                            Right to Left
  Multiplicative	                        *, /, %	                                        Left to Right
  Additive	                                 +, -	                                        Left to Right
  Shift	                                    <<, >>	                                        Left to Right
  Relational	                            <, <=, >, >=	                                Left to Right
  Equality	                                ==, !=	                                        Left to Right
  Bitwise AND	                            &	                                            Left to Right
  Bitwise XOR	                            ^	                                            Left to Right
  Bitwise OR	                            |	                                            Left to Right
  Logical AND	                            &&	                                            Left to Right
  Logical OR	                            ||	                                            Left to Right
  Ternary	                                ?:	                                            Right to Left
  Assignment	                            =, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=	    Right to Left
                               
*/

grammar BITE;

/** The start rule; begin parsing here. */

program: module*;

module
    : moduleDeclaration
    ( importDirective | usingDirective )*
    declaration*                          
    EOF
    ; 

moduleDeclaration 
    : DeclareModule Identifier ( DotOperator Identifier )* SemicolonSeperator
    ;

importDirective 
    : ImportDirective Identifier ( DotOperator Identifier )* SemicolonSeperator
    ;

usingDirective 
    : UsingDirective Identifier ( DotOperator Identifier )* SemicolonSeperator
    ;

declaration 
    : classDeclaration                  
    | structDeclaration                  
    | functionDeclaration                
    | classInstanceDeclaration           
    | variableDeclaration                     
    | statement 
    ;                            
                                                       
classDeclaration : 
    ( privateModifier | publicModifier )? ( staticModifier | abstractModifier )? 
    DeclareClass Identifier (ColonOperator inheritance)? ( block | SemicolonSeperator )
    ;

structDeclaration : 
    ( privateModifier | publicModifier )? 
    DeclareStruct Identifier (block|SemicolonSeperator)
    ; 

functionDeclaration :
    ( privateModifier | publicModifier )? ( staticModifier | abstractModifier )? 
    DeclareFunction Identifier OpeningRoundBracket parameters? ClosingRoundBracket ( block | SemicolonSeperator )
    ;

classInstanceDeclaration : 
    ( privateModifier | publicModifier )? ( staticModifier )? 
    DeclareVariable Identifier AssignOperator DeclareClassInstance Identifier ( DotOperator Identifier )* OpeningRoundBracket arguments? ClosingRoundBracket SemicolonSeperator
    | Identifier AssignOperator DeclareClassInstance Identifier ( DotOperator Identifier )* OpeningRoundBracket arguments? ClosingRoundBracket SemicolonSeperator
    ;

variableDeclaration 
    : ( privateModifier | publicModifier )? ( staticModifier )? 
    DeclareVariable Identifier ( ( AssignOperator exprStatement )? | SemicolonSeperator )
    ;
   
statements 
    : declaration ( declaration )*
    ;

statement 
    : exprStatement
    | forStatement
    | ifStatement
    | returnStatement
    | breakStatement
    | usingStatement
    | whileStatement
    | block 
    ;  
            
exprStatement 
    : expression SemicolonSeperator 
    ;

localVarDeclaration 
    : Identifier ( ( AssignOperator expression )? ) 
    ;

localVarInitializer 
    : DeclareVariable localVarDeclaration ( CommaSeperator localVarDeclaration )*
    ;

forInitializer 
    : localVarInitializer 
    | expression ( CommaSeperator expression )*
    ;

forIterator
    : expression ( CommaSeperator expression )* 
    ;

forStatement
    : DeclareForLoop OpeningRoundBracket ( forInitializer )?  
    SemicolonSeperator ( condition = expression )?  
    SemicolonSeperator ( forIterator )? 
    ClosingRoundBracket ( statement )?
    ;
                     
ifStatement         
    : ControlFlowIf OpeningRoundBracket expression ClosingRoundBracket trueStatement = statement
    ( ControlFlowElse falseStatement = statement )?
    ;
                     
returnStatement     
    : FunctionReturn expression? SemicolonSeperator
    ;

breakStatement      
    : Break SemicolonSeperator
    ;

usingStatement      
    : UsingDirective OpeningRoundBracket expression ClosingRoundBracket block
    ;

whileStatement
    : DeclareWhileLoop OpeningRoundBracket expression ClosingRoundBracket block 
    ;

block
    : OpeningCurlyBracket declaration* ClosingCurlyBracket 
    ;


expression     
    : assignment;

assignment     
    : call ( AssignOperator | MinusAssignOperator 
        | PlusAssignOperator | MultiplyAssignOperator
        | DivideAssignOperator | ModuloAssignOperator 
        | BitwiseAndAssignOperator | BitwiseOrAssignOperator
        | BitwiseXorAssignOperator | BitwiseLeftShiftAssignOperator 
        | BitwiseRightShiftAssignOperator ) assignment
    | ternary
    ;
  
ternary	       
    : logicOr (QuestionMarkOperator logicOr ColonOperator logicOr)*
    ;

logicOr        
    : logicAnd ( LogicalOrOperator logicAnd )* 
    ;

logicAnd       
    : bitwiseOr ( LogicalAndOperator bitwiseOr )* 
    ;

bitwiseOr      
    : bitwiseXor ( BitwiseOrOperator bitwiseXor )* 
    ;

bitwiseXor     
    : bitwiseAnd ( BitwiseXorOperator bitwiseAnd )* 
    ;

bitwiseAnd     
    : equality ( BitwiseAndOperator equality )*
    ;

equality       
    : relational ( ( UnequalOperator | EqualOperator ) relational )* 
    ;

relational     
    : shift ( ( GreaterOperator | GreaterEqualOperator | SmallerOperator | SmallerEqualOperator ) shift )* 
    ;

shift          
    : additive ( ( ShiftLeftOperator | ShiftRightOperator ) additive )* 
    ;

additive       
    : multiplicative ( ( MinusOperator | PlusOperator ) multiplicative )* 
    ;

multiplicative 
    : unary ( ( DivideOperator | MultiplyOperator | ModuloOperator) unary )* 
    ;

unary          
    : ( LogicalNegationOperator | MinusOperator| PlusOperator | PlusPlusOperator | MinusMinusOperator | ComplimentOperator ) unary 
    | call 
    | unary ( PlusPlusOperator | MinusMinusOperator )
    ;

call           
    : primary (callArguments | DotOperator Identifier| elementAccess )*
    ;

primary        
    : BooleanLiteral 
    | NullReference 
    | ThisReference 
    | IntegerLiteral 
    | FloatingLiteral 
    | StringLiteral 
    | Identifier 
    | OpeningRoundBracket expression ClosingRoundBracket
    ;

privateModifier         : DeclarePrivate;
publicModifier          : DeclarePublic;
abstractModifier        : DeclareAbstract;
staticModifier          : DeclareStatic;                                   
parameters              : parametersIdentifier ( CommaSeperator parametersIdentifier )* ;
arguments               : argumentExpression ( CommaSeperator argumentExpression )* ; 
inheritance             : Identifier ( CommaSeperator Identifier )* ;                  
callArguments           : OpeningRoundBracket arguments? ClosingRoundBracket;
elementAccess           : (SquarebracketLeft elementIdentifier SquarebracketRight)+;    
elementIdentifier       : (IntegerLiteral|StringLiteral|call);   
argumentExpression      : (ReferenceOperator)? expression;
parametersIdentifier    : Identifier;

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

Identifier: [a-zA-Z0-9]+;
	
COMMENT:   '/*' .*? '*/'    -> skip; 
WS  :   [ \r\t\u000C\n]+ -> skip;

LINE_COMMENT: '//' ~[\r\n]* '\r'? '\n' -> skip;