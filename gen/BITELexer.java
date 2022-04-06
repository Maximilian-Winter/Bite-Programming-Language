// Generated from C:/Language Dev 3/Bite Programming Language/Bite/Grammar\BITELexer.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class BITELexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

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
		DQUOTE=78, LPAR=79, RPAR=80, TEXT=81, BACKSLASH_PAREN=82, ESCAPE_SEQUENCE=83, 
		COMMENT=84, WS=85, LINE_COMMENT=86;
	public static final int
		IN_STRING=1;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE", "IN_STRING"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"DeclareModule", "DeclareClass", "DeclareStruct", "DeclareClassInstance", 
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
			"UnterminatedStringLiteral", "DIGIT", "DecimalLiteral", "NONZERODIGIT", 
			"Fractionalconstant", "Exponentpart", "SIGN", "Digitsequence", "IDENTIFIER", 
			"DQUOTE", "LPAR", "RPAR", "TEXT", "BACKSLASH_PAREN", "ESCAPE_SEQUENCE", 
			"DQUOTE_IN_STRING", "COMMENT", "WS", "LINE_COMMENT"
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
			"'^'", "'|'", null, null, "'{'", "'}'", "'['", "']'", "','", "';'", "'$'", 
			null, "'false'", "'true'", null, null, null, null, null, null, null, 
			null, null, null, "'\\('"
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
			"LPAR", "RPAR", "TEXT", "BACKSLASH_PAREN", "ESCAPE_SEQUENCE", "COMMENT", 
			"WS", "LINE_COMMENT"
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


	    int nesting = 0;


	public BITELexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "BITELexer.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	@Override
	public void action(RuleContext _localctx, int ruleIndex, int actionIndex) {
		switch (ruleIndex) {
		case 84:
			LPAR_action((RuleContext)_localctx, actionIndex);
			break;
		case 85:
			RPAR_action((RuleContext)_localctx, actionIndex);
			break;
		case 87:
			BACKSLASH_PAREN_action((RuleContext)_localctx, actionIndex);
			break;
		}
	}
	private void LPAR_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 0:

			    nesting++;
			    pushMode(DEFAULT_MODE);

			break;
		}
	}
	private void RPAR_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 1:

			    if (nesting > 0) {
			        nesting--;
			        popMode();
			    }

			break;
		}
	}
	private void BACKSLASH_PAREN_action(RuleContext _localctx, int actionIndex) {
		switch (actionIndex) {
		case 2:

			    nesting++;
			    pushMode(DEFAULT_MODE);

			break;
		}
	}

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2X\u024f\b\1\b\1\4"+
		"\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n"+
		"\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t"+
		"+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t\62\4\63\t\63\4\64"+
		"\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\4:\t:\4;\t;\4<\t<\4=\t"+
		"=\4>\t>\4?\t?\4@\t@\4A\tA\4B\tB\4C\tC\4D\tD\4E\tE\4F\tF\4G\tG\4H\tH\4"+
		"I\tI\4J\tJ\4K\tK\4L\tL\4M\tM\4N\tN\4O\tO\4P\tP\4Q\tQ\4R\tR\4S\tS\4T\t"+
		"T\4U\tU\4V\tV\4W\tW\4X\tX\4Y\tY\4Z\tZ\4[\t[\4\\\t\\\4]\t]\4^\t^\3\2\3"+
		"\2\3\2\3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4"+
		"\3\4\3\5\3\5\3\5\3\5\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3"+
		"\7\3\b\3\b\3\b\3\b\3\t\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13"+
		"\3\13\3\13\3\f\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\3\r\3\r\3\r"+
		"\3\r\3\16\3\16\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17"+
		"\3\17\3\17\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3\22\3\22\3\22\3\22"+
		"\3\22\3\22\3\22\3\23\3\23\3\23\3\23\3\23\3\23\3\24\3\24\3\24\3\24\3\24"+
		"\3\25\3\25\3\25\3\25\3\25\3\26\3\26\3\26\3\26\3\26\3\26\3\27\3\27\3\27"+
		"\3\27\3\27\3\27\3\27\3\30\3\30\3\31\3\31\3\31\3\32\3\32\3\32\3\33\3\33"+
		"\3\33\3\34\3\34\3\34\3\35\3\35\3\35\3\36\3\36\3\36\3\37\3\37\3\37\3 \3"+
		" \3 \3!\3!\3!\3!\3\"\3\"\3\"\3\"\3#\3#\3#\3$\3$\3$\3%\3%\3%\3&\3&\3&\3"+
		"\'\3\'\3(\3(\3(\3)\3)\3)\3*\3*\3+\3+\3+\3,\3,\3,\3-\3-\3.\3.\3.\3/\3/"+
		"\3\60\3\60\3\60\3\61\3\61\3\62\3\62\3\63\3\63\3\64\3\64\3\65\3\65\3\66"+
		"\3\66\3\67\3\67\3\67\38\38\39\39\3:\3:\3;\3;\3<\3<\3=\3=\3>\3>\3?\3?\3"+
		"@\3@\3A\3A\3B\3B\3C\3C\3D\3D\3E\3E\3F\3F\5F\u01b6\nF\3G\3G\3G\3G\3G\3"+
		"G\3H\3H\3H\3H\3H\3I\3I\3J\3J\5J\u01c7\nJ\3J\3J\3J\5J\u01cc\nJ\3K\3K\3"+
		"K\3L\3L\3L\3L\3L\5L\u01d6\nL\7L\u01d8\nL\fL\16L\u01db\13L\3M\3M\3N\6N"+
		"\u01e0\nN\rN\16N\u01e1\3O\3O\3P\5P\u01e7\nP\3P\3P\3P\3P\3P\5P\u01ee\n"+
		"P\3Q\3Q\5Q\u01f2\nQ\3Q\3Q\3Q\5Q\u01f7\nQ\3Q\5Q\u01fa\nQ\3R\3R\3S\3S\5"+
		"S\u0200\nS\3S\7S\u0203\nS\fS\16S\u0206\13S\3T\3T\7T\u020a\nT\fT\16T\u020d"+
		"\13T\3U\3U\3U\3U\3V\3V\3V\3W\3W\3W\3X\6X\u021a\nX\rX\16X\u021b\3Y\3Y\3"+
		"Y\3Y\3Y\3Z\3Z\3Z\3[\3[\3[\3[\3[\3\\\3\\\3\\\3\\\7\\\u022f\n\\\f\\\16\\"+
		"\u0232\13\\\3\\\3\\\3\\\3\\\3\\\3]\6]\u023a\n]\r]\16]\u023b\3]\3]\3^\3"+
		"^\3^\3^\7^\u0244\n^\f^\16^\u0247\13^\3^\5^\u024a\n^\3^\3^\3^\3^\3\u0230"+
		"\2_\4\3\6\4\b\5\n\6\f\7\16\b\20\t\22\n\24\13\26\f\30\r\32\16\34\17\36"+
		"\20 \21\"\22$\23&\24(\25*\26,\27.\30\60\31\62\32\64\33\66\348\35:\36<"+
		"\37> @!B\"D#F$H%J&L\'N(P)R*T+V,X-Z.\\/^\60`\61b\62d\63f\64h\65j\66l\67"+
		"n8p9r:t;v<x=z>|?~@\u0080A\u0082B\u0084C\u0086D\u0088E\u008aF\u008cG\u008e"+
		"H\u0090I\u0092J\u0094K\u0096L\u0098M\u009a\2\u009cN\u009e\2\u00a0\2\u00a2"+
		"\2\u00a4\2\u00a6\2\u00a8O\u00aaP\u00acQ\u00aeR\u00b0S\u00b2T\u00b4U\u00b6"+
		"\2\u00b8V\u00baW\u00bcX\4\2\3\13\6\2\f\f\17\17$$^^\3\2\62;\3\2\63;\4\2"+
		"--//\5\2C\\aac|\6\2\62;C\\aac|\4\2$$^^\5\2\13\f\16\17\"\"\4\2\f\f\17\17"+
		"\2\u025b\2\4\3\2\2\2\2\6\3\2\2\2\2\b\3\2\2\2\2\n\3\2\2\2\2\f\3\2\2\2\2"+
		"\16\3\2\2\2\2\20\3\2\2\2\2\22\3\2\2\2\2\24\3\2\2\2\2\26\3\2\2\2\2\30\3"+
		"\2\2\2\2\32\3\2\2\2\2\34\3\2\2\2\2\36\3\2\2\2\2 \3\2\2\2\2\"\3\2\2\2\2"+
		"$\3\2\2\2\2&\3\2\2\2\2(\3\2\2\2\2*\3\2\2\2\2,\3\2\2\2\2.\3\2\2\2\2\60"+
		"\3\2\2\2\2\62\3\2\2\2\2\64\3\2\2\2\2\66\3\2\2\2\28\3\2\2\2\2:\3\2\2\2"+
		"\2<\3\2\2\2\2>\3\2\2\2\2@\3\2\2\2\2B\3\2\2\2\2D\3\2\2\2\2F\3\2\2\2\2H"+
		"\3\2\2\2\2J\3\2\2\2\2L\3\2\2\2\2N\3\2\2\2\2P\3\2\2\2\2R\3\2\2\2\2T\3\2"+
		"\2\2\2V\3\2\2\2\2X\3\2\2\2\2Z\3\2\2\2\2\\\3\2\2\2\2^\3\2\2\2\2`\3\2\2"+
		"\2\2b\3\2\2\2\2d\3\2\2\2\2f\3\2\2\2\2h\3\2\2\2\2j\3\2\2\2\2l\3\2\2\2\2"+
		"n\3\2\2\2\2p\3\2\2\2\2r\3\2\2\2\2t\3\2\2\2\2v\3\2\2\2\2x\3\2\2\2\2z\3"+
		"\2\2\2\2|\3\2\2\2\2~\3\2\2\2\2\u0080\3\2\2\2\2\u0082\3\2\2\2\2\u0084\3"+
		"\2\2\2\2\u0086\3\2\2\2\2\u0088\3\2\2\2\2\u008a\3\2\2\2\2\u008c\3\2\2\2"+
		"\2\u008e\3\2\2\2\2\u0090\3\2\2\2\2\u0092\3\2\2\2\2\u0094\3\2\2\2\2\u0096"+
		"\3\2\2\2\2\u0098\3\2\2\2\2\u009c\3\2\2\2\2\u00a8\3\2\2\2\2\u00aa\3\2\2"+
		"\2\2\u00ac\3\2\2\2\2\u00ae\3\2\2\2\3\u00b0\3\2\2\2\3\u00b2\3\2\2\2\3\u00b4"+
		"\3\2\2\2\3\u00b6\3\2\2\2\3\u00b8\3\2\2\2\3\u00ba\3\2\2\2\3\u00bc\3\2\2"+
		"\2\4\u00be\3\2\2\2\6\u00c5\3\2\2\2\b\u00cb\3\2\2\2\n\u00d2\3\2\2\2\f\u00d6"+
		"\3\2\2\2\16\u00df\3\2\2\2\20\u00e3\3\2\2\2\22\u00e7\3\2\2\2\24\u00eb\3"+
		"\2\2\2\26\u00ef\3\2\2\2\30\u00f5\3\2\2\2\32\u00fc\3\2\2\2\34\u0105\3\2"+
		"\2\2\36\u010c\3\2\2\2 \u0114\3\2\2\2\"\u0117\3\2\2\2$\u011c\3\2\2\2&\u0123"+
		"\3\2\2\2(\u0129\3\2\2\2*\u012e\3\2\2\2,\u0133\3\2\2\2.\u0139\3\2\2\2\60"+
		"\u0140\3\2\2\2\62\u0142\3\2\2\2\64\u0145\3\2\2\2\66\u0148\3\2\2\28\u014b"+
		"\3\2\2\2:\u014e\3\2\2\2<\u0151\3\2\2\2>\u0154\3\2\2\2@\u0157\3\2\2\2B"+
		"\u015a\3\2\2\2D\u015e\3\2\2\2F\u0162\3\2\2\2H\u0165\3\2\2\2J\u0168\3\2"+
		"\2\2L\u016b\3\2\2\2N\u016e\3\2\2\2P\u0170\3\2\2\2R\u0173\3\2\2\2T\u0176"+
		"\3\2\2\2V\u0178\3\2\2\2X\u017b\3\2\2\2Z\u017e\3\2\2\2\\\u0180\3\2\2\2"+
		"^\u0183\3\2\2\2`\u0185\3\2\2\2b\u0188\3\2\2\2d\u018a\3\2\2\2f\u018c\3"+
		"\2\2\2h\u018e\3\2\2\2j\u0190\3\2\2\2l\u0192\3\2\2\2n\u0194\3\2\2\2p\u0197"+
		"\3\2\2\2r\u0199\3\2\2\2t\u019b\3\2\2\2v\u019d\3\2\2\2x\u019f\3\2\2\2z"+
		"\u01a1\3\2\2\2|\u01a3\3\2\2\2~\u01a5\3\2\2\2\u0080\u01a7\3\2\2\2\u0082"+
		"\u01a9\3\2\2\2\u0084\u01ab\3\2\2\2\u0086\u01ad\3\2\2\2\u0088\u01af\3\2"+
		"\2\2\u008a\u01b1\3\2\2\2\u008c\u01b5\3\2\2\2\u008e\u01b7\3\2\2\2\u0090"+
		"\u01bd\3\2\2\2\u0092\u01c2\3\2\2\2\u0094\u01cb\3\2\2\2\u0096\u01cd\3\2"+
		"\2\2\u0098\u01d0\3\2\2\2\u009a\u01dc\3\2\2\2\u009c\u01df\3\2\2\2\u009e"+
		"\u01e3\3\2\2\2\u00a0\u01ed\3\2\2\2\u00a2\u01f9\3\2\2\2\u00a4\u01fb\3\2"+
		"\2\2\u00a6\u01fd\3\2\2\2\u00a8\u0207\3\2\2\2\u00aa\u020e\3\2\2\2\u00ac"+
		"\u0212\3\2\2\2\u00ae\u0215\3\2\2\2\u00b0\u0219\3\2\2\2\u00b2\u021d\3\2"+
		"\2\2\u00b4\u0222\3\2\2\2\u00b6\u0225\3\2\2\2\u00b8\u022a\3\2\2\2\u00ba"+
		"\u0239\3\2\2\2\u00bc\u023f\3\2\2\2\u00be\u00bf\7o\2\2\u00bf\u00c0\7q\2"+
		"\2\u00c0\u00c1\7f\2\2\u00c1\u00c2\7w\2\2\u00c2\u00c3\7n\2\2\u00c3\u00c4"+
		"\7g\2\2\u00c4\5\3\2\2\2\u00c5\u00c6\7e\2\2\u00c6\u00c7\7n\2\2\u00c7\u00c8"+
		"\7c\2\2\u00c8\u00c9\7u\2\2\u00c9\u00ca\7u\2\2\u00ca\7\3\2\2\2\u00cb\u00cc"+
		"\7u\2\2\u00cc\u00cd\7v\2\2\u00cd\u00ce\7t\2\2\u00ce\u00cf\7w\2\2\u00cf"+
		"\u00d0\7e\2\2\u00d0\u00d1\7v\2\2\u00d1\t\3\2\2\2\u00d2\u00d3\7p\2\2\u00d3"+
		"\u00d4\7g\2\2\u00d4\u00d5\7y\2\2\u00d5\13\3\2\2\2\u00d6\u00d7\7h\2\2\u00d7"+
		"\u00d8\7w\2\2\u00d8\u00d9\7p\2\2\u00d9\u00da\7e\2\2\u00da\u00db\7v\2\2"+
		"\u00db\u00dc\7k\2\2\u00dc\u00dd\7q\2\2\u00dd\u00de\7p\2\2\u00de\r\3\2"+
		"\2\2\u00df\u00e0\7x\2\2\u00e0\u00e1\7c\2\2\u00e1\u00e2\7t\2\2\u00e2\17"+
		"\3\2\2\2\u00e3\u00e4\7i\2\2\u00e4\u00e5\7g\2\2\u00e5\u00e6\7v\2\2\u00e6"+
		"\21\3\2\2\2\u00e7\u00e8\7u\2\2\u00e8\u00e9\7g\2\2\u00e9\u00ea\7v\2\2\u00ea"+
		"\23\3\2\2\2\u00eb\u00ec\7h\2\2\u00ec\u00ed\7q\2\2\u00ed\u00ee\7t\2\2\u00ee"+
		"\25\3\2\2\2\u00ef\u00f0\7y\2\2\u00f0\u00f1\7j\2\2\u00f1\u00f2\7k\2\2\u00f2"+
		"\u00f3\7n\2\2\u00f3\u00f4\7g\2\2\u00f4\27\3\2\2\2\u00f5\u00f6\7u\2\2\u00f6"+
		"\u00f7\7v\2\2\u00f7\u00f8\7c\2\2\u00f8\u00f9\7v\2\2\u00f9\u00fa\7k\2\2"+
		"\u00fa\u00fb\7e\2\2\u00fb\31\3\2\2\2\u00fc\u00fd\7c\2\2\u00fd\u00fe\7"+
		"d\2\2\u00fe\u00ff\7u\2\2\u00ff\u0100\7v\2\2\u0100\u0101\7t\2\2\u0101\u0102"+
		"\7c\2\2\u0102\u0103\7e\2\2\u0103\u0104\7v\2\2\u0104\33\3\2\2\2\u0105\u0106"+
		"\7r\2\2\u0106\u0107\7w\2\2\u0107\u0108\7d\2\2\u0108\u0109\7n\2\2\u0109"+
		"\u010a\7k\2\2\u010a\u010b\7e\2\2\u010b\35\3\2\2\2\u010c\u010d\7r\2\2\u010d"+
		"\u010e\7t\2\2\u010e\u010f\7k\2\2\u010f\u0110\7x\2\2\u0110\u0111\7c\2\2"+
		"\u0111\u0112\7v\2\2\u0112\u0113\7g\2\2\u0113\37\3\2\2\2\u0114\u0115\7"+
		"k\2\2\u0115\u0116\7h\2\2\u0116!\3\2\2\2\u0117\u0118\7g\2\2\u0118\u0119"+
		"\7n\2\2\u0119\u011a\7u\2\2\u011a\u011b\7g\2\2\u011b#\3\2\2\2\u011c\u011d"+
		"\7t\2\2\u011d\u011e\7g\2\2\u011e\u011f\7v\2\2\u011f\u0120\7w\2\2\u0120"+
		"\u0121\7t\2\2\u0121\u0122\7p\2\2\u0122%\3\2\2\2\u0123\u0124\7d\2\2\u0124"+
		"\u0125\7t\2\2\u0125\u0126\7g\2\2\u0126\u0127\7c\2\2\u0127\u0128\7m\2\2"+
		"\u0128\'\3\2\2\2\u0129\u012a\7p\2\2\u012a\u012b\7w\2\2\u012b\u012c\7n"+
		"\2\2\u012c\u012d\7n\2\2\u012d)\3\2\2\2\u012e\u012f\7v\2\2\u012f\u0130"+
		"\7j\2\2\u0130\u0131\7k\2\2\u0131\u0132\7u\2\2\u0132+\3\2\2\2\u0133\u0134"+
		"\7w\2\2\u0134\u0135\7u\2\2\u0135\u0136\7k\2\2\u0136\u0137\7p\2\2\u0137"+
		"\u0138\7i\2\2\u0138-\3\2\2\2\u0139\u013a\7k\2\2\u013a\u013b\7o\2\2\u013b"+
		"\u013c\7r\2\2\u013c\u013d\7q\2\2\u013d\u013e\7t\2\2\u013e\u013f\7v\2\2"+
		"\u013f/\3\2\2\2\u0140\u0141\7?\2\2\u0141\61\3\2\2\2\u0142\u0143\7-\2\2"+
		"\u0143\u0144\7?\2\2\u0144\63\3\2\2\2\u0145\u0146\7/\2\2\u0146\u0147\7"+
		"?\2\2\u0147\65\3\2\2\2\u0148\u0149\7,\2\2\u0149\u014a\7?\2\2\u014a\67"+
		"\3\2\2\2\u014b\u014c\7\61\2\2\u014c\u014d\7?\2\2\u014d9\3\2\2\2\u014e"+
		"\u014f\7\'\2\2\u014f\u0150\7?\2\2\u0150;\3\2\2\2\u0151\u0152\7(\2\2\u0152"+
		"\u0153\7?\2\2\u0153=\3\2\2\2\u0154\u0155\7~\2\2\u0155\u0156\7?\2\2\u0156"+
		"?\3\2\2\2\u0157\u0158\7`\2\2\u0158\u0159\7?\2\2\u0159A\3\2\2\2\u015a\u015b"+
		"\7>\2\2\u015b\u015c\7>\2\2\u015c\u015d\7?\2\2\u015dC\3\2\2\2\u015e\u015f"+
		"\7@\2\2\u015f\u0160\7@\2\2\u0160\u0161\7?\2\2\u0161E\3\2\2\2\u0162\u0163"+
		"\7~\2\2\u0163\u0164\7~\2\2\u0164G\3\2\2\2\u0165\u0166\7(\2\2\u0166\u0167"+
		"\7(\2\2\u0167I\3\2\2\2\u0168\u0169\7#\2\2\u0169\u016a\7?\2\2\u016aK\3"+
		"\2\2\2\u016b\u016c\7?\2\2\u016c\u016d\7?\2\2\u016dM\3\2\2\2\u016e\u016f"+
		"\7@\2\2\u016fO\3\2\2\2\u0170\u0171\7@\2\2\u0171\u0172\7@\2\2\u0172Q\3"+
		"\2\2\2\u0173\u0174\7@\2\2\u0174\u0175\7?\2\2\u0175S\3\2\2\2\u0176\u0177"+
		"\7>\2\2\u0177U\3\2\2\2\u0178\u0179\7>\2\2\u0179\u017a\7>\2\2\u017aW\3"+
		"\2\2\2\u017b\u017c\7>\2\2\u017c\u017d\7?\2\2\u017dY\3\2\2\2\u017e\u017f"+
		"\7/\2\2\u017f[\3\2\2\2\u0180\u0181\7/\2\2\u0181\u0182\7/\2\2\u0182]\3"+
		"\2\2\2\u0183\u0184\7-\2\2\u0184_\3\2\2\2\u0185\u0186\7-\2\2\u0186\u0187"+
		"\7-\2\2\u0187a\3\2\2\2\u0188\u0189\7\61\2\2\u0189c\3\2\2\2\u018a\u018b"+
		"\7,\2\2\u018be\3\2\2\2\u018c\u018d\7#\2\2\u018dg\3\2\2\2\u018e\u018f\7"+
		"\60\2\2\u018fi\3\2\2\2\u0190\u0191\7A\2\2\u0191k\3\2\2\2\u0192\u0193\7"+
		"<\2\2\u0193m\3\2\2\2\u0194\u0195\7/\2\2\u0195\u0196\7@\2\2\u0196o\3\2"+
		"\2\2\u0197\u0198\7\'\2\2\u0198q\3\2\2\2\u0199\u019a\7\u0080\2\2\u019a"+
		"s\3\2\2\2\u019b\u019c\7(\2\2\u019cu\3\2\2\2\u019d\u019e\7`\2\2\u019ew"+
		"\3\2\2\2\u019f\u01a0\7~\2\2\u01a0y\3\2\2\2\u01a1\u01a2\7*\2\2\u01a2{\3"+
		"\2\2\2\u01a3\u01a4\7+\2\2\u01a4}\3\2\2\2\u01a5\u01a6\7}\2\2\u01a6\177"+
		"\3\2\2\2\u01a7\u01a8\7\177\2\2\u01a8\u0081\3\2\2\2\u01a9\u01aa\7]\2\2"+
		"\u01aa\u0083\3\2\2\2\u01ab\u01ac\7_\2\2\u01ac\u0085\3\2\2\2\u01ad\u01ae"+
		"\7.\2\2\u01ae\u0087\3\2\2\2\u01af\u01b0\7=\2\2\u01b0\u0089\3\2\2\2\u01b1"+
		"\u01b2\7&\2\2\u01b2\u008b\3\2\2\2\u01b3\u01b6\5\u008eG\2\u01b4\u01b6\5"+
		"\u0090H\2\u01b5\u01b3\3\2\2\2\u01b5\u01b4\3\2\2\2\u01b6\u008d\3\2\2\2"+
		"\u01b7\u01b8\7h\2\2\u01b8\u01b9\7c\2\2\u01b9\u01ba\7n\2\2\u01ba\u01bb"+
		"\7u\2\2\u01bb\u01bc\7g\2\2\u01bc\u008f\3\2\2\2\u01bd\u01be\7v\2\2\u01be"+
		"\u01bf\7t\2\2\u01bf\u01c0\7w\2\2\u01c0\u01c1\7g\2\2\u01c1\u0091\3\2\2"+
		"\2\u01c2\u01c3\5\u009cN\2\u01c3\u0093\3\2\2\2\u01c4\u01c6\5\u00a0P\2\u01c5"+
		"\u01c7\5\u00a2Q\2\u01c6\u01c5\3\2\2\2\u01c6\u01c7\3\2\2\2\u01c7\u01cc"+
		"\3\2\2\2\u01c8\u01c9\5\u00a6S\2\u01c9\u01ca\5\u00a2Q\2\u01ca\u01cc\3\2"+
		"\2\2\u01cb\u01c4\3\2\2\2\u01cb\u01c8\3\2\2\2\u01cc\u0095\3\2\2\2\u01cd"+
		"\u01ce\5\u0098L\2\u01ce\u01cf\7$\2\2\u01cf\u0097\3\2\2\2\u01d0\u01d9\7"+
		"$\2\2\u01d1\u01d8\n\2\2\2\u01d2\u01d5\7^\2\2\u01d3\u01d6\13\2\2\2\u01d4"+
		"\u01d6\7\2\2\3\u01d5\u01d3\3\2\2\2\u01d5\u01d4\3\2\2\2\u01d6\u01d8\3\2"+
		"\2\2\u01d7\u01d1\3\2\2\2\u01d7\u01d2\3\2\2\2\u01d8\u01db\3\2\2\2\u01d9"+
		"\u01d7\3\2\2\2\u01d9\u01da\3\2\2\2\u01da\u0099\3\2\2\2\u01db\u01d9\3\2"+
		"\2\2\u01dc\u01dd\t\3\2\2\u01dd\u009b\3\2\2\2\u01de\u01e0\5\u009aM\2\u01df"+
		"\u01de\3\2\2\2\u01e0\u01e1\3\2\2\2\u01e1\u01df\3\2\2\2\u01e1\u01e2\3\2"+
		"\2\2\u01e2\u009d\3\2\2\2\u01e3\u01e4\t\4\2\2\u01e4\u009f\3\2\2\2\u01e5"+
		"\u01e7\5\u00a6S\2\u01e6\u01e5\3\2\2\2\u01e6\u01e7\3\2\2\2\u01e7\u01e8"+
		"\3\2\2\2\u01e8\u01e9\7\60\2\2\u01e9\u01ee\5\u00a6S\2\u01ea\u01eb\5\u00a6"+
		"S\2\u01eb\u01ec\7\60\2\2\u01ec\u01ee\3\2\2\2\u01ed\u01e6\3\2\2\2\u01ed"+
		"\u01ea\3\2\2\2\u01ee\u00a1\3\2\2\2\u01ef\u01f1\7g\2\2\u01f0\u01f2\5\u00a4"+
		"R\2\u01f1\u01f0\3\2\2\2\u01f1\u01f2\3\2\2\2\u01f2\u01f3\3\2\2\2\u01f3"+
		"\u01fa\5\u00a6S\2\u01f4\u01f6\7G\2\2\u01f5\u01f7\5\u00a4R\2\u01f6\u01f5"+
		"\3\2\2\2\u01f6\u01f7\3\2\2\2\u01f7\u01f8\3\2\2\2\u01f8\u01fa\5\u00a6S"+
		"\2\u01f9\u01ef\3\2\2\2\u01f9\u01f4\3\2\2\2\u01fa\u00a3\3\2\2\2\u01fb\u01fc"+
		"\t\5\2\2\u01fc\u00a5\3\2\2\2\u01fd\u0204\5\u009aM\2\u01fe\u0200\7)\2\2"+
		"\u01ff\u01fe\3\2\2\2\u01ff\u0200\3\2\2\2\u0200\u0201\3\2\2\2\u0201\u0203"+
		"\5\u009aM\2\u0202\u01ff\3\2\2\2\u0203\u0206\3\2\2\2\u0204\u0202\3\2\2"+
		"\2\u0204\u0205\3\2\2\2\u0205\u00a7\3\2\2\2\u0206\u0204\3\2\2\2\u0207\u020b"+
		"\t\6\2\2\u0208\u020a\t\7\2\2\u0209\u0208\3\2\2\2\u020a\u020d\3\2\2\2\u020b"+
		"\u0209\3\2\2\2\u020b\u020c\3\2\2\2\u020c\u00a9\3\2\2\2\u020d\u020b\3\2"+
		"\2\2\u020e\u020f\7$\2\2\u020f\u0210\3\2\2\2\u0210\u0211\bU\2\2\u0211\u00ab"+
		"\3\2\2\2\u0212\u0213\7*\2\2\u0213\u0214\bV\3\2\u0214\u00ad\3\2\2\2\u0215"+
		"\u0216\7+\2\2\u0216\u0217\bW\4\2\u0217\u00af\3\2\2\2\u0218\u021a\n\b\2"+
		"\2\u0219\u0218\3\2\2\2\u021a\u021b\3\2\2\2\u021b\u0219\3\2\2\2\u021b\u021c"+
		"\3\2\2\2\u021c\u00b1\3\2\2\2\u021d\u021e\7^\2\2\u021e\u021f\7*\2\2\u021f"+
		"\u0220\3\2\2\2\u0220\u0221\bY\5\2\u0221\u00b3\3\2\2\2\u0222\u0223\7^\2"+
		"\2\u0223\u0224\13\2\2\2\u0224\u00b5\3\2\2\2\u0225\u0226\7$\2\2\u0226\u0227"+
		"\3\2\2\2\u0227\u0228\b[\6\2\u0228\u0229\b[\7\2\u0229\u00b7\3\2\2\2\u022a"+
		"\u022b\7\61\2\2\u022b\u022c\7,\2\2\u022c\u0230\3\2\2\2\u022d\u022f\13"+
		"\2\2\2\u022e\u022d\3\2\2\2\u022f\u0232\3\2\2\2\u0230\u0231\3\2\2\2\u0230"+
		"\u022e\3\2\2\2\u0231\u0233\3\2\2\2\u0232\u0230\3\2\2\2\u0233\u0234\7,"+
		"\2\2\u0234\u0235\7\61\2\2\u0235\u0236\3\2\2\2\u0236\u0237\b\\\b\2\u0237"+
		"\u00b9\3\2\2\2\u0238\u023a\t\t\2\2\u0239\u0238\3\2\2\2\u023a\u023b\3\2"+
		"\2\2\u023b\u0239\3\2\2\2\u023b\u023c\3\2\2\2\u023c\u023d\3\2\2\2\u023d"+
		"\u023e\b]\b\2\u023e\u00bb\3\2\2\2\u023f\u0240\7\61\2\2\u0240\u0241\7\61"+
		"\2\2\u0241\u0245\3\2\2\2\u0242\u0244\n\n\2\2\u0243\u0242\3\2\2\2\u0244"+
		"\u0247\3\2\2\2\u0245\u0243\3\2\2\2\u0245\u0246\3\2\2\2\u0246\u0249\3\2"+
		"\2\2\u0247\u0245\3\2\2\2\u0248\u024a\7\17\2\2\u0249\u0248\3\2\2\2\u0249"+
		"\u024a\3\2\2\2\u024a\u024b\3\2\2\2\u024b\u024c\7\f\2\2\u024c\u024d\3\2"+
		"\2\2\u024d\u024e\b^\b\2\u024e\u00bd\3\2\2\2\30\2\3\u01b5\u01c6\u01cb\u01d5"+
		"\u01d7\u01d9\u01e1\u01e6\u01ed\u01f1\u01f6\u01f9\u01ff\u0204\u020b\u021b"+
		"\u0230\u023b\u0245\u0249\t\7\3\2\3V\2\3W\3\3Y\4\tP\2\6\2\2\b\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}