// Generated from c:\Language Dev 3\Bite Programming Language\Bite\Grammar\BITEParser.g4 by ANTLR 4.8
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class BITEParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		DeclareModule=1, DeclareClass=2, DeclareStruct=3, DeclareClassInstance=4, 
		DeclareFunction=5, DeclareVariable=6, DeclareGetter=7, DeclareSetter=8, 
		DeclareForLoop=9, DeclareWhileLoop=10, DeclareStatic=11, DeclareAbstract=12, 
		DeclarePublic=13, DeclarePrivate=14, ControlFlowIf=15, ControlFlowElse=16, 
		FunctionReturn=17, Break=18, NullReference=19, ThisReference=20, UsingDirective=21, 
		ImportDirective=22, AssignOperator=23, PlusAssignOperator=24, MinusAssignOperator=25, 
		MultiplyAssignOperator=26, DivideAssignOperator=27, ModuloAssignOperator=28, 
		BitwiseAndAssignOperator=29, BitwiseOrAssignOperator=30, BitwiseXorAssignOperator=31, 
		BitwiseLeftShiftAssignOperator=32, BitwiseRightShiftAssignOperator=33, 
		LogicalOrOperator=34, LogicalAndOperator=35, UnequalOperator=36, EqualOperator=37, 
		GreaterOperator=38, ShiftRightOperator=39, GreaterEqualOperator=40, SmallerOperator=41, 
		ShiftLeftOperator=42, SmallerEqualOperator=43, MinusOperator=44, MinusMinusOperator=45, 
		PlusOperator=46, PlusPlusOperator=47, DivideOperator=48, MultiplyOperator=49, 
		LogicalNegationOperator=50, DotOperator=51, QuestionMarkOperator=52, ColonOperator=53, 
		ReferenceOperator=54, ModuloOperator=55, ComplimentOperator=56, BitwiseAndOperator=57, 
		BitwiseXorOperator=58, BitwiseOrOperator=59, OpeningRoundBracket=60, ClosingRoundBracket=61, 
		OpeningCurlyBracket=62, ClosingCurlyBracket=63, SquarebracketLeft=64, 
		SquarebracketRight=65, CommaSeperator=66, SemicolonSeperator=67, DollarOperator=68, 
		BooleanLiteral=69, False_=70, True_=71, IntegerLiteral=72, FloatingLiteral=73, 
		StringLiteral=74, UnterminatedStringLiteral=75, DecimalLiteral=76, IDENTIFIER=77, 
		DQUOTE=78, LPAR=79, RPAR=80, TEXT=81, DOLLAR_PAREN=82, ESCAPE_SEQUENCE=83, 
		COMMENT=84, WS=85, LINE_COMMENT=86, Identifier=87;
	public static final int
		RULE_program = 0, RULE_module = 1, RULE_moduleDeclaration = 2, RULE_importDirective = 3, 
		RULE_usingDirective = 4, RULE_declaration = 5, RULE_classDeclaration = 6, 
		RULE_structDeclaration = 7, RULE_functionDeclaration = 8, RULE_classInstanceDeclaration = 9, 
		RULE_variableDeclaration = 10, RULE_statements = 11, RULE_statement = 12, 
		RULE_exprStatement = 13, RULE_localVarDeclaration = 14, RULE_localVarInitializer = 15, 
		RULE_forInitializer = 16, RULE_forIterator = 17, RULE_forStatement = 18, 
		RULE_ifStatement = 19, RULE_returnStatement = 20, RULE_breakStatement = 21, 
		RULE_usingStatement = 22, RULE_whileStatement = 23, RULE_block = 24, RULE_expression = 25, 
		RULE_assignment = 26, RULE_lambdaExpression = 27, RULE_ternary = 28, RULE_logicOr = 29, 
		RULE_logicAnd = 30, RULE_bitwiseOr = 31, RULE_bitwiseXor = 32, RULE_bitwiseAnd = 33, 
		RULE_equality = 34, RULE_relational = 35, RULE_shift = 36, RULE_additive = 37, 
		RULE_multiplicative = 38, RULE_unary = 39, RULE_call = 40, RULE_primary = 41, 
		RULE_privateModifier = 42, RULE_publicModifier = 43, RULE_abstractModifier = 44, 
		RULE_staticModifier = 45, RULE_parameters = 46, RULE_arguments = 47, RULE_inheritance = 48, 
		RULE_callArguments = 49, RULE_elementAccess = 50, RULE_elementIdentifier = 51, 
		RULE_argumentExpression = 52, RULE_parametersIdentifier = 53, RULE_stringContents = 54;
	private static String[] makeRuleNames() {
		return new String[] {
			"program", "module", "moduleDeclaration", "importDirective", "usingDirective", 
			"declaration", "classDeclaration", "structDeclaration", "functionDeclaration", 
			"classInstanceDeclaration", "variableDeclaration", "statements", "statement", 
			"exprStatement", "localVarDeclaration", "localVarInitializer", "forInitializer", 
			"forIterator", "forStatement", "ifStatement", "returnStatement", "breakStatement", 
			"usingStatement", "whileStatement", "block", "expression", "assignment", 
			"lambdaExpression", "ternary", "logicOr", "logicAnd", "bitwiseOr", "bitwiseXor", 
			"bitwiseAnd", "equality", "relational", "shift", "additive", "multiplicative", 
			"unary", "call", "primary", "privateModifier", "publicModifier", "abstractModifier", 
			"staticModifier", "parameters", "arguments", "inheritance", "callArguments", 
			"elementAccess", "elementIdentifier", "argumentExpression", "parametersIdentifier", 
			"stringContents"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'module'", "'class'", "'struct'", "'new'", "'function'", "'var'", 
			"'get'", "'set'", "'for'", "'while'", "'static'", "'abstract'", "'public'", 
			"'private'", "'if'", "'else'", "'return'", "'break'", "'null'", "'this'", 
			"'using'", "'import'", "'='", "'+='", "'-='", "'*='", "'/='", "'%='", 
			"'&='", "'|='", "'^='", "'<<='", "'>>='", "'||'", "'&&'", "'!='", "'=='", 
			"'>'", "'>>'", "'>='", "'<'", "'<<'", "'<='", "'-'", "'--'", "'+'", "'++'", 
			"'/'", "'*'", "'!'", "'.'", "'?'", "':'", "'->'", "'%'", "'~'", "'&'", 
			"'^'", "'|'", "'('", "')'", null, null, "'['", "']'", "','", "';'", "'$'", 
			null, "'false'", "'true'", null, null, null, null, null, null, null, 
			null, null, null, "'${'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "DeclareModule", "DeclareClass", "DeclareStruct", "DeclareClassInstance", 
			"DeclareFunction", "DeclareVariable", "DeclareGetter", "DeclareSetter", 
			"DeclareForLoop", "DeclareWhileLoop", "DeclareStatic", "DeclareAbstract", 
			"DeclarePublic", "DeclarePrivate", "ControlFlowIf", "ControlFlowElse", 
			"FunctionReturn", "Break", "NullReference", "ThisReference", "UsingDirective", 
			"ImportDirective", "AssignOperator", "PlusAssignOperator", "MinusAssignOperator", 
			"MultiplyAssignOperator", "DivideAssignOperator", "ModuloAssignOperator", 
			"BitwiseAndAssignOperator", "BitwiseOrAssignOperator", "BitwiseXorAssignOperator", 
			"BitwiseLeftShiftAssignOperator", "BitwiseRightShiftAssignOperator", 
			"LogicalOrOperator", "LogicalAndOperator", "UnequalOperator", "EqualOperator", 
			"GreaterOperator", "ShiftRightOperator", "GreaterEqualOperator", "SmallerOperator", 
			"ShiftLeftOperator", "SmallerEqualOperator", "MinusOperator", "MinusMinusOperator", 
			"PlusOperator", "PlusPlusOperator", "DivideOperator", "MultiplyOperator", 
			"LogicalNegationOperator", "DotOperator", "QuestionMarkOperator", "ColonOperator", 
			"ReferenceOperator", "ModuloOperator", "ComplimentOperator", "BitwiseAndOperator", 
			"BitwiseXorOperator", "BitwiseOrOperator", "OpeningRoundBracket", "ClosingRoundBracket", 
			"OpeningCurlyBracket", "ClosingCurlyBracket", "SquarebracketLeft", "SquarebracketRight", 
			"CommaSeperator", "SemicolonSeperator", "DollarOperator", "BooleanLiteral", 
			"False_", "True_", "IntegerLiteral", "FloatingLiteral", "StringLiteral", 
			"UnterminatedStringLiteral", "DecimalLiteral", "IDENTIFIER", "DQUOTE", 
			"LPAR", "RPAR", "TEXT", "DOLLAR_PAREN", "ESCAPE_SEQUENCE", "COMMENT", 
			"WS", "LINE_COMMENT", "Identifier"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "BITEParser.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public BITEParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	public static class ProgramContext extends ParserRuleContext {
		public List<ModuleContext> module() {
			return getRuleContexts(ModuleContext.class);
		}
		public ModuleContext module(int i) {
			return getRuleContext(ModuleContext.class,i);
		}
		public ProgramContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_program; }
	}

	public final ProgramContext program() throws RecognitionException {
		ProgramContext _localctx = new ProgramContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_program);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(113);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DeclareModule) {
				{
				{
				setState(110);
				module();
				}
				}
				setState(115);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ModuleContext extends ParserRuleContext {
		public ModuleDeclarationContext moduleDeclaration() {
			return getRuleContext(ModuleDeclarationContext.class,0);
		}
		public TerminalNode EOF() { return getToken(BITEParser.EOF, 0); }
		public List<ImportDirectiveContext> importDirective() {
			return getRuleContexts(ImportDirectiveContext.class);
		}
		public ImportDirectiveContext importDirective(int i) {
			return getRuleContext(ImportDirectiveContext.class,i);
		}
		public List<UsingDirectiveContext> usingDirective() {
			return getRuleContexts(UsingDirectiveContext.class);
		}
		public UsingDirectiveContext usingDirective(int i) {
			return getRuleContext(UsingDirectiveContext.class,i);
		}
		public List<DeclarationContext> declaration() {
			return getRuleContexts(DeclarationContext.class);
		}
		public DeclarationContext declaration(int i) {
			return getRuleContext(DeclarationContext.class,i);
		}
		public ModuleContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_module; }
	}

	public final ModuleContext module() throws RecognitionException {
		ModuleContext _localctx = new ModuleContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_module);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(116);
			moduleDeclaration();
			setState(121);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,2,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					setState(119);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case ImportDirective:
						{
						setState(117);
						importDirective();
						}
						break;
					case UsingDirective:
						{
						setState(118);
						usingDirective();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					} 
				}
				setState(123);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,2,_ctx);
			}
			setState(127);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DeclareClass) | (1L << DeclareStruct) | (1L << DeclareFunction) | (1L << DeclareVariable) | (1L << DeclareForLoop) | (1L << DeclareWhileLoop) | (1L << DeclareStatic) | (1L << DeclareAbstract) | (1L << DeclarePublic) | (1L << DeclarePrivate) | (1L << ControlFlowIf) | (1L << FunctionReturn) | (1L << Break) | (1L << NullReference) | (1L << ThisReference) | (1L << UsingDirective) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket) | (1L << OpeningCurlyBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				{
				setState(124);
				declaration();
				}
				}
				setState(129);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(130);
			match(EOF);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ModuleDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareModule() { return getToken(BITEParser.DeclareModule, 0); }
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public List<TerminalNode> DotOperator() { return getTokens(BITEParser.DotOperator); }
		public TerminalNode DotOperator(int i) {
			return getToken(BITEParser.DotOperator, i);
		}
		public ModuleDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_moduleDeclaration; }
	}

	public final ModuleDeclarationContext moduleDeclaration() throws RecognitionException {
		ModuleDeclarationContext _localctx = new ModuleDeclarationContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_moduleDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(132);
			match(DeclareModule);
			setState(133);
			match(Identifier);
			setState(138);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DotOperator) {
				{
				{
				setState(134);
				match(DotOperator);
				setState(135);
				match(Identifier);
				}
				}
				setState(140);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(141);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ImportDirectiveContext extends ParserRuleContext {
		public TerminalNode ImportDirective() { return getToken(BITEParser.ImportDirective, 0); }
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public List<TerminalNode> DotOperator() { return getTokens(BITEParser.DotOperator); }
		public TerminalNode DotOperator(int i) {
			return getToken(BITEParser.DotOperator, i);
		}
		public ImportDirectiveContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_importDirective; }
	}

	public final ImportDirectiveContext importDirective() throws RecognitionException {
		ImportDirectiveContext _localctx = new ImportDirectiveContext(_ctx, getState());
		enterRule(_localctx, 6, RULE_importDirective);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(143);
			match(ImportDirective);
			setState(144);
			match(Identifier);
			setState(149);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DotOperator) {
				{
				{
				setState(145);
				match(DotOperator);
				setState(146);
				match(Identifier);
				}
				}
				setState(151);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(152);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class UsingDirectiveContext extends ParserRuleContext {
		public TerminalNode UsingDirective() { return getToken(BITEParser.UsingDirective, 0); }
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public List<TerminalNode> DotOperator() { return getTokens(BITEParser.DotOperator); }
		public TerminalNode DotOperator(int i) {
			return getToken(BITEParser.DotOperator, i);
		}
		public UsingDirectiveContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_usingDirective; }
	}

	public final UsingDirectiveContext usingDirective() throws RecognitionException {
		UsingDirectiveContext _localctx = new UsingDirectiveContext(_ctx, getState());
		enterRule(_localctx, 8, RULE_usingDirective);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(154);
			match(UsingDirective);
			setState(155);
			match(Identifier);
			setState(160);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==DotOperator) {
				{
				{
				setState(156);
				match(DotOperator);
				setState(157);
				match(Identifier);
				}
				}
				setState(162);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(163);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class DeclarationContext extends ParserRuleContext {
		public ClassDeclarationContext classDeclaration() {
			return getRuleContext(ClassDeclarationContext.class,0);
		}
		public StructDeclarationContext structDeclaration() {
			return getRuleContext(StructDeclarationContext.class,0);
		}
		public FunctionDeclarationContext functionDeclaration() {
			return getRuleContext(FunctionDeclarationContext.class,0);
		}
		public ClassInstanceDeclarationContext classInstanceDeclaration() {
			return getRuleContext(ClassInstanceDeclarationContext.class,0);
		}
		public VariableDeclarationContext variableDeclaration() {
			return getRuleContext(VariableDeclarationContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public DeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_declaration; }
	}

	public final DeclarationContext declaration() throws RecognitionException {
		DeclarationContext _localctx = new DeclarationContext(_ctx, getState());
		enterRule(_localctx, 10, RULE_declaration);
		try {
			setState(171);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,7,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(165);
				classDeclaration();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(166);
				structDeclaration();
				}
				break;
			case 3:
				enterOuterAlt(_localctx, 3);
				{
				setState(167);
				functionDeclaration();
				}
				break;
			case 4:
				enterOuterAlt(_localctx, 4);
				{
				setState(168);
				classInstanceDeclaration();
				}
				break;
			case 5:
				enterOuterAlt(_localctx, 5);
				{
				setState(169);
				variableDeclaration();
				}
				break;
			case 6:
				enterOuterAlt(_localctx, 6);
				{
				setState(170);
				statement();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ClassDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareClass() { return getToken(BITEParser.DeclareClass, 0); }
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public PrivateModifierContext privateModifier() {
			return getRuleContext(PrivateModifierContext.class,0);
		}
		public PublicModifierContext publicModifier() {
			return getRuleContext(PublicModifierContext.class,0);
		}
		public StaticModifierContext staticModifier() {
			return getRuleContext(StaticModifierContext.class,0);
		}
		public AbstractModifierContext abstractModifier() {
			return getRuleContext(AbstractModifierContext.class,0);
		}
		public TerminalNode ColonOperator() { return getToken(BITEParser.ColonOperator, 0); }
		public InheritanceContext inheritance() {
			return getRuleContext(InheritanceContext.class,0);
		}
		public ClassDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_classDeclaration; }
	}

	public final ClassDeclarationContext classDeclaration() throws RecognitionException {
		ClassDeclarationContext _localctx = new ClassDeclarationContext(_ctx, getState());
		enterRule(_localctx, 12, RULE_classDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(175);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclarePrivate:
				{
				setState(173);
				privateModifier();
				}
				break;
			case DeclarePublic:
				{
				setState(174);
				publicModifier();
				}
				break;
			case DeclareClass:
			case DeclareStatic:
			case DeclareAbstract:
				break;
			default:
				break;
			}
			setState(179);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclareStatic:
				{
				setState(177);
				staticModifier();
				}
				break;
			case DeclareAbstract:
				{
				setState(178);
				abstractModifier();
				}
				break;
			case DeclareClass:
				break;
			default:
				break;
			}
			setState(181);
			match(DeclareClass);
			setState(182);
			match(Identifier);
			setState(185);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ColonOperator) {
				{
				setState(183);
				match(ColonOperator);
				setState(184);
				inheritance();
				}
			}

			setState(189);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case OpeningCurlyBracket:
				{
				setState(187);
				block();
				}
				break;
			case SemicolonSeperator:
				{
				setState(188);
				match(SemicolonSeperator);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StructDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareStruct() { return getToken(BITEParser.DeclareStruct, 0); }
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public PrivateModifierContext privateModifier() {
			return getRuleContext(PrivateModifierContext.class,0);
		}
		public PublicModifierContext publicModifier() {
			return getRuleContext(PublicModifierContext.class,0);
		}
		public StructDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_structDeclaration; }
	}

	public final StructDeclarationContext structDeclaration() throws RecognitionException {
		StructDeclarationContext _localctx = new StructDeclarationContext(_ctx, getState());
		enterRule(_localctx, 14, RULE_structDeclaration);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(193);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclarePrivate:
				{
				setState(191);
				privateModifier();
				}
				break;
			case DeclarePublic:
				{
				setState(192);
				publicModifier();
				}
				break;
			case DeclareStruct:
				break;
			default:
				break;
			}
			setState(195);
			match(DeclareStruct);
			setState(196);
			match(Identifier);
			setState(199);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case OpeningCurlyBracket:
				{
				setState(197);
				block();
				}
				break;
			case SemicolonSeperator:
				{
				setState(198);
				match(SemicolonSeperator);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class FunctionDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareFunction() { return getToken(BITEParser.DeclareFunction, 0); }
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public PrivateModifierContext privateModifier() {
			return getRuleContext(PrivateModifierContext.class,0);
		}
		public PublicModifierContext publicModifier() {
			return getRuleContext(PublicModifierContext.class,0);
		}
		public StaticModifierContext staticModifier() {
			return getRuleContext(StaticModifierContext.class,0);
		}
		public AbstractModifierContext abstractModifier() {
			return getRuleContext(AbstractModifierContext.class,0);
		}
		public ParametersContext parameters() {
			return getRuleContext(ParametersContext.class,0);
		}
		public FunctionDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_functionDeclaration; }
	}

	public final FunctionDeclarationContext functionDeclaration() throws RecognitionException {
		FunctionDeclarationContext _localctx = new FunctionDeclarationContext(_ctx, getState());
		enterRule(_localctx, 16, RULE_functionDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(203);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclarePrivate:
				{
				setState(201);
				privateModifier();
				}
				break;
			case DeclarePublic:
				{
				setState(202);
				publicModifier();
				}
				break;
			case DeclareFunction:
			case DeclareStatic:
			case DeclareAbstract:
				break;
			default:
				break;
			}
			setState(207);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclareStatic:
				{
				setState(205);
				staticModifier();
				}
				break;
			case DeclareAbstract:
				{
				setState(206);
				abstractModifier();
				}
				break;
			case DeclareFunction:
				break;
			default:
				break;
			}
			setState(209);
			match(DeclareFunction);
			setState(210);
			match(Identifier);
			setState(211);
			match(OpeningRoundBracket);
			setState(213);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==Identifier) {
				{
				setState(212);
				parameters();
				}
			}

			setState(215);
			match(ClosingRoundBracket);
			setState(218);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case OpeningCurlyBracket:
				{
				setState(216);
				block();
				}
				break;
			case SemicolonSeperator:
				{
				setState(217);
				match(SemicolonSeperator);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ClassInstanceDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareVariable() { return getToken(BITEParser.DeclareVariable, 0); }
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public TerminalNode AssignOperator() { return getToken(BITEParser.AssignOperator, 0); }
		public TerminalNode DeclareClassInstance() { return getToken(BITEParser.DeclareClassInstance, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public PrivateModifierContext privateModifier() {
			return getRuleContext(PrivateModifierContext.class,0);
		}
		public PublicModifierContext publicModifier() {
			return getRuleContext(PublicModifierContext.class,0);
		}
		public StaticModifierContext staticModifier() {
			return getRuleContext(StaticModifierContext.class,0);
		}
		public List<TerminalNode> DotOperator() { return getTokens(BITEParser.DotOperator); }
		public TerminalNode DotOperator(int i) {
			return getToken(BITEParser.DotOperator, i);
		}
		public ArgumentsContext arguments() {
			return getRuleContext(ArgumentsContext.class,0);
		}
		public ClassInstanceDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_classInstanceDeclaration; }
	}

	public final ClassInstanceDeclarationContext classInstanceDeclaration() throws RecognitionException {
		ClassInstanceDeclarationContext _localctx = new ClassInstanceDeclarationContext(_ctx, getState());
		enterRule(_localctx, 18, RULE_classInstanceDeclaration);
		int _la;
		try {
			setState(262);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclareVariable:
			case DeclareStatic:
			case DeclarePublic:
			case DeclarePrivate:
				enterOuterAlt(_localctx, 1);
				{
				setState(222);
				_errHandler.sync(this);
				switch (_input.LA(1)) {
				case DeclarePrivate:
					{
					setState(220);
					privateModifier();
					}
					break;
				case DeclarePublic:
					{
					setState(221);
					publicModifier();
					}
					break;
				case DeclareVariable:
				case DeclareStatic:
					break;
				default:
					break;
				}
				setState(225);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==DeclareStatic) {
					{
					setState(224);
					staticModifier();
					}
				}

				setState(227);
				match(DeclareVariable);
				setState(228);
				match(Identifier);
				setState(229);
				match(AssignOperator);
				setState(230);
				match(DeclareClassInstance);
				setState(231);
				match(Identifier);
				setState(236);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==DotOperator) {
					{
					{
					setState(232);
					match(DotOperator);
					setState(233);
					match(Identifier);
					}
					}
					setState(238);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(239);
				match(OpeningRoundBracket);
				setState(241);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ReferenceOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
					{
					setState(240);
					arguments();
					}
				}

				setState(243);
				match(ClosingRoundBracket);
				setState(244);
				match(SemicolonSeperator);
				}
				break;
			case Identifier:
				enterOuterAlt(_localctx, 2);
				{
				setState(245);
				match(Identifier);
				setState(246);
				match(AssignOperator);
				setState(247);
				match(DeclareClassInstance);
				setState(248);
				match(Identifier);
				setState(253);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==DotOperator) {
					{
					{
					setState(249);
					match(DotOperator);
					setState(250);
					match(Identifier);
					}
					}
					setState(255);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				setState(256);
				match(OpeningRoundBracket);
				setState(258);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ReferenceOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
					{
					setState(257);
					arguments();
					}
				}

				setState(260);
				match(ClosingRoundBracket);
				setState(261);
				match(SemicolonSeperator);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class VariableDeclarationContext extends ParserRuleContext {
		public TerminalNode DeclareVariable() { return getToken(BITEParser.DeclareVariable, 0); }
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public PrivateModifierContext privateModifier() {
			return getRuleContext(PrivateModifierContext.class,0);
		}
		public PublicModifierContext publicModifier() {
			return getRuleContext(PublicModifierContext.class,0);
		}
		public StaticModifierContext staticModifier() {
			return getRuleContext(StaticModifierContext.class,0);
		}
		public TerminalNode AssignOperator() { return getToken(BITEParser.AssignOperator, 0); }
		public ExprStatementContext exprStatement() {
			return getRuleContext(ExprStatementContext.class,0);
		}
		public VariableDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_variableDeclaration; }
	}

	public final VariableDeclarationContext variableDeclaration() throws RecognitionException {
		VariableDeclarationContext _localctx = new VariableDeclarationContext(_ctx, getState());
		enterRule(_localctx, 20, RULE_variableDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(266);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclarePrivate:
				{
				setState(264);
				privateModifier();
				}
				break;
			case DeclarePublic:
				{
				setState(265);
				publicModifier();
				}
				break;
			case DeclareVariable:
			case DeclareStatic:
				break;
			default:
				break;
			}
			setState(269);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==DeclareStatic) {
				{
				setState(268);
				staticModifier();
				}
			}

			setState(271);
			match(DeclareVariable);
			setState(272);
			match(Identifier);
			setState(278);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case EOF:
			case DeclareClass:
			case DeclareStruct:
			case DeclareFunction:
			case DeclareVariable:
			case DeclareForLoop:
			case DeclareWhileLoop:
			case DeclareStatic:
			case DeclareAbstract:
			case DeclarePublic:
			case DeclarePrivate:
			case ControlFlowIf:
			case FunctionReturn:
			case Break:
			case NullReference:
			case ThisReference:
			case UsingDirective:
			case AssignOperator:
			case MinusOperator:
			case MinusMinusOperator:
			case PlusOperator:
			case PlusPlusOperator:
			case LogicalNegationOperator:
			case ComplimentOperator:
			case OpeningRoundBracket:
			case OpeningCurlyBracket:
			case ClosingCurlyBracket:
			case BooleanLiteral:
			case IntegerLiteral:
			case FloatingLiteral:
			case StringLiteral:
			case Identifier:
				{
				setState(275);
				_errHandler.sync(this);
				_la = _input.LA(1);
				if (_la==AssignOperator) {
					{
					setState(273);
					match(AssignOperator);
					setState(274);
					exprStatement();
					}
				}

				}
				break;
			case SemicolonSeperator:
				{
				setState(277);
				match(SemicolonSeperator);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StatementsContext extends ParserRuleContext {
		public List<DeclarationContext> declaration() {
			return getRuleContexts(DeclarationContext.class);
		}
		public DeclarationContext declaration(int i) {
			return getRuleContext(DeclarationContext.class,i);
		}
		public StatementsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statements; }
	}

	public final StatementsContext statements() throws RecognitionException {
		StatementsContext _localctx = new StatementsContext(_ctx, getState());
		enterRule(_localctx, 22, RULE_statements);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(280);
			declaration();
			setState(284);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DeclareClass) | (1L << DeclareStruct) | (1L << DeclareFunction) | (1L << DeclareVariable) | (1L << DeclareForLoop) | (1L << DeclareWhileLoop) | (1L << DeclareStatic) | (1L << DeclareAbstract) | (1L << DeclarePublic) | (1L << DeclarePrivate) | (1L << ControlFlowIf) | (1L << FunctionReturn) | (1L << Break) | (1L << NullReference) | (1L << ThisReference) | (1L << UsingDirective) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket) | (1L << OpeningCurlyBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				{
				setState(281);
				declaration();
				}
				}
				setState(286);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StatementContext extends ParserRuleContext {
		public ExprStatementContext exprStatement() {
			return getRuleContext(ExprStatementContext.class,0);
		}
		public ForStatementContext forStatement() {
			return getRuleContext(ForStatementContext.class,0);
		}
		public IfStatementContext ifStatement() {
			return getRuleContext(IfStatementContext.class,0);
		}
		public ReturnStatementContext returnStatement() {
			return getRuleContext(ReturnStatementContext.class,0);
		}
		public BreakStatementContext breakStatement() {
			return getRuleContext(BreakStatementContext.class,0);
		}
		public UsingStatementContext usingStatement() {
			return getRuleContext(UsingStatementContext.class,0);
		}
		public WhileStatementContext whileStatement() {
			return getRuleContext(WhileStatementContext.class,0);
		}
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public StatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement; }
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 24, RULE_statement);
		try {
			setState(295);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case NullReference:
			case ThisReference:
			case MinusOperator:
			case MinusMinusOperator:
			case PlusOperator:
			case PlusPlusOperator:
			case LogicalNegationOperator:
			case ComplimentOperator:
			case OpeningRoundBracket:
			case BooleanLiteral:
			case IntegerLiteral:
			case FloatingLiteral:
			case StringLiteral:
			case Identifier:
				enterOuterAlt(_localctx, 1);
				{
				setState(287);
				exprStatement();
				}
				break;
			case DeclareForLoop:
				enterOuterAlt(_localctx, 2);
				{
				setState(288);
				forStatement();
				}
				break;
			case ControlFlowIf:
				enterOuterAlt(_localctx, 3);
				{
				setState(289);
				ifStatement();
				}
				break;
			case FunctionReturn:
				enterOuterAlt(_localctx, 4);
				{
				setState(290);
				returnStatement();
				}
				break;
			case Break:
				enterOuterAlt(_localctx, 5);
				{
				setState(291);
				breakStatement();
				}
				break;
			case UsingDirective:
				enterOuterAlt(_localctx, 6);
				{
				setState(292);
				usingStatement();
				}
				break;
			case DeclareWhileLoop:
				enterOuterAlt(_localctx, 7);
				{
				setState(293);
				whileStatement();
				}
				break;
			case OpeningCurlyBracket:
				enterOuterAlt(_localctx, 8);
				{
				setState(294);
				block();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExprStatementContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public ExprStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_exprStatement; }
	}

	public final ExprStatementContext exprStatement() throws RecognitionException {
		ExprStatementContext _localctx = new ExprStatementContext(_ctx, getState());
		enterRule(_localctx, 26, RULE_exprStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(297);
			expression();
			setState(298);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LocalVarDeclarationContext extends ParserRuleContext {
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public TerminalNode AssignOperator() { return getToken(BITEParser.AssignOperator, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public LocalVarDeclarationContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_localVarDeclaration; }
	}

	public final LocalVarDeclarationContext localVarDeclaration() throws RecognitionException {
		LocalVarDeclarationContext _localctx = new LocalVarDeclarationContext(_ctx, getState());
		enterRule(_localctx, 28, RULE_localVarDeclaration);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(300);
			match(Identifier);
			{
			setState(303);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==AssignOperator) {
				{
				setState(301);
				match(AssignOperator);
				setState(302);
				expression();
				}
			}

			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LocalVarInitializerContext extends ParserRuleContext {
		public TerminalNode DeclareVariable() { return getToken(BITEParser.DeclareVariable, 0); }
		public List<LocalVarDeclarationContext> localVarDeclaration() {
			return getRuleContexts(LocalVarDeclarationContext.class);
		}
		public LocalVarDeclarationContext localVarDeclaration(int i) {
			return getRuleContext(LocalVarDeclarationContext.class,i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public LocalVarInitializerContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_localVarInitializer; }
	}

	public final LocalVarInitializerContext localVarInitializer() throws RecognitionException {
		LocalVarInitializerContext _localctx = new LocalVarInitializerContext(_ctx, getState());
		enterRule(_localctx, 30, RULE_localVarInitializer);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(305);
			match(DeclareVariable);
			setState(306);
			localVarDeclaration();
			setState(311);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CommaSeperator) {
				{
				{
				setState(307);
				match(CommaSeperator);
				setState(308);
				localVarDeclaration();
				}
				}
				setState(313);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ForInitializerContext extends ParserRuleContext {
		public LocalVarInitializerContext localVarInitializer() {
			return getRuleContext(LocalVarInitializerContext.class,0);
		}
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public ForInitializerContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_forInitializer; }
	}

	public final ForInitializerContext forInitializer() throws RecognitionException {
		ForInitializerContext _localctx = new ForInitializerContext(_ctx, getState());
		enterRule(_localctx, 32, RULE_forInitializer);
		int _la;
		try {
			setState(323);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case DeclareVariable:
				enterOuterAlt(_localctx, 1);
				{
				setState(314);
				localVarInitializer();
				}
				break;
			case NullReference:
			case ThisReference:
			case MinusOperator:
			case MinusMinusOperator:
			case PlusOperator:
			case PlusPlusOperator:
			case LogicalNegationOperator:
			case ComplimentOperator:
			case OpeningRoundBracket:
			case BooleanLiteral:
			case IntegerLiteral:
			case FloatingLiteral:
			case StringLiteral:
			case Identifier:
				enterOuterAlt(_localctx, 2);
				{
				setState(315);
				expression();
				setState(320);
				_errHandler.sync(this);
				_la = _input.LA(1);
				while (_la==CommaSeperator) {
					{
					{
					setState(316);
					match(CommaSeperator);
					setState(317);
					expression();
					}
					}
					setState(322);
					_errHandler.sync(this);
					_la = _input.LA(1);
				}
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ForIteratorContext extends ParserRuleContext {
		public List<ExpressionContext> expression() {
			return getRuleContexts(ExpressionContext.class);
		}
		public ExpressionContext expression(int i) {
			return getRuleContext(ExpressionContext.class,i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public ForIteratorContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_forIterator; }
	}

	public final ForIteratorContext forIterator() throws RecognitionException {
		ForIteratorContext _localctx = new ForIteratorContext(_ctx, getState());
		enterRule(_localctx, 34, RULE_forIterator);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(325);
			expression();
			setState(330);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CommaSeperator) {
				{
				{
				setState(326);
				match(CommaSeperator);
				setState(327);
				expression();
				}
				}
				setState(332);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ForStatementContext extends ParserRuleContext {
		public ExpressionContext condition;
		public TerminalNode DeclareForLoop() { return getToken(BITEParser.DeclareForLoop, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public List<TerminalNode> SemicolonSeperator() { return getTokens(BITEParser.SemicolonSeperator); }
		public TerminalNode SemicolonSeperator(int i) {
			return getToken(BITEParser.SemicolonSeperator, i);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public ForInitializerContext forInitializer() {
			return getRuleContext(ForInitializerContext.class,0);
		}
		public ForIteratorContext forIterator() {
			return getRuleContext(ForIteratorContext.class,0);
		}
		public StatementContext statement() {
			return getRuleContext(StatementContext.class,0);
		}
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public ForStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_forStatement; }
	}

	public final ForStatementContext forStatement() throws RecognitionException {
		ForStatementContext _localctx = new ForStatementContext(_ctx, getState());
		enterRule(_localctx, 36, RULE_forStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(333);
			match(DeclareForLoop);
			setState(334);
			match(OpeningRoundBracket);
			setState(336);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DeclareVariable) | (1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				setState(335);
				forInitializer();
				}
			}

			setState(338);
			match(SemicolonSeperator);
			setState(340);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				setState(339);
				((ForStatementContext)_localctx).condition = expression();
				}
			}

			setState(342);
			match(SemicolonSeperator);
			setState(344);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				setState(343);
				forIterator();
				}
			}

			setState(346);
			match(ClosingRoundBracket);
			setState(348);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,39,_ctx) ) {
			case 1:
				{
				setState(347);
				statement();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class IfStatementContext extends ParserRuleContext {
		public StatementContext trueStatement;
		public StatementContext falseStatement;
		public TerminalNode ControlFlowIf() { return getToken(BITEParser.ControlFlowIf, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public TerminalNode ControlFlowElse() { return getToken(BITEParser.ControlFlowElse, 0); }
		public IfStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_ifStatement; }
	}

	public final IfStatementContext ifStatement() throws RecognitionException {
		IfStatementContext _localctx = new IfStatementContext(_ctx, getState());
		enterRule(_localctx, 38, RULE_ifStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(350);
			match(ControlFlowIf);
			setState(351);
			match(OpeningRoundBracket);
			setState(352);
			expression();
			setState(353);
			match(ClosingRoundBracket);
			setState(354);
			((IfStatementContext)_localctx).trueStatement = statement();
			setState(357);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,40,_ctx) ) {
			case 1:
				{
				setState(355);
				match(ControlFlowElse);
				setState(356);
				((IfStatementContext)_localctx).falseStatement = statement();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ReturnStatementContext extends ParserRuleContext {
		public TerminalNode FunctionReturn() { return getToken(BITEParser.FunctionReturn, 0); }
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public ReturnStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_returnStatement; }
	}

	public final ReturnStatementContext returnStatement() throws RecognitionException {
		ReturnStatementContext _localctx = new ReturnStatementContext(_ctx, getState());
		enterRule(_localctx, 40, RULE_returnStatement);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(359);
			match(FunctionReturn);
			setState(361);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				setState(360);
				expression();
				}
			}

			setState(363);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BreakStatementContext extends ParserRuleContext {
		public TerminalNode Break() { return getToken(BITEParser.Break, 0); }
		public TerminalNode SemicolonSeperator() { return getToken(BITEParser.SemicolonSeperator, 0); }
		public BreakStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_breakStatement; }
	}

	public final BreakStatementContext breakStatement() throws RecognitionException {
		BreakStatementContext _localctx = new BreakStatementContext(_ctx, getState());
		enterRule(_localctx, 42, RULE_breakStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(365);
			match(Break);
			setState(366);
			match(SemicolonSeperator);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class UsingStatementContext extends ParserRuleContext {
		public TerminalNode UsingDirective() { return getToken(BITEParser.UsingDirective, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public UsingStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_usingStatement; }
	}

	public final UsingStatementContext usingStatement() throws RecognitionException {
		UsingStatementContext _localctx = new UsingStatementContext(_ctx, getState());
		enterRule(_localctx, 44, RULE_usingStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(368);
			match(UsingDirective);
			setState(369);
			match(OpeningRoundBracket);
			setState(370);
			expression();
			setState(371);
			match(ClosingRoundBracket);
			setState(372);
			block();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class WhileStatementContext extends ParserRuleContext {
		public TerminalNode DeclareWhileLoop() { return getToken(BITEParser.DeclareWhileLoop, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public WhileStatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_whileStatement; }
	}

	public final WhileStatementContext whileStatement() throws RecognitionException {
		WhileStatementContext _localctx = new WhileStatementContext(_ctx, getState());
		enterRule(_localctx, 46, RULE_whileStatement);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(374);
			match(DeclareWhileLoop);
			setState(375);
			match(OpeningRoundBracket);
			setState(376);
			expression();
			setState(377);
			match(ClosingRoundBracket);
			setState(378);
			block();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BlockContext extends ParserRuleContext {
		public TerminalNode OpeningCurlyBracket() { return getToken(BITEParser.OpeningCurlyBracket, 0); }
		public TerminalNode ClosingCurlyBracket() { return getToken(BITEParser.ClosingCurlyBracket, 0); }
		public List<DeclarationContext> declaration() {
			return getRuleContexts(DeclarationContext.class);
		}
		public DeclarationContext declaration(int i) {
			return getRuleContext(DeclarationContext.class,i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_block; }
	}

	public final BlockContext block() throws RecognitionException {
		BlockContext _localctx = new BlockContext(_ctx, getState());
		enterRule(_localctx, 48, RULE_block);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(380);
			match(OpeningCurlyBracket);
			setState(384);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DeclareClass) | (1L << DeclareStruct) | (1L << DeclareFunction) | (1L << DeclareVariable) | (1L << DeclareForLoop) | (1L << DeclareWhileLoop) | (1L << DeclareStatic) | (1L << DeclareAbstract) | (1L << DeclarePublic) | (1L << DeclarePrivate) | (1L << ControlFlowIf) | (1L << FunctionReturn) | (1L << Break) | (1L << NullReference) | (1L << ThisReference) | (1L << UsingDirective) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket) | (1L << OpeningCurlyBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				{
				setState(381);
				declaration();
				}
				}
				setState(386);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			setState(387);
			match(ClosingCurlyBracket);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ExpressionContext extends ParserRuleContext {
		public AssignmentContext assignment() {
			return getRuleContext(AssignmentContext.class,0);
		}
		public LambdaExpressionContext lambdaExpression() {
			return getRuleContext(LambdaExpressionContext.class,0);
		}
		public ExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_expression; }
	}

	public final ExpressionContext expression() throws RecognitionException {
		ExpressionContext _localctx = new ExpressionContext(_ctx, getState());
		enterRule(_localctx, 50, RULE_expression);
		try {
			setState(391);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,43,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(389);
				assignment();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(390);
				lambdaExpression();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AssignmentContext extends ParserRuleContext {
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public AssignmentContext assignment() {
			return getRuleContext(AssignmentContext.class,0);
		}
		public TerminalNode AssignOperator() { return getToken(BITEParser.AssignOperator, 0); }
		public TerminalNode MinusAssignOperator() { return getToken(BITEParser.MinusAssignOperator, 0); }
		public TerminalNode PlusAssignOperator() { return getToken(BITEParser.PlusAssignOperator, 0); }
		public TerminalNode MultiplyAssignOperator() { return getToken(BITEParser.MultiplyAssignOperator, 0); }
		public TerminalNode DivideAssignOperator() { return getToken(BITEParser.DivideAssignOperator, 0); }
		public TerminalNode ModuloAssignOperator() { return getToken(BITEParser.ModuloAssignOperator, 0); }
		public TerminalNode BitwiseAndAssignOperator() { return getToken(BITEParser.BitwiseAndAssignOperator, 0); }
		public TerminalNode BitwiseOrAssignOperator() { return getToken(BITEParser.BitwiseOrAssignOperator, 0); }
		public TerminalNode BitwiseXorAssignOperator() { return getToken(BITEParser.BitwiseXorAssignOperator, 0); }
		public TerminalNode BitwiseLeftShiftAssignOperator() { return getToken(BITEParser.BitwiseLeftShiftAssignOperator, 0); }
		public TerminalNode BitwiseRightShiftAssignOperator() { return getToken(BITEParser.BitwiseRightShiftAssignOperator, 0); }
		public TernaryContext ternary() {
			return getRuleContext(TernaryContext.class,0);
		}
		public AssignmentContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_assignment; }
	}

	public final AssignmentContext assignment() throws RecognitionException {
		AssignmentContext _localctx = new AssignmentContext(_ctx, getState());
		enterRule(_localctx, 52, RULE_assignment);
		int _la;
		try {
			setState(398);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,44,_ctx) ) {
			case 1:
				enterOuterAlt(_localctx, 1);
				{
				setState(393);
				call();
				setState(394);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << AssignOperator) | (1L << PlusAssignOperator) | (1L << MinusAssignOperator) | (1L << MultiplyAssignOperator) | (1L << DivideAssignOperator) | (1L << ModuloAssignOperator) | (1L << BitwiseAndAssignOperator) | (1L << BitwiseOrAssignOperator) | (1L << BitwiseXorAssignOperator) | (1L << BitwiseLeftShiftAssignOperator) | (1L << BitwiseRightShiftAssignOperator))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(395);
				assignment();
				}
				break;
			case 2:
				enterOuterAlt(_localctx, 2);
				{
				setState(397);
				ternary();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LambdaExpressionContext extends ParserRuleContext {
		public CallArgumentsContext callArguments() {
			return getRuleContext(CallArgumentsContext.class,0);
		}
		public TerminalNode ReferenceOperator() { return getToken(BITEParser.ReferenceOperator, 0); }
		public BlockContext block() {
			return getRuleContext(BlockContext.class,0);
		}
		public LambdaExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_lambdaExpression; }
	}

	public final LambdaExpressionContext lambdaExpression() throws RecognitionException {
		LambdaExpressionContext _localctx = new LambdaExpressionContext(_ctx, getState());
		enterRule(_localctx, 54, RULE_lambdaExpression);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(400);
			callArguments();
			setState(401);
			match(ReferenceOperator);
			setState(402);
			block();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class TernaryContext extends ParserRuleContext {
		public List<LogicOrContext> logicOr() {
			return getRuleContexts(LogicOrContext.class);
		}
		public LogicOrContext logicOr(int i) {
			return getRuleContext(LogicOrContext.class,i);
		}
		public List<TerminalNode> QuestionMarkOperator() { return getTokens(BITEParser.QuestionMarkOperator); }
		public TerminalNode QuestionMarkOperator(int i) {
			return getToken(BITEParser.QuestionMarkOperator, i);
		}
		public List<TerminalNode> ColonOperator() { return getTokens(BITEParser.ColonOperator); }
		public TerminalNode ColonOperator(int i) {
			return getToken(BITEParser.ColonOperator, i);
		}
		public TernaryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_ternary; }
	}

	public final TernaryContext ternary() throws RecognitionException {
		TernaryContext _localctx = new TernaryContext(_ctx, getState());
		enterRule(_localctx, 56, RULE_ternary);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(404);
			logicOr();
			setState(412);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==QuestionMarkOperator) {
				{
				{
				setState(405);
				match(QuestionMarkOperator);
				setState(406);
				logicOr();
				setState(407);
				match(ColonOperator);
				setState(408);
				logicOr();
				}
				}
				setState(414);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LogicOrContext extends ParserRuleContext {
		public List<LogicAndContext> logicAnd() {
			return getRuleContexts(LogicAndContext.class);
		}
		public LogicAndContext logicAnd(int i) {
			return getRuleContext(LogicAndContext.class,i);
		}
		public List<TerminalNode> LogicalOrOperator() { return getTokens(BITEParser.LogicalOrOperator); }
		public TerminalNode LogicalOrOperator(int i) {
			return getToken(BITEParser.LogicalOrOperator, i);
		}
		public LogicOrContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_logicOr; }
	}

	public final LogicOrContext logicOr() throws RecognitionException {
		LogicOrContext _localctx = new LogicOrContext(_ctx, getState());
		enterRule(_localctx, 58, RULE_logicOr);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(415);
			logicAnd();
			setState(420);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LogicalOrOperator) {
				{
				{
				setState(416);
				match(LogicalOrOperator);
				setState(417);
				logicAnd();
				}
				}
				setState(422);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class LogicAndContext extends ParserRuleContext {
		public List<BitwiseOrContext> bitwiseOr() {
			return getRuleContexts(BitwiseOrContext.class);
		}
		public BitwiseOrContext bitwiseOr(int i) {
			return getRuleContext(BitwiseOrContext.class,i);
		}
		public List<TerminalNode> LogicalAndOperator() { return getTokens(BITEParser.LogicalAndOperator); }
		public TerminalNode LogicalAndOperator(int i) {
			return getToken(BITEParser.LogicalAndOperator, i);
		}
		public LogicAndContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_logicAnd; }
	}

	public final LogicAndContext logicAnd() throws RecognitionException {
		LogicAndContext _localctx = new LogicAndContext(_ctx, getState());
		enterRule(_localctx, 60, RULE_logicAnd);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(423);
			bitwiseOr();
			setState(428);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==LogicalAndOperator) {
				{
				{
				setState(424);
				match(LogicalAndOperator);
				setState(425);
				bitwiseOr();
				}
				}
				setState(430);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BitwiseOrContext extends ParserRuleContext {
		public List<BitwiseXorContext> bitwiseXor() {
			return getRuleContexts(BitwiseXorContext.class);
		}
		public BitwiseXorContext bitwiseXor(int i) {
			return getRuleContext(BitwiseXorContext.class,i);
		}
		public List<TerminalNode> BitwiseOrOperator() { return getTokens(BITEParser.BitwiseOrOperator); }
		public TerminalNode BitwiseOrOperator(int i) {
			return getToken(BITEParser.BitwiseOrOperator, i);
		}
		public BitwiseOrContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_bitwiseOr; }
	}

	public final BitwiseOrContext bitwiseOr() throws RecognitionException {
		BitwiseOrContext _localctx = new BitwiseOrContext(_ctx, getState());
		enterRule(_localctx, 62, RULE_bitwiseOr);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(431);
			bitwiseXor();
			setState(436);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BitwiseOrOperator) {
				{
				{
				setState(432);
				match(BitwiseOrOperator);
				setState(433);
				bitwiseXor();
				}
				}
				setState(438);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BitwiseXorContext extends ParserRuleContext {
		public List<BitwiseAndContext> bitwiseAnd() {
			return getRuleContexts(BitwiseAndContext.class);
		}
		public BitwiseAndContext bitwiseAnd(int i) {
			return getRuleContext(BitwiseAndContext.class,i);
		}
		public List<TerminalNode> BitwiseXorOperator() { return getTokens(BITEParser.BitwiseXorOperator); }
		public TerminalNode BitwiseXorOperator(int i) {
			return getToken(BITEParser.BitwiseXorOperator, i);
		}
		public BitwiseXorContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_bitwiseXor; }
	}

	public final BitwiseXorContext bitwiseXor() throws RecognitionException {
		BitwiseXorContext _localctx = new BitwiseXorContext(_ctx, getState());
		enterRule(_localctx, 64, RULE_bitwiseXor);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(439);
			bitwiseAnd();
			setState(444);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BitwiseXorOperator) {
				{
				{
				setState(440);
				match(BitwiseXorOperator);
				setState(441);
				bitwiseAnd();
				}
				}
				setState(446);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class BitwiseAndContext extends ParserRuleContext {
		public List<EqualityContext> equality() {
			return getRuleContexts(EqualityContext.class);
		}
		public EqualityContext equality(int i) {
			return getRuleContext(EqualityContext.class,i);
		}
		public List<TerminalNode> BitwiseAndOperator() { return getTokens(BITEParser.BitwiseAndOperator); }
		public TerminalNode BitwiseAndOperator(int i) {
			return getToken(BITEParser.BitwiseAndOperator, i);
		}
		public BitwiseAndContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_bitwiseAnd; }
	}

	public final BitwiseAndContext bitwiseAnd() throws RecognitionException {
		BitwiseAndContext _localctx = new BitwiseAndContext(_ctx, getState());
		enterRule(_localctx, 66, RULE_bitwiseAnd);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(447);
			equality();
			setState(452);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==BitwiseAndOperator) {
				{
				{
				setState(448);
				match(BitwiseAndOperator);
				setState(449);
				equality();
				}
				}
				setState(454);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class EqualityContext extends ParserRuleContext {
		public List<RelationalContext> relational() {
			return getRuleContexts(RelationalContext.class);
		}
		public RelationalContext relational(int i) {
			return getRuleContext(RelationalContext.class,i);
		}
		public List<TerminalNode> UnequalOperator() { return getTokens(BITEParser.UnequalOperator); }
		public TerminalNode UnequalOperator(int i) {
			return getToken(BITEParser.UnequalOperator, i);
		}
		public List<TerminalNode> EqualOperator() { return getTokens(BITEParser.EqualOperator); }
		public TerminalNode EqualOperator(int i) {
			return getToken(BITEParser.EqualOperator, i);
		}
		public EqualityContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_equality; }
	}

	public final EqualityContext equality() throws RecognitionException {
		EqualityContext _localctx = new EqualityContext(_ctx, getState());
		enterRule(_localctx, 68, RULE_equality);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(455);
			relational();
			setState(460);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==UnequalOperator || _la==EqualOperator) {
				{
				{
				setState(456);
				_la = _input.LA(1);
				if ( !(_la==UnequalOperator || _la==EqualOperator) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(457);
				relational();
				}
				}
				setState(462);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class RelationalContext extends ParserRuleContext {
		public List<ShiftContext> shift() {
			return getRuleContexts(ShiftContext.class);
		}
		public ShiftContext shift(int i) {
			return getRuleContext(ShiftContext.class,i);
		}
		public List<TerminalNode> GreaterOperator() { return getTokens(BITEParser.GreaterOperator); }
		public TerminalNode GreaterOperator(int i) {
			return getToken(BITEParser.GreaterOperator, i);
		}
		public List<TerminalNode> GreaterEqualOperator() { return getTokens(BITEParser.GreaterEqualOperator); }
		public TerminalNode GreaterEqualOperator(int i) {
			return getToken(BITEParser.GreaterEqualOperator, i);
		}
		public List<TerminalNode> SmallerOperator() { return getTokens(BITEParser.SmallerOperator); }
		public TerminalNode SmallerOperator(int i) {
			return getToken(BITEParser.SmallerOperator, i);
		}
		public List<TerminalNode> SmallerEqualOperator() { return getTokens(BITEParser.SmallerEqualOperator); }
		public TerminalNode SmallerEqualOperator(int i) {
			return getToken(BITEParser.SmallerEqualOperator, i);
		}
		public RelationalContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_relational; }
	}

	public final RelationalContext relational() throws RecognitionException {
		RelationalContext _localctx = new RelationalContext(_ctx, getState());
		enterRule(_localctx, 70, RULE_relational);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(463);
			shift();
			setState(468);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << GreaterOperator) | (1L << GreaterEqualOperator) | (1L << SmallerOperator) | (1L << SmallerEqualOperator))) != 0)) {
				{
				{
				setState(464);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << GreaterOperator) | (1L << GreaterEqualOperator) | (1L << SmallerOperator) | (1L << SmallerEqualOperator))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(465);
				shift();
				}
				}
				setState(470);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ShiftContext extends ParserRuleContext {
		public List<AdditiveContext> additive() {
			return getRuleContexts(AdditiveContext.class);
		}
		public AdditiveContext additive(int i) {
			return getRuleContext(AdditiveContext.class,i);
		}
		public List<TerminalNode> ShiftLeftOperator() { return getTokens(BITEParser.ShiftLeftOperator); }
		public TerminalNode ShiftLeftOperator(int i) {
			return getToken(BITEParser.ShiftLeftOperator, i);
		}
		public List<TerminalNode> ShiftRightOperator() { return getTokens(BITEParser.ShiftRightOperator); }
		public TerminalNode ShiftRightOperator(int i) {
			return getToken(BITEParser.ShiftRightOperator, i);
		}
		public ShiftContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_shift; }
	}

	public final ShiftContext shift() throws RecognitionException {
		ShiftContext _localctx = new ShiftContext(_ctx, getState());
		enterRule(_localctx, 72, RULE_shift);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(471);
			additive();
			setState(476);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==ShiftRightOperator || _la==ShiftLeftOperator) {
				{
				{
				setState(472);
				_la = _input.LA(1);
				if ( !(_la==ShiftRightOperator || _la==ShiftLeftOperator) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(473);
				additive();
				}
				}
				setState(478);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AdditiveContext extends ParserRuleContext {
		public List<MultiplicativeContext> multiplicative() {
			return getRuleContexts(MultiplicativeContext.class);
		}
		public MultiplicativeContext multiplicative(int i) {
			return getRuleContext(MultiplicativeContext.class,i);
		}
		public List<TerminalNode> MinusOperator() { return getTokens(BITEParser.MinusOperator); }
		public TerminalNode MinusOperator(int i) {
			return getToken(BITEParser.MinusOperator, i);
		}
		public List<TerminalNode> PlusOperator() { return getTokens(BITEParser.PlusOperator); }
		public TerminalNode PlusOperator(int i) {
			return getToken(BITEParser.PlusOperator, i);
		}
		public AdditiveContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_additive; }
	}

	public final AdditiveContext additive() throws RecognitionException {
		AdditiveContext _localctx = new AdditiveContext(_ctx, getState());
		enterRule(_localctx, 74, RULE_additive);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(479);
			multiplicative();
			setState(484);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==MinusOperator || _la==PlusOperator) {
				{
				{
				setState(480);
				_la = _input.LA(1);
				if ( !(_la==MinusOperator || _la==PlusOperator) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(481);
				multiplicative();
				}
				}
				setState(486);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class MultiplicativeContext extends ParserRuleContext {
		public List<UnaryContext> unary() {
			return getRuleContexts(UnaryContext.class);
		}
		public UnaryContext unary(int i) {
			return getRuleContext(UnaryContext.class,i);
		}
		public List<TerminalNode> DivideOperator() { return getTokens(BITEParser.DivideOperator); }
		public TerminalNode DivideOperator(int i) {
			return getToken(BITEParser.DivideOperator, i);
		}
		public List<TerminalNode> MultiplyOperator() { return getTokens(BITEParser.MultiplyOperator); }
		public TerminalNode MultiplyOperator(int i) {
			return getToken(BITEParser.MultiplyOperator, i);
		}
		public List<TerminalNode> ModuloOperator() { return getTokens(BITEParser.ModuloOperator); }
		public TerminalNode ModuloOperator(int i) {
			return getToken(BITEParser.ModuloOperator, i);
		}
		public MultiplicativeContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_multiplicative; }
	}

	public final MultiplicativeContext multiplicative() throws RecognitionException {
		MultiplicativeContext _localctx = new MultiplicativeContext(_ctx, getState());
		enterRule(_localctx, 76, RULE_multiplicative);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(487);
			unary(0);
			setState(492);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DivideOperator) | (1L << MultiplyOperator) | (1L << ModuloOperator))) != 0)) {
				{
				{
				setState(488);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << DivideOperator) | (1L << MultiplyOperator) | (1L << ModuloOperator))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(489);
				unary(0);
				}
				}
				setState(494);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class UnaryContext extends ParserRuleContext {
		public UnaryContext unary() {
			return getRuleContext(UnaryContext.class,0);
		}
		public TerminalNode LogicalNegationOperator() { return getToken(BITEParser.LogicalNegationOperator, 0); }
		public TerminalNode MinusOperator() { return getToken(BITEParser.MinusOperator, 0); }
		public TerminalNode PlusOperator() { return getToken(BITEParser.PlusOperator, 0); }
		public TerminalNode PlusPlusOperator() { return getToken(BITEParser.PlusPlusOperator, 0); }
		public TerminalNode MinusMinusOperator() { return getToken(BITEParser.MinusMinusOperator, 0); }
		public TerminalNode ComplimentOperator() { return getToken(BITEParser.ComplimentOperator, 0); }
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public UnaryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_unary; }
	}

	public final UnaryContext unary() throws RecognitionException {
		return unary(0);
	}

	private UnaryContext unary(int _p) throws RecognitionException {
		ParserRuleContext _parentctx = _ctx;
		int _parentState = getState();
		UnaryContext _localctx = new UnaryContext(_ctx, _parentState);
		UnaryContext _prevctx = _localctx;
		int _startState = 78;
		enterRecursionRule(_localctx, 78, RULE_unary, _p);
		int _la;
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(499);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case MinusOperator:
			case MinusMinusOperator:
			case PlusOperator:
			case PlusPlusOperator:
			case LogicalNegationOperator:
			case ComplimentOperator:
				{
				setState(496);
				_la = _input.LA(1);
				if ( !((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ComplimentOperator))) != 0)) ) {
				_errHandler.recoverInline(this);
				}
				else {
					if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
					_errHandler.reportMatch(this);
					consume();
				}
				setState(497);
				unary(3);
				}
				break;
			case NullReference:
			case ThisReference:
			case OpeningRoundBracket:
			case BooleanLiteral:
			case IntegerLiteral:
			case FloatingLiteral:
			case StringLiteral:
			case Identifier:
				{
				setState(498);
				call();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
			_ctx.stop = _input.LT(-1);
			setState(505);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,57,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					if ( _parseListeners!=null ) triggerExitRuleEvent();
					_prevctx = _localctx;
					{
					{
					_localctx = new UnaryContext(_parentctx, _parentState);
					pushNewRecursionContext(_localctx, _startState, RULE_unary);
					setState(501);
					if (!(precpred(_ctx, 1))) throw new FailedPredicateException(this, "precpred(_ctx, 1)");
					setState(502);
					_la = _input.LA(1);
					if ( !(_la==MinusMinusOperator || _la==PlusPlusOperator) ) {
					_errHandler.recoverInline(this);
					}
					else {
						if ( _input.LA(1)==Token.EOF ) matchedEOF = true;
						_errHandler.reportMatch(this);
						consume();
					}
					}
					} 
				}
				setState(507);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,57,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			unrollRecursionContexts(_parentctx);
		}
		return _localctx;
	}

	public static class CallContext extends ParserRuleContext {
		public PrimaryContext primary() {
			return getRuleContext(PrimaryContext.class,0);
		}
		public List<CallArgumentsContext> callArguments() {
			return getRuleContexts(CallArgumentsContext.class);
		}
		public CallArgumentsContext callArguments(int i) {
			return getRuleContext(CallArgumentsContext.class,i);
		}
		public List<TerminalNode> DotOperator() { return getTokens(BITEParser.DotOperator); }
		public TerminalNode DotOperator(int i) {
			return getToken(BITEParser.DotOperator, i);
		}
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public List<ElementAccessContext> elementAccess() {
			return getRuleContexts(ElementAccessContext.class);
		}
		public ElementAccessContext elementAccess(int i) {
			return getRuleContext(ElementAccessContext.class,i);
		}
		public CallContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_call; }
	}

	public final CallContext call() throws RecognitionException {
		CallContext _localctx = new CallContext(_ctx, getState());
		enterRule(_localctx, 80, RULE_call);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(508);
			primary();
			setState(515);
			_errHandler.sync(this);
			_alt = getInterpreter().adaptivePredict(_input,59,_ctx);
			while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					setState(513);
					_errHandler.sync(this);
					switch (_input.LA(1)) {
					case OpeningRoundBracket:
						{
						setState(509);
						callArguments();
						}
						break;
					case DotOperator:
						{
						setState(510);
						match(DotOperator);
						setState(511);
						match(Identifier);
						}
						break;
					case SquarebracketLeft:
						{
						setState(512);
						elementAccess();
						}
						break;
					default:
						throw new NoViableAltException(this);
					}
					} 
				}
				setState(517);
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,59,_ctx);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PrimaryContext extends ParserRuleContext {
		public TerminalNode BooleanLiteral() { return getToken(BITEParser.BooleanLiteral, 0); }
		public TerminalNode NullReference() { return getToken(BITEParser.NullReference, 0); }
		public TerminalNode ThisReference() { return getToken(BITEParser.ThisReference, 0); }
		public TerminalNode IntegerLiteral() { return getToken(BITEParser.IntegerLiteral, 0); }
		public TerminalNode FloatingLiteral() { return getToken(BITEParser.FloatingLiteral, 0); }
		public TerminalNode StringLiteral() { return getToken(BITEParser.StringLiteral, 0); }
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public PrimaryContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_primary; }
	}

	public final PrimaryContext primary() throws RecognitionException {
		PrimaryContext _localctx = new PrimaryContext(_ctx, getState());
		enterRule(_localctx, 82, RULE_primary);
		try {
			setState(529);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case BooleanLiteral:
				enterOuterAlt(_localctx, 1);
				{
				setState(518);
				match(BooleanLiteral);
				}
				break;
			case NullReference:
				enterOuterAlt(_localctx, 2);
				{
				setState(519);
				match(NullReference);
				}
				break;
			case ThisReference:
				enterOuterAlt(_localctx, 3);
				{
				setState(520);
				match(ThisReference);
				}
				break;
			case IntegerLiteral:
				enterOuterAlt(_localctx, 4);
				{
				setState(521);
				match(IntegerLiteral);
				}
				break;
			case FloatingLiteral:
				enterOuterAlt(_localctx, 5);
				{
				setState(522);
				match(FloatingLiteral);
				}
				break;
			case StringLiteral:
				enterOuterAlt(_localctx, 6);
				{
				setState(523);
				match(StringLiteral);
				}
				break;
			case Identifier:
				enterOuterAlt(_localctx, 7);
				{
				setState(524);
				match(Identifier);
				}
				break;
			case OpeningRoundBracket:
				enterOuterAlt(_localctx, 8);
				{
				setState(525);
				match(OpeningRoundBracket);
				setState(526);
				expression();
				setState(527);
				match(ClosingRoundBracket);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PrivateModifierContext extends ParserRuleContext {
		public TerminalNode DeclarePrivate() { return getToken(BITEParser.DeclarePrivate, 0); }
		public PrivateModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_privateModifier; }
	}

	public final PrivateModifierContext privateModifier() throws RecognitionException {
		PrivateModifierContext _localctx = new PrivateModifierContext(_ctx, getState());
		enterRule(_localctx, 84, RULE_privateModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(531);
			match(DeclarePrivate);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class PublicModifierContext extends ParserRuleContext {
		public TerminalNode DeclarePublic() { return getToken(BITEParser.DeclarePublic, 0); }
		public PublicModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_publicModifier; }
	}

	public final PublicModifierContext publicModifier() throws RecognitionException {
		PublicModifierContext _localctx = new PublicModifierContext(_ctx, getState());
		enterRule(_localctx, 86, RULE_publicModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(533);
			match(DeclarePublic);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class AbstractModifierContext extends ParserRuleContext {
		public TerminalNode DeclareAbstract() { return getToken(BITEParser.DeclareAbstract, 0); }
		public AbstractModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_abstractModifier; }
	}

	public final AbstractModifierContext abstractModifier() throws RecognitionException {
		AbstractModifierContext _localctx = new AbstractModifierContext(_ctx, getState());
		enterRule(_localctx, 88, RULE_abstractModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(535);
			match(DeclareAbstract);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StaticModifierContext extends ParserRuleContext {
		public TerminalNode DeclareStatic() { return getToken(BITEParser.DeclareStatic, 0); }
		public StaticModifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_staticModifier; }
	}

	public final StaticModifierContext staticModifier() throws RecognitionException {
		StaticModifierContext _localctx = new StaticModifierContext(_ctx, getState());
		enterRule(_localctx, 90, RULE_staticModifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(537);
			match(DeclareStatic);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ParametersContext extends ParserRuleContext {
		public List<ParametersIdentifierContext> parametersIdentifier() {
			return getRuleContexts(ParametersIdentifierContext.class);
		}
		public ParametersIdentifierContext parametersIdentifier(int i) {
			return getRuleContext(ParametersIdentifierContext.class,i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public ParametersContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_parameters; }
	}

	public final ParametersContext parameters() throws RecognitionException {
		ParametersContext _localctx = new ParametersContext(_ctx, getState());
		enterRule(_localctx, 92, RULE_parameters);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(539);
			parametersIdentifier();
			setState(544);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CommaSeperator) {
				{
				{
				setState(540);
				match(CommaSeperator);
				setState(541);
				parametersIdentifier();
				}
				}
				setState(546);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ArgumentsContext extends ParserRuleContext {
		public List<ArgumentExpressionContext> argumentExpression() {
			return getRuleContexts(ArgumentExpressionContext.class);
		}
		public ArgumentExpressionContext argumentExpression(int i) {
			return getRuleContext(ArgumentExpressionContext.class,i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public ArgumentsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_arguments; }
	}

	public final ArgumentsContext arguments() throws RecognitionException {
		ArgumentsContext _localctx = new ArgumentsContext(_ctx, getState());
		enterRule(_localctx, 94, RULE_arguments);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(547);
			argumentExpression();
			setState(552);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CommaSeperator) {
				{
				{
				setState(548);
				match(CommaSeperator);
				setState(549);
				argumentExpression();
				}
				}
				setState(554);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class InheritanceContext extends ParserRuleContext {
		public List<TerminalNode> Identifier() { return getTokens(BITEParser.Identifier); }
		public TerminalNode Identifier(int i) {
			return getToken(BITEParser.Identifier, i);
		}
		public List<TerminalNode> CommaSeperator() { return getTokens(BITEParser.CommaSeperator); }
		public TerminalNode CommaSeperator(int i) {
			return getToken(BITEParser.CommaSeperator, i);
		}
		public InheritanceContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_inheritance; }
	}

	public final InheritanceContext inheritance() throws RecognitionException {
		InheritanceContext _localctx = new InheritanceContext(_ctx, getState());
		enterRule(_localctx, 96, RULE_inheritance);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(555);
			match(Identifier);
			setState(560);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while (_la==CommaSeperator) {
				{
				{
				setState(556);
				match(CommaSeperator);
				setState(557);
				match(Identifier);
				}
				}
				setState(562);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class CallArgumentsContext extends ParserRuleContext {
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public ArgumentsContext arguments() {
			return getRuleContext(ArgumentsContext.class,0);
		}
		public CallArgumentsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_callArguments; }
	}

	public final CallArgumentsContext callArguments() throws RecognitionException {
		CallArgumentsContext _localctx = new CallArgumentsContext(_ctx, getState());
		enterRule(_localctx, 98, RULE_callArguments);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(563);
			match(OpeningRoundBracket);
			setState(565);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if ((((_la) & ~0x3f) == 0 && ((1L << _la) & ((1L << NullReference) | (1L << ThisReference) | (1L << MinusOperator) | (1L << MinusMinusOperator) | (1L << PlusOperator) | (1L << PlusPlusOperator) | (1L << LogicalNegationOperator) | (1L << ReferenceOperator) | (1L << ComplimentOperator) | (1L << OpeningRoundBracket))) != 0) || ((((_la - 69)) & ~0x3f) == 0 && ((1L << (_la - 69)) & ((1L << (BooleanLiteral - 69)) | (1L << (IntegerLiteral - 69)) | (1L << (FloatingLiteral - 69)) | (1L << (StringLiteral - 69)) | (1L << (Identifier - 69)))) != 0)) {
				{
				setState(564);
				arguments();
				}
			}

			setState(567);
			match(ClosingRoundBracket);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ElementAccessContext extends ParserRuleContext {
		public List<TerminalNode> SquarebracketLeft() { return getTokens(BITEParser.SquarebracketLeft); }
		public TerminalNode SquarebracketLeft(int i) {
			return getToken(BITEParser.SquarebracketLeft, i);
		}
		public List<ElementIdentifierContext> elementIdentifier() {
			return getRuleContexts(ElementIdentifierContext.class);
		}
		public ElementIdentifierContext elementIdentifier(int i) {
			return getRuleContext(ElementIdentifierContext.class,i);
		}
		public List<TerminalNode> SquarebracketRight() { return getTokens(BITEParser.SquarebracketRight); }
		public TerminalNode SquarebracketRight(int i) {
			return getToken(BITEParser.SquarebracketRight, i);
		}
		public ElementAccessContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_elementAccess; }
	}

	public final ElementAccessContext elementAccess() throws RecognitionException {
		ElementAccessContext _localctx = new ElementAccessContext(_ctx, getState());
		enterRule(_localctx, 100, RULE_elementAccess);
		try {
			int _alt;
			enterOuterAlt(_localctx, 1);
			{
			setState(573); 
			_errHandler.sync(this);
			_alt = 1;
			do {
				switch (_alt) {
				case 1:
					{
					{
					setState(569);
					match(SquarebracketLeft);
					setState(570);
					elementIdentifier();
					setState(571);
					match(SquarebracketRight);
					}
					}
					break;
				default:
					throw new NoViableAltException(this);
				}
				setState(575); 
				_errHandler.sync(this);
				_alt = getInterpreter().adaptivePredict(_input,65,_ctx);
			} while ( _alt!=2 && _alt!=org.antlr.v4.runtime.atn.ATN.INVALID_ALT_NUMBER );
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ElementIdentifierContext extends ParserRuleContext {
		public TerminalNode IntegerLiteral() { return getToken(BITEParser.IntegerLiteral, 0); }
		public TerminalNode StringLiteral() { return getToken(BITEParser.StringLiteral, 0); }
		public CallContext call() {
			return getRuleContext(CallContext.class,0);
		}
		public ElementIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_elementIdentifier; }
	}

	public final ElementIdentifierContext elementIdentifier() throws RecognitionException {
		ElementIdentifierContext _localctx = new ElementIdentifierContext(_ctx, getState());
		enterRule(_localctx, 102, RULE_elementIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(580);
			_errHandler.sync(this);
			switch ( getInterpreter().adaptivePredict(_input,66,_ctx) ) {
			case 1:
				{
				setState(577);
				match(IntegerLiteral);
				}
				break;
			case 2:
				{
				setState(578);
				match(StringLiteral);
				}
				break;
			case 3:
				{
				setState(579);
				call();
				}
				break;
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ArgumentExpressionContext extends ParserRuleContext {
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ReferenceOperator() { return getToken(BITEParser.ReferenceOperator, 0); }
		public ArgumentExpressionContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_argumentExpression; }
	}

	public final ArgumentExpressionContext argumentExpression() throws RecognitionException {
		ArgumentExpressionContext _localctx = new ArgumentExpressionContext(_ctx, getState());
		enterRule(_localctx, 104, RULE_argumentExpression);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(583);
			_errHandler.sync(this);
			_la = _input.LA(1);
			if (_la==ReferenceOperator) {
				{
				setState(582);
				match(ReferenceOperator);
				}
			}

			setState(585);
			expression();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class ParametersIdentifierContext extends ParserRuleContext {
		public TerminalNode Identifier() { return getToken(BITEParser.Identifier, 0); }
		public ParametersIdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_parametersIdentifier; }
	}

	public final ParametersIdentifierContext parametersIdentifier() throws RecognitionException {
		ParametersIdentifierContext _localctx = new ParametersIdentifierContext(_ctx, getState());
		enterRule(_localctx, 106, RULE_parametersIdentifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(587);
			match(Identifier);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static class StringContentsContext extends ParserRuleContext {
		public TerminalNode TEXT() { return getToken(BITEParser.TEXT, 0); }
		public TerminalNode ESCAPE_SEQUENCE() { return getToken(BITEParser.ESCAPE_SEQUENCE, 0); }
		public TerminalNode DollarOperator() { return getToken(BITEParser.DollarOperator, 0); }
		public TerminalNode OpeningRoundBracket() { return getToken(BITEParser.OpeningRoundBracket, 0); }
		public ExpressionContext expression() {
			return getRuleContext(ExpressionContext.class,0);
		}
		public TerminalNode ClosingRoundBracket() { return getToken(BITEParser.ClosingRoundBracket, 0); }
		public StringContentsContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_stringContents; }
	}

	public final StringContentsContext stringContents() throws RecognitionException {
		StringContentsContext _localctx = new StringContentsContext(_ctx, getState());
		enterRule(_localctx, 108, RULE_stringContents);
		try {
			setState(596);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case TEXT:
				enterOuterAlt(_localctx, 1);
				{
				setState(589);
				match(TEXT);
				}
				break;
			case ESCAPE_SEQUENCE:
				enterOuterAlt(_localctx, 2);
				{
				setState(590);
				match(ESCAPE_SEQUENCE);
				}
				break;
			case DollarOperator:
				enterOuterAlt(_localctx, 3);
				{
				setState(591);
				match(DollarOperator);
				setState(592);
				match(OpeningRoundBracket);
				setState(593);
				expression();
				setState(594);
				match(ClosingRoundBracket);
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public boolean sempred(RuleContext _localctx, int ruleIndex, int predIndex) {
		switch (ruleIndex) {
		case 39:
			return unary_sempred((UnaryContext)_localctx, predIndex);
		}
		return true;
	}
	private boolean unary_sempred(UnaryContext _localctx, int predIndex) {
		switch (predIndex) {
		case 0:
			return precpred(_ctx, 1);
		}
		return true;
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\3Y\u0259\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t"+
		"\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t+\4"+
		",\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t\62\4\63\t\63\4\64\t"+
		"\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\3\2\7\2r\n\2\f\2\16\2u\13\2\3"+
		"\3\3\3\3\3\7\3z\n\3\f\3\16\3}\13\3\3\3\7\3\u0080\n\3\f\3\16\3\u0083\13"+
		"\3\3\3\3\3\3\4\3\4\3\4\3\4\7\4\u008b\n\4\f\4\16\4\u008e\13\4\3\4\3\4\3"+
		"\5\3\5\3\5\3\5\7\5\u0096\n\5\f\5\16\5\u0099\13\5\3\5\3\5\3\6\3\6\3\6\3"+
		"\6\7\6\u00a1\n\6\f\6\16\6\u00a4\13\6\3\6\3\6\3\7\3\7\3\7\3\7\3\7\3\7\5"+
		"\7\u00ae\n\7\3\b\3\b\5\b\u00b2\n\b\3\b\3\b\5\b\u00b6\n\b\3\b\3\b\3\b\3"+
		"\b\5\b\u00bc\n\b\3\b\3\b\5\b\u00c0\n\b\3\t\3\t\5\t\u00c4\n\t\3\t\3\t\3"+
		"\t\3\t\5\t\u00ca\n\t\3\n\3\n\5\n\u00ce\n\n\3\n\3\n\5\n\u00d2\n\n\3\n\3"+
		"\n\3\n\3\n\5\n\u00d8\n\n\3\n\3\n\3\n\5\n\u00dd\n\n\3\13\3\13\5\13\u00e1"+
		"\n\13\3\13\5\13\u00e4\n\13\3\13\3\13\3\13\3\13\3\13\3\13\3\13\7\13\u00ed"+
		"\n\13\f\13\16\13\u00f0\13\13\3\13\3\13\5\13\u00f4\n\13\3\13\3\13\3\13"+
		"\3\13\3\13\3\13\3\13\3\13\7\13\u00fe\n\13\f\13\16\13\u0101\13\13\3\13"+
		"\3\13\5\13\u0105\n\13\3\13\3\13\5\13\u0109\n\13\3\f\3\f\5\f\u010d\n\f"+
		"\3\f\5\f\u0110\n\f\3\f\3\f\3\f\3\f\5\f\u0116\n\f\3\f\5\f\u0119\n\f\3\r"+
		"\3\r\7\r\u011d\n\r\f\r\16\r\u0120\13\r\3\16\3\16\3\16\3\16\3\16\3\16\3"+
		"\16\3\16\5\16\u012a\n\16\3\17\3\17\3\17\3\20\3\20\3\20\5\20\u0132\n\20"+
		"\3\21\3\21\3\21\3\21\7\21\u0138\n\21\f\21\16\21\u013b\13\21\3\22\3\22"+
		"\3\22\3\22\7\22\u0141\n\22\f\22\16\22\u0144\13\22\5\22\u0146\n\22\3\23"+
		"\3\23\3\23\7\23\u014b\n\23\f\23\16\23\u014e\13\23\3\24\3\24\3\24\5\24"+
		"\u0153\n\24\3\24\3\24\5\24\u0157\n\24\3\24\3\24\5\24\u015b\n\24\3\24\3"+
		"\24\5\24\u015f\n\24\3\25\3\25\3\25\3\25\3\25\3\25\3\25\5\25\u0168\n\25"+
		"\3\26\3\26\5\26\u016c\n\26\3\26\3\26\3\27\3\27\3\27\3\30\3\30\3\30\3\30"+
		"\3\30\3\30\3\31\3\31\3\31\3\31\3\31\3\31\3\32\3\32\7\32\u0181\n\32\f\32"+
		"\16\32\u0184\13\32\3\32\3\32\3\33\3\33\5\33\u018a\n\33\3\34\3\34\3\34"+
		"\3\34\3\34\5\34\u0191\n\34\3\35\3\35\3\35\3\35\3\36\3\36\3\36\3\36\3\36"+
		"\3\36\7\36\u019d\n\36\f\36\16\36\u01a0\13\36\3\37\3\37\3\37\7\37\u01a5"+
		"\n\37\f\37\16\37\u01a8\13\37\3 \3 \3 \7 \u01ad\n \f \16 \u01b0\13 \3!"+
		"\3!\3!\7!\u01b5\n!\f!\16!\u01b8\13!\3\"\3\"\3\"\7\"\u01bd\n\"\f\"\16\""+
		"\u01c0\13\"\3#\3#\3#\7#\u01c5\n#\f#\16#\u01c8\13#\3$\3$\3$\7$\u01cd\n"+
		"$\f$\16$\u01d0\13$\3%\3%\3%\7%\u01d5\n%\f%\16%\u01d8\13%\3&\3&\3&\7&\u01dd"+
		"\n&\f&\16&\u01e0\13&\3\'\3\'\3\'\7\'\u01e5\n\'\f\'\16\'\u01e8\13\'\3("+
		"\3(\3(\7(\u01ed\n(\f(\16(\u01f0\13(\3)\3)\3)\3)\5)\u01f6\n)\3)\3)\7)\u01fa"+
		"\n)\f)\16)\u01fd\13)\3*\3*\3*\3*\3*\7*\u0204\n*\f*\16*\u0207\13*\3+\3"+
		"+\3+\3+\3+\3+\3+\3+\3+\3+\3+\5+\u0214\n+\3,\3,\3-\3-\3.\3.\3/\3/\3\60"+
		"\3\60\3\60\7\60\u0221\n\60\f\60\16\60\u0224\13\60\3\61\3\61\3\61\7\61"+
		"\u0229\n\61\f\61\16\61\u022c\13\61\3\62\3\62\3\62\7\62\u0231\n\62\f\62"+
		"\16\62\u0234\13\62\3\63\3\63\5\63\u0238\n\63\3\63\3\63\3\64\3\64\3\64"+
		"\3\64\6\64\u0240\n\64\r\64\16\64\u0241\3\65\3\65\3\65\5\65\u0247\n\65"+
		"\3\66\5\66\u024a\n\66\3\66\3\66\3\67\3\67\38\38\38\38\38\38\38\58\u0257"+
		"\n8\38\2\3P9\2\4\6\b\n\f\16\20\22\24\26\30\32\34\36 \"$&(*,.\60\62\64"+
		"\668:<>@BDFHJLNPRTVXZ\\^`bdfhjln\2\n\3\2\31#\3\2&\'\5\2((*+--\4\2)),,"+
		"\4\2..\60\60\4\2\62\6399\5\2.\61\64\64::\4\2//\61\61\2\u0280\2s\3\2\2"+
		"\2\4v\3\2\2\2\6\u0086\3\2\2\2\b\u0091\3\2\2\2\n\u009c\3\2\2\2\f\u00ad"+
		"\3\2\2\2\16\u00b1\3\2\2\2\20\u00c3\3\2\2\2\22\u00cd\3\2\2\2\24\u0108\3"+
		"\2\2\2\26\u010c\3\2\2\2\30\u011a\3\2\2\2\32\u0129\3\2\2\2\34\u012b\3\2"+
		"\2\2\36\u012e\3\2\2\2 \u0133\3\2\2\2\"\u0145\3\2\2\2$\u0147\3\2\2\2&\u014f"+
		"\3\2\2\2(\u0160\3\2\2\2*\u0169\3\2\2\2,\u016f\3\2\2\2.\u0172\3\2\2\2\60"+
		"\u0178\3\2\2\2\62\u017e\3\2\2\2\64\u0189\3\2\2\2\66\u0190\3\2\2\28\u0192"+
		"\3\2\2\2:\u0196\3\2\2\2<\u01a1\3\2\2\2>\u01a9\3\2\2\2@\u01b1\3\2\2\2B"+
		"\u01b9\3\2\2\2D\u01c1\3\2\2\2F\u01c9\3\2\2\2H\u01d1\3\2\2\2J\u01d9\3\2"+
		"\2\2L\u01e1\3\2\2\2N\u01e9\3\2\2\2P\u01f5\3\2\2\2R\u01fe\3\2\2\2T\u0213"+
		"\3\2\2\2V\u0215\3\2\2\2X\u0217\3\2\2\2Z\u0219\3\2\2\2\\\u021b\3\2\2\2"+
		"^\u021d\3\2\2\2`\u0225\3\2\2\2b\u022d\3\2\2\2d\u0235\3\2\2\2f\u023f\3"+
		"\2\2\2h\u0246\3\2\2\2j\u0249\3\2\2\2l\u024d\3\2\2\2n\u0256\3\2\2\2pr\5"+
		"\4\3\2qp\3\2\2\2ru\3\2\2\2sq\3\2\2\2st\3\2\2\2t\3\3\2\2\2us\3\2\2\2v{"+
		"\5\6\4\2wz\5\b\5\2xz\5\n\6\2yw\3\2\2\2yx\3\2\2\2z}\3\2\2\2{y\3\2\2\2{"+
		"|\3\2\2\2|\u0081\3\2\2\2}{\3\2\2\2~\u0080\5\f\7\2\177~\3\2\2\2\u0080\u0083"+
		"\3\2\2\2\u0081\177\3\2\2\2\u0081\u0082\3\2\2\2\u0082\u0084\3\2\2\2\u0083"+
		"\u0081\3\2\2\2\u0084\u0085\7\2\2\3\u0085\5\3\2\2\2\u0086\u0087\7\3\2\2"+
		"\u0087\u008c\7Y\2\2\u0088\u0089\7\65\2\2\u0089\u008b\7Y\2\2\u008a\u0088"+
		"\3\2\2\2\u008b\u008e\3\2\2\2\u008c\u008a\3\2\2\2\u008c\u008d\3\2\2\2\u008d"+
		"\u008f\3\2\2\2\u008e\u008c\3\2\2\2\u008f\u0090\7E\2\2\u0090\7\3\2\2\2"+
		"\u0091\u0092\7\30\2\2\u0092\u0097\7Y\2\2\u0093\u0094\7\65\2\2\u0094\u0096"+
		"\7Y\2\2\u0095\u0093\3\2\2\2\u0096\u0099\3\2\2\2\u0097\u0095\3\2\2\2\u0097"+
		"\u0098\3\2\2\2\u0098\u009a\3\2\2\2\u0099\u0097\3\2\2\2\u009a\u009b\7E"+
		"\2\2\u009b\t\3\2\2\2\u009c\u009d\7\27\2\2\u009d\u00a2\7Y\2\2\u009e\u009f"+
		"\7\65\2\2\u009f\u00a1\7Y\2\2\u00a0\u009e\3\2\2\2\u00a1\u00a4\3\2\2\2\u00a2"+
		"\u00a0\3\2\2\2\u00a2\u00a3\3\2\2\2\u00a3\u00a5\3\2\2\2\u00a4\u00a2\3\2"+
		"\2\2\u00a5\u00a6\7E\2\2\u00a6\13\3\2\2\2\u00a7\u00ae\5\16\b\2\u00a8\u00ae"+
		"\5\20\t\2\u00a9\u00ae\5\22\n\2\u00aa\u00ae\5\24\13\2\u00ab\u00ae\5\26"+
		"\f\2\u00ac\u00ae\5\32\16\2\u00ad\u00a7\3\2\2\2\u00ad\u00a8\3\2\2\2\u00ad"+
		"\u00a9\3\2\2\2\u00ad\u00aa\3\2\2\2\u00ad\u00ab\3\2\2\2\u00ad\u00ac\3\2"+
		"\2\2\u00ae\r\3\2\2\2\u00af\u00b2\5V,\2\u00b0\u00b2\5X-\2\u00b1\u00af\3"+
		"\2\2\2\u00b1\u00b0\3\2\2\2\u00b1\u00b2\3\2\2\2\u00b2\u00b5\3\2\2\2\u00b3"+
		"\u00b6\5\\/\2\u00b4\u00b6\5Z.\2\u00b5\u00b3\3\2\2\2\u00b5\u00b4\3\2\2"+
		"\2\u00b5\u00b6\3\2\2\2\u00b6\u00b7\3\2\2\2\u00b7\u00b8\7\4\2\2\u00b8\u00bb"+
		"\7Y\2\2\u00b9\u00ba\7\67\2\2\u00ba\u00bc\5b\62\2\u00bb\u00b9\3\2\2\2\u00bb"+
		"\u00bc\3\2\2\2\u00bc\u00bf\3\2\2\2\u00bd\u00c0\5\62\32\2\u00be\u00c0\7"+
		"E\2\2\u00bf\u00bd\3\2\2\2\u00bf\u00be\3\2\2\2\u00c0\17\3\2\2\2\u00c1\u00c4"+
		"\5V,\2\u00c2\u00c4\5X-\2\u00c3\u00c1\3\2\2\2\u00c3\u00c2\3\2\2\2\u00c3"+
		"\u00c4\3\2\2\2\u00c4\u00c5\3\2\2\2\u00c5\u00c6\7\5\2\2\u00c6\u00c9\7Y"+
		"\2\2\u00c7\u00ca\5\62\32\2\u00c8\u00ca\7E\2\2\u00c9\u00c7\3\2\2\2\u00c9"+
		"\u00c8\3\2\2\2\u00ca\21\3\2\2\2\u00cb\u00ce\5V,\2\u00cc\u00ce\5X-\2\u00cd"+
		"\u00cb\3\2\2\2\u00cd\u00cc\3\2\2\2\u00cd\u00ce\3\2\2\2\u00ce\u00d1\3\2"+
		"\2\2\u00cf\u00d2\5\\/\2\u00d0\u00d2\5Z.\2\u00d1\u00cf\3\2\2\2\u00d1\u00d0"+
		"\3\2\2\2\u00d1\u00d2\3\2\2\2\u00d2\u00d3\3\2\2\2\u00d3\u00d4\7\7\2\2\u00d4"+
		"\u00d5\7Y\2\2\u00d5\u00d7\7>\2\2\u00d6\u00d8\5^\60\2\u00d7\u00d6\3\2\2"+
		"\2\u00d7\u00d8\3\2\2\2\u00d8\u00d9\3\2\2\2\u00d9\u00dc\7?\2\2\u00da\u00dd"+
		"\5\62\32\2\u00db\u00dd\7E\2\2\u00dc\u00da\3\2\2\2\u00dc\u00db\3\2\2\2"+
		"\u00dd\23\3\2\2\2\u00de\u00e1\5V,\2\u00df\u00e1\5X-\2\u00e0\u00de\3\2"+
		"\2\2\u00e0\u00df\3\2\2\2\u00e0\u00e1\3\2\2\2\u00e1\u00e3\3\2\2\2\u00e2"+
		"\u00e4\5\\/\2\u00e3\u00e2\3\2\2\2\u00e3\u00e4\3\2\2\2\u00e4\u00e5\3\2"+
		"\2\2\u00e5\u00e6\7\b\2\2\u00e6\u00e7\7Y\2\2\u00e7\u00e8\7\31\2\2\u00e8"+
		"\u00e9\7\6\2\2\u00e9\u00ee\7Y\2\2\u00ea\u00eb\7\65\2\2\u00eb\u00ed\7Y"+
		"\2\2\u00ec\u00ea\3\2\2\2\u00ed\u00f0\3\2\2\2\u00ee\u00ec\3\2\2\2\u00ee"+
		"\u00ef\3\2\2\2\u00ef\u00f1\3\2\2\2\u00f0\u00ee\3\2\2\2\u00f1\u00f3\7>"+
		"\2\2\u00f2\u00f4\5`\61\2\u00f3\u00f2\3\2\2\2\u00f3\u00f4\3\2\2\2\u00f4"+
		"\u00f5\3\2\2\2\u00f5\u00f6\7?\2\2\u00f6\u0109\7E\2\2\u00f7\u00f8\7Y\2"+
		"\2\u00f8\u00f9\7\31\2\2\u00f9\u00fa\7\6\2\2\u00fa\u00ff\7Y\2\2\u00fb\u00fc"+
		"\7\65\2\2\u00fc\u00fe\7Y\2\2\u00fd\u00fb\3\2\2\2\u00fe\u0101\3\2\2\2\u00ff"+
		"\u00fd\3\2\2\2\u00ff\u0100\3\2\2\2\u0100\u0102\3\2\2\2\u0101\u00ff\3\2"+
		"\2\2\u0102\u0104\7>\2\2\u0103\u0105\5`\61\2\u0104\u0103\3\2\2\2\u0104"+
		"\u0105\3\2\2\2\u0105\u0106\3\2\2\2\u0106\u0107\7?\2\2\u0107\u0109\7E\2"+
		"\2\u0108\u00e0\3\2\2\2\u0108\u00f7\3\2\2\2\u0109\25\3\2\2\2\u010a\u010d"+
		"\5V,\2\u010b\u010d\5X-\2\u010c\u010a\3\2\2\2\u010c\u010b\3\2\2\2\u010c"+
		"\u010d\3\2\2\2\u010d\u010f\3\2\2\2\u010e\u0110\5\\/\2\u010f\u010e\3\2"+
		"\2\2\u010f\u0110\3\2\2\2\u0110\u0111\3\2\2\2\u0111\u0112\7\b\2\2\u0112"+
		"\u0118\7Y\2\2\u0113\u0114\7\31\2\2\u0114\u0116\5\34\17\2\u0115\u0113\3"+
		"\2\2\2\u0115\u0116\3\2\2\2\u0116\u0119\3\2\2\2\u0117\u0119\7E\2\2\u0118"+
		"\u0115\3\2\2\2\u0118\u0117\3\2\2\2\u0119\27\3\2\2\2\u011a\u011e\5\f\7"+
		"\2\u011b\u011d\5\f\7\2\u011c\u011b\3\2\2\2\u011d\u0120\3\2\2\2\u011e\u011c"+
		"\3\2\2\2\u011e\u011f\3\2\2\2\u011f\31\3\2\2\2\u0120\u011e\3\2\2\2\u0121"+
		"\u012a\5\34\17\2\u0122\u012a\5&\24\2\u0123\u012a\5(\25\2\u0124\u012a\5"+
		"*\26\2\u0125\u012a\5,\27\2\u0126\u012a\5.\30\2\u0127\u012a\5\60\31\2\u0128"+
		"\u012a\5\62\32\2\u0129\u0121\3\2\2\2\u0129\u0122\3\2\2\2\u0129\u0123\3"+
		"\2\2\2\u0129\u0124\3\2\2\2\u0129\u0125\3\2\2\2\u0129\u0126\3\2\2\2\u0129"+
		"\u0127\3\2\2\2\u0129\u0128\3\2\2\2\u012a\33\3\2\2\2\u012b\u012c\5\64\33"+
		"\2\u012c\u012d\7E\2\2\u012d\35\3\2\2\2\u012e\u0131\7Y\2\2\u012f\u0130"+
		"\7\31\2\2\u0130\u0132\5\64\33\2\u0131\u012f\3\2\2\2\u0131\u0132\3\2\2"+
		"\2\u0132\37\3\2\2\2\u0133\u0134\7\b\2\2\u0134\u0139\5\36\20\2\u0135\u0136"+
		"\7D\2\2\u0136\u0138\5\36\20\2\u0137\u0135\3\2\2\2\u0138\u013b\3\2\2\2"+
		"\u0139\u0137\3\2\2\2\u0139\u013a\3\2\2\2\u013a!\3\2\2\2\u013b\u0139\3"+
		"\2\2\2\u013c\u0146\5 \21\2\u013d\u0142\5\64\33\2\u013e\u013f\7D\2\2\u013f"+
		"\u0141\5\64\33\2\u0140\u013e\3\2\2\2\u0141\u0144\3\2\2\2\u0142\u0140\3"+
		"\2\2\2\u0142\u0143\3\2\2\2\u0143\u0146\3\2\2\2\u0144\u0142\3\2\2\2\u0145"+
		"\u013c\3\2\2\2\u0145\u013d\3\2\2\2\u0146#\3\2\2\2\u0147\u014c\5\64\33"+
		"\2\u0148\u0149\7D\2\2\u0149\u014b\5\64\33\2\u014a\u0148\3\2\2\2\u014b"+
		"\u014e\3\2\2\2\u014c\u014a\3\2\2\2\u014c\u014d\3\2\2\2\u014d%\3\2\2\2"+
		"\u014e\u014c\3\2\2\2\u014f\u0150\7\13\2\2\u0150\u0152\7>\2\2\u0151\u0153"+
		"\5\"\22\2\u0152\u0151\3\2\2\2\u0152\u0153\3\2\2\2\u0153\u0154\3\2\2\2"+
		"\u0154\u0156\7E\2\2\u0155\u0157\5\64\33\2\u0156\u0155\3\2\2\2\u0156\u0157"+
		"\3\2\2\2\u0157\u0158\3\2\2\2\u0158\u015a\7E\2\2\u0159\u015b\5$\23\2\u015a"+
		"\u0159\3\2\2\2\u015a\u015b\3\2\2\2\u015b\u015c\3\2\2\2\u015c\u015e\7?"+
		"\2\2\u015d\u015f\5\32\16\2\u015e\u015d\3\2\2\2\u015e\u015f\3\2\2\2\u015f"+
		"\'\3\2\2\2\u0160\u0161\7\21\2\2\u0161\u0162\7>\2\2\u0162\u0163\5\64\33"+
		"\2\u0163\u0164\7?\2\2\u0164\u0167\5\32\16\2\u0165\u0166\7\22\2\2\u0166"+
		"\u0168\5\32\16\2\u0167\u0165\3\2\2\2\u0167\u0168\3\2\2\2\u0168)\3\2\2"+
		"\2\u0169\u016b\7\23\2\2\u016a\u016c\5\64\33\2\u016b\u016a\3\2\2\2\u016b"+
		"\u016c\3\2\2\2\u016c\u016d\3\2\2\2\u016d\u016e\7E\2\2\u016e+\3\2\2\2\u016f"+
		"\u0170\7\24\2\2\u0170\u0171\7E\2\2\u0171-\3\2\2\2\u0172\u0173\7\27\2\2"+
		"\u0173\u0174\7>\2\2\u0174\u0175\5\64\33\2\u0175\u0176\7?\2\2\u0176\u0177"+
		"\5\62\32\2\u0177/\3\2\2\2\u0178\u0179\7\f\2\2\u0179\u017a\7>\2\2\u017a"+
		"\u017b\5\64\33\2\u017b\u017c\7?\2\2\u017c\u017d\5\62\32\2\u017d\61\3\2"+
		"\2\2\u017e\u0182\7@\2\2\u017f\u0181\5\f\7\2\u0180\u017f\3\2\2\2\u0181"+
		"\u0184\3\2\2\2\u0182\u0180\3\2\2\2\u0182\u0183\3\2\2\2\u0183\u0185\3\2"+
		"\2\2\u0184\u0182\3\2\2\2\u0185\u0186\7A\2\2\u0186\63\3\2\2\2\u0187\u018a"+
		"\5\66\34\2\u0188\u018a\58\35\2\u0189\u0187\3\2\2\2\u0189\u0188\3\2\2\2"+
		"\u018a\65\3\2\2\2\u018b\u018c\5R*\2\u018c\u018d\t\2\2\2\u018d\u018e\5"+
		"\66\34\2\u018e\u0191\3\2\2\2\u018f\u0191\5:\36\2\u0190\u018b\3\2\2\2\u0190"+
		"\u018f\3\2\2\2\u0191\67\3\2\2\2\u0192\u0193\5d\63\2\u0193\u0194\78\2\2"+
		"\u0194\u0195\5\62\32\2\u01959\3\2\2\2\u0196\u019e\5<\37\2\u0197\u0198"+
		"\7\66\2\2\u0198\u0199\5<\37\2\u0199\u019a\7\67\2\2\u019a\u019b\5<\37\2"+
		"\u019b\u019d\3\2\2\2\u019c\u0197\3\2\2\2\u019d\u01a0\3\2\2\2\u019e\u019c"+
		"\3\2\2\2\u019e\u019f\3\2\2\2\u019f;\3\2\2\2\u01a0\u019e\3\2\2\2\u01a1"+
		"\u01a6\5> \2\u01a2\u01a3\7$\2\2\u01a3\u01a5\5> \2\u01a4\u01a2\3\2\2\2"+
		"\u01a5\u01a8\3\2\2\2\u01a6\u01a4\3\2\2\2\u01a6\u01a7\3\2\2\2\u01a7=\3"+
		"\2\2\2\u01a8\u01a6\3\2\2\2\u01a9\u01ae\5@!\2\u01aa\u01ab\7%\2\2\u01ab"+
		"\u01ad\5@!\2\u01ac\u01aa\3\2\2\2\u01ad\u01b0\3\2\2\2\u01ae\u01ac\3\2\2"+
		"\2\u01ae\u01af\3\2\2\2\u01af?\3\2\2\2\u01b0\u01ae\3\2\2\2\u01b1\u01b6"+
		"\5B\"\2\u01b2\u01b3\7=\2\2\u01b3\u01b5\5B\"\2\u01b4\u01b2\3\2\2\2\u01b5"+
		"\u01b8\3\2\2\2\u01b6\u01b4\3\2\2\2\u01b6\u01b7\3\2\2\2\u01b7A\3\2\2\2"+
		"\u01b8\u01b6\3\2\2\2\u01b9\u01be\5D#\2\u01ba\u01bb\7<\2\2\u01bb\u01bd"+
		"\5D#\2\u01bc\u01ba\3\2\2\2\u01bd\u01c0\3\2\2\2\u01be\u01bc\3\2\2\2\u01be"+
		"\u01bf\3\2\2\2\u01bfC\3\2\2\2\u01c0\u01be\3\2\2\2\u01c1\u01c6\5F$\2\u01c2"+
		"\u01c3\7;\2\2\u01c3\u01c5\5F$\2\u01c4\u01c2\3\2\2\2\u01c5\u01c8\3\2\2"+
		"\2\u01c6\u01c4\3\2\2\2\u01c6\u01c7\3\2\2\2\u01c7E\3\2\2\2\u01c8\u01c6"+
		"\3\2\2\2\u01c9\u01ce\5H%\2\u01ca\u01cb\t\3\2\2\u01cb\u01cd\5H%\2\u01cc"+
		"\u01ca\3\2\2\2\u01cd\u01d0\3\2\2\2\u01ce\u01cc\3\2\2\2\u01ce\u01cf\3\2"+
		"\2\2\u01cfG\3\2\2\2\u01d0\u01ce\3\2\2\2\u01d1\u01d6\5J&\2\u01d2\u01d3"+
		"\t\4\2\2\u01d3\u01d5\5J&\2\u01d4\u01d2\3\2\2\2\u01d5\u01d8\3\2\2\2\u01d6"+
		"\u01d4\3\2\2\2\u01d6\u01d7\3\2\2\2\u01d7I\3\2\2\2\u01d8\u01d6\3\2\2\2"+
		"\u01d9\u01de\5L\'\2\u01da\u01db\t\5\2\2\u01db\u01dd\5L\'\2\u01dc\u01da"+
		"\3\2\2\2\u01dd\u01e0\3\2\2\2\u01de\u01dc\3\2\2\2\u01de\u01df\3\2\2\2\u01df"+
		"K\3\2\2\2\u01e0\u01de\3\2\2\2\u01e1\u01e6\5N(\2\u01e2\u01e3\t\6\2\2\u01e3"+
		"\u01e5\5N(\2\u01e4\u01e2\3\2\2\2\u01e5\u01e8\3\2\2\2\u01e6\u01e4\3\2\2"+
		"\2\u01e6\u01e7\3\2\2\2\u01e7M\3\2\2\2\u01e8\u01e6\3\2\2\2\u01e9\u01ee"+
		"\5P)\2\u01ea\u01eb\t\7\2\2\u01eb\u01ed\5P)\2\u01ec\u01ea\3\2\2\2\u01ed"+
		"\u01f0\3\2\2\2\u01ee\u01ec\3\2\2\2\u01ee\u01ef\3\2\2\2\u01efO\3\2\2\2"+
		"\u01f0\u01ee\3\2\2\2\u01f1\u01f2\b)\1\2\u01f2\u01f3\t\b\2\2\u01f3\u01f6"+
		"\5P)\5\u01f4\u01f6\5R*\2\u01f5\u01f1\3\2\2\2\u01f5\u01f4\3\2\2\2\u01f6"+
		"\u01fb\3\2\2\2\u01f7\u01f8\f\3\2\2\u01f8\u01fa\t\t\2\2\u01f9\u01f7\3\2"+
		"\2\2\u01fa\u01fd\3\2\2\2\u01fb\u01f9\3\2\2\2\u01fb\u01fc\3\2\2\2\u01fc"+
		"Q\3\2\2\2\u01fd\u01fb\3\2\2\2\u01fe\u0205\5T+\2\u01ff\u0204\5d\63\2\u0200"+
		"\u0201\7\65\2\2\u0201\u0204\7Y\2\2\u0202\u0204\5f\64\2\u0203\u01ff\3\2"+
		"\2\2\u0203\u0200\3\2\2\2\u0203\u0202\3\2\2\2\u0204\u0207\3\2\2\2\u0205"+
		"\u0203\3\2\2\2\u0205\u0206\3\2\2\2\u0206S\3\2\2\2\u0207\u0205\3\2\2\2"+
		"\u0208\u0214\7G\2\2\u0209\u0214\7\25\2\2\u020a\u0214\7\26\2\2\u020b\u0214"+
		"\7J\2\2\u020c\u0214\7K\2\2\u020d\u0214\7L\2\2\u020e\u0214\7Y\2\2\u020f"+
		"\u0210\7>\2\2\u0210\u0211\5\64\33\2\u0211\u0212\7?\2\2\u0212\u0214\3\2"+
		"\2\2\u0213\u0208\3\2\2\2\u0213\u0209\3\2\2\2\u0213\u020a\3\2\2\2\u0213"+
		"\u020b\3\2\2\2\u0213\u020c\3\2\2\2\u0213\u020d\3\2\2\2\u0213\u020e\3\2"+
		"\2\2\u0213\u020f\3\2\2\2\u0214U\3\2\2\2\u0215\u0216\7\20\2\2\u0216W\3"+
		"\2\2\2\u0217\u0218\7\17\2\2\u0218Y\3\2\2\2\u0219\u021a\7\16\2\2\u021a"+
		"[\3\2\2\2\u021b\u021c\7\r\2\2\u021c]\3\2\2\2\u021d\u0222\5l\67\2\u021e"+
		"\u021f\7D\2\2\u021f\u0221\5l\67\2\u0220\u021e\3\2\2\2\u0221\u0224\3\2"+
		"\2\2\u0222\u0220\3\2\2\2\u0222\u0223\3\2\2\2\u0223_\3\2\2\2\u0224\u0222"+
		"\3\2\2\2\u0225\u022a\5j\66\2\u0226\u0227\7D\2\2\u0227\u0229\5j\66\2\u0228"+
		"\u0226\3\2\2\2\u0229\u022c\3\2\2\2\u022a\u0228\3\2\2\2\u022a\u022b\3\2"+
		"\2\2\u022ba\3\2\2\2\u022c\u022a\3\2\2\2\u022d\u0232\7Y\2\2\u022e\u022f"+
		"\7D\2\2\u022f\u0231\7Y\2\2\u0230\u022e\3\2\2\2\u0231\u0234\3\2\2\2\u0232"+
		"\u0230\3\2\2\2\u0232\u0233\3\2\2\2\u0233c\3\2\2\2\u0234\u0232\3\2\2\2"+
		"\u0235\u0237\7>\2\2\u0236\u0238\5`\61\2\u0237\u0236\3\2\2\2\u0237\u0238"+
		"\3\2\2\2\u0238\u0239\3\2\2\2\u0239\u023a\7?\2\2\u023ae\3\2\2\2\u023b\u023c"+
		"\7B\2\2\u023c\u023d\5h\65\2\u023d\u023e\7C\2\2\u023e\u0240\3\2\2\2\u023f"+
		"\u023b\3\2\2\2\u0240\u0241\3\2\2\2\u0241\u023f\3\2\2\2\u0241\u0242\3\2"+
		"\2\2\u0242g\3\2\2\2\u0243\u0247\7J\2\2\u0244\u0247\7L\2\2\u0245\u0247"+
		"\5R*\2\u0246\u0243\3\2\2\2\u0246\u0244\3\2\2\2\u0246\u0245\3\2\2\2\u0247"+
		"i\3\2\2\2\u0248\u024a\78\2\2\u0249\u0248\3\2\2\2\u0249\u024a\3\2\2\2\u024a"+
		"\u024b\3\2\2\2\u024b\u024c\5\64\33\2\u024ck\3\2\2\2\u024d\u024e\7Y\2\2"+
		"\u024em\3\2\2\2\u024f\u0257\7S\2\2\u0250\u0257\7U\2\2\u0251\u0252\7F\2"+
		"\2\u0252\u0253\7>\2\2\u0253\u0254\5\64\33\2\u0254\u0255\7?\2\2\u0255\u0257"+
		"\3\2\2\2\u0256\u024f\3\2\2\2\u0256\u0250\3\2\2\2\u0256\u0251\3\2\2\2\u0257"+
		"o\3\2\2\2Gsy{\u0081\u008c\u0097\u00a2\u00ad\u00b1\u00b5\u00bb\u00bf\u00c3"+
		"\u00c9\u00cd\u00d1\u00d7\u00dc\u00e0\u00e3\u00ee\u00f3\u00ff\u0104\u0108"+
		"\u010c\u010f\u0115\u0118\u011e\u0129\u0131\u0139\u0142\u0145\u014c\u0152"+
		"\u0156\u015a\u015e\u0167\u016b\u0182\u0189\u0190\u019e\u01a6\u01ae\u01b6"+
		"\u01be\u01c6\u01ce\u01d6\u01de\u01e6\u01ee\u01f5\u01fb\u0203\u0205\u0213"+
		"\u0222\u022a\u0232\u0237\u0241\u0246\u0249\u0256";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}