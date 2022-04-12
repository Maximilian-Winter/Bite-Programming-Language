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

parser grammar BITEParser;

options 
{
    tokenVocab = 'BITELexer';
}

/** The start rule; begin parsing here. */

program: module*;

module
    : moduleDeclaration
    ( importDirective | usingDirective )*
    declaration*                          
    EOF
    ; 

moduleDeclaration 
    : DeclareModule Identifier ( DotOperator Identifier )* SemicolonSeparator
    ;

importDirective 
    : ImportDirective Identifier ( DotOperator Identifier )* SemicolonSeparator
    ;

usingDirective 
    : UsingDirective Identifier ( DotOperator Identifier )* SemicolonSeparator
    ;

declaration 
    : classDeclaration
    | structDeclaration
    | externalFunctionDeclaration
    | functionDeclaration
    | classInstanceDeclaration
    | variableDeclaration
    | statement
    ;

classDeclaration : 
    ( privateModifier | publicModifier )? ( staticModifier | abstractModifier )? 
    DeclareClass Identifier (ColonOperator inheritance)? ( block | SemicolonSeparator )
    ;

structDeclaration : 
    ( privateModifier | publicModifier )? 
    DeclareStruct Identifier (block|SemicolonSeparator)
    ; 

externalFunctionDeclaration :
    ( privateModifier | publicModifier )? ( staticModifier | abstractModifier )? 
    ExternModifier? CallableModifier? DeclareFunction Identifier ( AsKeyword Identifier )? OpeningRoundBracket parameters? ClosingRoundBracket SemicolonSeparator
    ;

functionDeclaration :
    ( privateModifier | publicModifier )? ( staticModifier | abstractModifier )? 
    DeclareFunction Identifier OpeningRoundBracket parameters? ClosingRoundBracket ( block | SemicolonSeparator )
    ;

classInstanceDeclaration : 
    ( privateModifier | publicModifier )? ( staticModifier )? 
    DeclareVariable Identifier AssignOperator DeclareClassInstance Identifier ( DotOperator Identifier )* OpeningRoundBracket arguments? ClosingRoundBracket (SemicolonSeparator |  CURLY_L initializerExpression CURLY_R )
    | Identifier AssignOperator DeclareClassInstance Identifier ( DotOperator Identifier )* OpeningRoundBracket arguments? ClosingRoundBracket SemicolonSeparator
    ;

variableDeclaration 
    : ( privateModifier | publicModifier )? ( staticModifier )? 
    DeclareVariable Identifier ( ( AssignOperator exprStatement )? | SemicolonSeparator )
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
    | startThreadStatement
    | useThreadStatement
    | whileStatement
    | block 
    ;  
            
exprStatement 
    : expression SemicolonSeparator 
    ;
    
localVarDeclaration 
    : Identifier ( ( AssignOperator expression )? ) 
    ;

localVarInitializer 
    : DeclareVariable localVarDeclaration ( CommaSeparator localVarDeclaration )*
    ;

forInitializer 
    : localVarInitializer 
    | expression ( CommaSeparator expression )*
    ;

forIterator
    : expression ( CommaSeparator expression )* 
    ;

forStatement
    : DeclareForLoop OpeningRoundBracket ( forInitializer )?  
    SemicolonSeparator ( condition = expression )?  
    SemicolonSeparator ( forIterator )? 
    ClosingRoundBracket ( statement )?
    ;
                     
ifStatement         
    : ControlFlowIf OpeningRoundBracket expression ClosingRoundBracket trueStatement = statement
    ( ControlFlowElse falseStatement = statement )?
    ;
                     
returnStatement     
    : FunctionReturn expression? SemicolonSeparator
    ;

breakStatement      
    : Break SemicolonSeparator
    ;

usingStatement      
    : UsingDirective OpeningRoundBracket expression ClosingRoundBracket block
    ;

startThreadStatement
    : StartStatement ThreadStatement OpeningRoundBracket expression ClosingRoundBracket
    ;

useThreadStatement
    : UseStatement ThreadStatement OpeningRoundBracket expression ClosingRoundBracket
    ;
    
whileStatement
    : DeclareWhileLoop OpeningRoundBracket expression ClosingRoundBracket block 
    ;

block
    : CURLY_L declaration* CURLY_R 
    ;


expression     
    : assignment
    | lambdaExpression
    ;
    
assignment     
    : call ( AssignOperator | MinusAssignOperator 
        | PlusAssignOperator | MultiplyAssignOperator
        | DivideAssignOperator | ModuloAssignOperator 
        | BitwiseAndAssignOperator | BitwiseOrAssignOperator
        | BitwiseXorAssignOperator | BitwiseLeftShiftAssignOperator 
        | BitwiseRightShiftAssignOperator ) assignment
    | ternary
    ;
  
lambdaExpression
    : callArguments ReferenceOperator block
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
    : primary ( callArguments | DotOperator Identifier | elementAccess )*
    ;

arrayExpression
    : SquareBracketLeft expression ( CommaSeparator expression )* SquareBracketRight;

elementInitialization
    : Identifier ColonOperator expression;

dictionaryExpression
    : CURLY_L elementInitialization ( CommaSeparator elementInitialization )* CURLY_R;

primary
    : BooleanLiteral 
    | NullReference 
    | ThisReference 
    | IntegerLiteral 
    | FloatingLiteral 
    | OpeningRoundBracket expression ClosingRoundBracket
    | Identifier 
    | string
    | arrayExpression
    | dictionaryExpression
    ;

propertyInitialization  : Identifier AssignOperator expression;

privateModifier         : DeclarePrivate;
publicModifier          : DeclarePublic;
abstractModifier        : DeclareAbstract;
staticModifier          : DeclareStatic;                     
initializerExpression   : propertyInitialization ( CommaSeparator propertyInitialization )*;           
parameters              : parametersIdentifier ( CommaSeparator parametersIdentifier )* ;
arguments               : argumentExpression ( CommaSeparator argumentExpression )* ; 
inheritance             : Identifier ( CommaSeparator Identifier )* ;                  
callArguments           : OpeningRoundBracket arguments? ClosingRoundBracket;
elementAccess           : ( SquareBracketLeft elementIdentifier SquareBracketRight )+;    
elementIdentifier       : ( IntegerLiteral | string | call );   
argumentExpression      : ( ReferenceOperator )? expression;
parametersIdentifier    : Identifier;

string: DQUOTE stringPart* DQUOTE;

stringPart:
  TEXT
  | ESCAPE_SEQUENCE
  | BACKSLASH_PAREN expression CURLY_R
;