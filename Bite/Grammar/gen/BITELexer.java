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
		AsKeyword=5, ExternModifier=6, CallableModifier=7, DeclareFunction=8, 
		DeclareVariable=9, DeclareGetter=10, DeclareSetter=11, DeclareForLoop=12, 
		DeclareWhileLoop=13, DeclareStatic=14, DeclareAbstract=15, DeclarePublic=16, 
		DeclarePrivate=17, ControlFlowIf=18, ControlFlowElse=19, FunctionReturn=20, 
		Break=21, NullReference=22, ThisReference=23, UsingDirective=24, ImportDirective=25, 
		StartStatement=26, UseStatement=27, ThreadStatement=28, AssignOperator=29, 
		PlusAssignOperator=30, MinusAssignOperator=31, MultiplyAssignOperator=32, 
		DivideAssignOperator=33, ModuloAssignOperator=34, BitwiseAndAssignOperator=35, 
		BitwiseOrAssignOperator=36, BitwiseXorAssignOperator=37, BitwiseLeftShiftAssignOperator=38, 
		BitwiseRightShiftAssignOperator=39, LogicalOrOperator=40, LogicalAndOperator=41, 
		UnequalOperator=42, EqualOperator=43, GreaterOperator=44, ShiftRightOperator=45, 
		GreaterEqualOperator=46, SmallerOperator=47, ShiftLeftOperator=48, SmallerEqualOperator=49, 
		MinusOperator=50, MinusMinusOperator=51, PlusOperator=52, PlusPlusOperator=53, 
		DivideOperator=54, MultiplyOperator=55, LogicalNegationOperator=56, DotOperator=57, 
		QuestionMarkOperator=58, ColonOperator=59, ReferenceOperator=60, ModuloOperator=61, 
		ComplimentOperator=62, BitwiseAndOperator=63, BitwiseXorOperator=64, BitwiseOrOperator=65, 
		OpeningRoundBracket=66, ClosingRoundBracket=67, SquarebracketLeft=68, 
		SquarebracketRight=69, CommaSeperator=70, SemicolonSeperator=71, DollarOperator=72, 
		BooleanLiteral=73, False_=74, True_=75, IntegerLiteral=76, FloatingLiteral=77, 
		DecimalLiteral=78, Identifier=79, COMMENT=80, WS=81, LINE_COMMENT=82, 
		DQUOTE=83, CURLY_L=84, CURLY_R=85, TEXT=86, BACKSLASH_PAREN=87, ESCAPE_SEQUENCE=88;
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
			"AsKeyword", "ExternModifier", "CallableModifier", "DeclareFunction", 
			"DeclareVariable", "DeclareGetter", "DeclareSetter", "DeclareForLoop", 
			"DeclareWhileLoop", "DeclareStatic", "DeclareAbstract", "DeclarePublic", 
			"DeclarePrivate", "ControlFlowIf", "ControlFlowElse", "FunctionReturn", 
			"Break", "NullReference", "ThisReference", "UsingDirective", "ImportDirective", 
			"StartStatement", "UseStatement", "ThreadStatement", "AssignOperator", 
			"PlusAssignOperator", "MinusAssignOperator", "MultiplyAssignOperator", 
			"DivideAssignOperator", "ModuloAssignOperator", "BitwiseAndAssignOperator", 
			"BitwiseOrAssignOperator", "BitwiseXorAssignOperator", "BitwiseLeftShiftAssignOperator", 
			"BitwiseRightShiftAssignOperator", "LogicalOrOperator", "LogicalAndOperator", 
			"UnequalOperator", "EqualOperator", "GreaterOperator", "ShiftRightOperator", 
			"GreaterEqualOperator", "SmallerOperator", "ShiftLeftOperator", "SmallerEqualOperator", 
			"MinusOperator", "MinusMinusOperator", "PlusOperator", "PlusPlusOperator", 
			"DivideOperator", "MultiplyOperator", "LogicalNegationOperator", "DotOperator", 
			"QuestionMarkOperator", "ColonOperator", "ReferenceOperator", "ModuloOperator", 
			"ComplimentOperator", "BitwiseAndOperator", "BitwiseXorOperator", "BitwiseOrOperator", 
			"OpeningRoundBracket", "ClosingRoundBracket", "SquarebracketLeft", "SquarebracketRight", 
			"CommaSeperator", "SemicolonSeperator", "DollarOperator", "BooleanLiteral", 
			"False_", "True_", "IntegerLiteral", "FloatingLiteral", "DIGIT", "DecimalLiteral", 
			"NONZERODIGIT", "Fractionalconstant", "Exponentpart", "SIGN", "Digitsequence", 
			"Identifier", "COMMENT", "WS", "LINE_COMMENT", "DQUOTE", "CURLY_L", "CURLY_R", 
			"TEXT", "BACKSLASH_PAREN", "ESCAPE_SEQUENCE", "DQUOTE_IN_STRING"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'module'", "'class'", "'struct'", "'new'", "'as'", "'extern'", 
			"'callable'", "'function'", "'var'", "'get'", "'set'", "'for'", "'while'", 
			"'static'", "'abstract'", "'public'", "'private'", "'if'", "'else'", 
			"'return'", "'break'", "'null'", "'this'", "'using'", "'import'", "'start'", 
			"'use'", "'thread'", "'='", "'+='", "'-='", "'*='", "'/='", "'%='", "'&='", 
			"'|='", "'^='", "'<<='", "'>>='", "'||'", "'&&'", "'!='", "'=='", "'>'", 
			"'>>'", "'>='", "'<'", "'<<'", "'<='", "'-'", "'--'", "'+'", "'++'", 
			"'/'", "'*'", "'!'", "'.'", "'?'", "':'", "'->'", "'%'", "'~'", "'&'", 
			"'^'", "'|'", "'('", "')'", "'['", "']'", "','", "';'", "'$'", null, 
			"'false'", "'true'", null, null, null, null, null, null, null, null, 
			"'{'", "'}'", null, "'${'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "DeclareModule", "DeclareClass", "DeclareStruct", "DeclareClassInstance", 
			"AsKeyword", "ExternModifier", "CallableModifier", "DeclareFunction", 
			"DeclareVariable", "DeclareGetter", "DeclareSetter", "DeclareForLoop", 
			"DeclareWhileLoop", "DeclareStatic", "DeclareAbstract", "DeclarePublic", 
			"DeclarePrivate", "ControlFlowIf", "ControlFlowElse", "FunctionReturn", 
			"Break", "NullReference", "ThisReference", "UsingDirective", "ImportDirective", 
			"StartStatement", "UseStatement", "ThreadStatement", "AssignOperator", 
			"PlusAssignOperator", "MinusAssignOperator", "MultiplyAssignOperator", 
			"DivideAssignOperator", "ModuloAssignOperator", "BitwiseAndAssignOperator", 
			"BitwiseOrAssignOperator", "BitwiseXorAssignOperator", "BitwiseLeftShiftAssignOperator", 
			"BitwiseRightShiftAssignOperator", "LogicalOrOperator", "LogicalAndOperator", 
			"UnequalOperator", "EqualOperator", "GreaterOperator", "ShiftRightOperator", 
			"GreaterEqualOperator", "SmallerOperator", "ShiftLeftOperator", "SmallerEqualOperator", 
			"MinusOperator", "MinusMinusOperator", "PlusOperator", "PlusPlusOperator", 
			"DivideOperator", "MultiplyOperator", "LogicalNegationOperator", "DotOperator", 
			"QuestionMarkOperator", "ColonOperator", "ReferenceOperator", "ModuloOperator", 
			"ComplimentOperator", "BitwiseAndOperator", "BitwiseXorOperator", "BitwiseOrOperator", 
			"OpeningRoundBracket", "ClosingRoundBracket", "SquarebracketLeft", "SquarebracketRight", 
			"CommaSeperator", "SemicolonSeperator", "DollarOperator", "BooleanLiteral", 
			"False_", "True_", "IntegerLiteral", "FloatingLiteral", "DecimalLiteral", 
			"Identifier", "COMMENT", "WS", "LINE_COMMENT", "DQUOTE", "CURLY_L", "CURLY_R", 
			"TEXT", "BACKSLASH_PAREN", "ESCAPE_SEQUENCE"
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

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2Z\u0266\b\1\b\1\4"+
		"\2\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n"+
		"\4\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\4*\t*\4+\t"+
		"+\4,\t,\4-\t-\4.\t.\4/\t/\4\60\t\60\4\61\t\61\4\62\t\62\4\63\t\63\4\64"+
		"\t\64\4\65\t\65\4\66\t\66\4\67\t\67\48\t8\49\t9\4:\t:\4;\t;\4<\t<\4=\t"+
		"=\4>\t>\4?\t?\4@\t@\4A\tA\4B\tB\4C\tC\4D\tD\4E\tE\4F\tF\4G\tG\4H\tH\4"+
		"I\tI\4J\tJ\4K\tK\4L\tL\4M\tM\4N\tN\4O\tO\4P\tP\4Q\tQ\4R\tR\4S\tS\4T\t"+
		"T\4U\tU\4V\tV\4W\tW\4X\tX\4Y\tY\4Z\tZ\4[\t[\4\\\t\\\4]\t]\4^\t^\4_\t_"+
		"\4`\t`\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\3\3\3\3\3\3\4\3\4\3\4"+
		"\3\4\3\4\3\4\3\4\3\5\3\5\3\5\3\5\3\6\3\6\3\6\3\7\3\7\3\7\3\7\3\7\3\7\3"+
		"\7\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3\b\3\t\3\t\3\t\3\t\3\t\3\t\3\t\3\t"+
		"\3\t\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3\13\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3"+
		"\r\3\16\3\16\3\16\3\16\3\16\3\16\3\17\3\17\3\17\3\17\3\17\3\17\3\17\3"+
		"\20\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\20\3\21\3\21\3\21\3\21\3\21\3"+
		"\21\3\21\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3\22\3\23\3\23\3\23\3\24\3"+
		"\24\3\24\3\24\3\24\3\25\3\25\3\25\3\25\3\25\3\25\3\25\3\26\3\26\3\26\3"+
		"\26\3\26\3\26\3\27\3\27\3\27\3\27\3\27\3\30\3\30\3\30\3\30\3\30\3\31\3"+
		"\31\3\31\3\31\3\31\3\31\3\32\3\32\3\32\3\32\3\32\3\32\3\32\3\33\3\33\3"+
		"\33\3\33\3\33\3\33\3\34\3\34\3\34\3\34\3\35\3\35\3\35\3\35\3\35\3\35\3"+
		"\35\3\36\3\36\3\37\3\37\3\37\3 \3 \3 \3!\3!\3!\3\"\3\"\3\"\3#\3#\3#\3"+
		"$\3$\3$\3%\3%\3%\3&\3&\3&\3\'\3\'\3\'\3\'\3(\3(\3(\3(\3)\3)\3)\3*\3*\3"+
		"*\3+\3+\3+\3,\3,\3,\3-\3-\3.\3.\3.\3/\3/\3/\3\60\3\60\3\61\3\61\3\61\3"+
		"\62\3\62\3\62\3\63\3\63\3\64\3\64\3\64\3\65\3\65\3\66\3\66\3\66\3\67\3"+
		"\67\38\38\39\39\3:\3:\3;\3;\3<\3<\3=\3=\3=\3>\3>\3?\3?\3@\3@\3A\3A\3B"+
		"\3B\3C\3C\3D\3D\3E\3E\3F\3F\3G\3G\3H\3H\3I\3I\3J\3J\5J\u01da\nJ\3K\3K"+
		"\3K\3K\3K\3K\3L\3L\3L\3L\3L\3M\3M\3N\3N\5N\u01eb\nN\3N\3N\3N\5N\u01f0"+
		"\nN\3O\3O\3P\6P\u01f5\nP\rP\16P\u01f6\3Q\3Q\3R\5R\u01fc\nR\3R\3R\3R\3"+
		"R\3R\5R\u0203\nR\3S\3S\5S\u0207\nS\3S\3S\3S\5S\u020c\nS\3S\5S\u020f\n"+
		"S\3T\3T\3U\3U\5U\u0215\nU\3U\7U\u0218\nU\fU\16U\u021b\13U\3V\3V\7V\u021f"+
		"\nV\fV\16V\u0222\13V\3W\3W\3W\3W\7W\u0228\nW\fW\16W\u022b\13W\3W\3W\3"+
		"W\3W\3W\3X\6X\u0233\nX\rX\16X\u0234\3X\3X\3Y\3Y\3Y\3Y\7Y\u023d\nY\fY\16"+
		"Y\u0240\13Y\3Y\5Y\u0243\nY\3Y\3Y\3Y\3Y\3Z\3Z\3Z\3Z\3[\3[\3[\3[\3\\\3\\"+
		"\3\\\3\\\3]\6]\u0256\n]\r]\16]\u0257\3^\3^\3^\3^\3^\3_\3_\3_\3`\3`\3`"+
		"\3`\3`\3\u0229\2a\4\3\6\4\b\5\n\6\f\7\16\b\20\t\22\n\24\13\26\f\30\r\32"+
		"\16\34\17\36\20 \21\"\22$\23&\24(\25*\26,\27.\30\60\31\62\32\64\33\66"+
		"\348\35:\36<\37> @!B\"D#F$H%J&L\'N(P)R*T+V,X-Z.\\/^\60`\61b\62d\63f\64"+
		"h\65j\66l\67n8p9r:t;v<x=z>|?~@\u0080A\u0082B\u0084C\u0086D\u0088E\u008a"+
		"F\u008cG\u008eH\u0090I\u0092J\u0094K\u0096L\u0098M\u009aN\u009cO\u009e"+
		"\2\u00a0P\u00a2\2\u00a4\2\u00a6\2\u00a8\2\u00aa\2\u00acQ\u00aeR\u00b0"+
		"S\u00b2T\u00b4U\u00b6V\u00b8W\u00baX\u00bcY\u00beZ\u00c0\2\4\2\3\n\3\2"+
		"\62;\3\2\63;\4\2--//\5\2C\\aac|\6\2\62;C\\aac|\5\2\13\f\16\17\"\"\4\2"+
		"\f\f\17\17\5\2$$&&^^\2\u026f\2\4\3\2\2\2\2\6\3\2\2\2\2\b\3\2\2\2\2\n\3"+
		"\2\2\2\2\f\3\2\2\2\2\16\3\2\2\2\2\20\3\2\2\2\2\22\3\2\2\2\2\24\3\2\2\2"+
		"\2\26\3\2\2\2\2\30\3\2\2\2\2\32\3\2\2\2\2\34\3\2\2\2\2\36\3\2\2\2\2 \3"+
		"\2\2\2\2\"\3\2\2\2\2$\3\2\2\2\2&\3\2\2\2\2(\3\2\2\2\2*\3\2\2\2\2,\3\2"+
		"\2\2\2.\3\2\2\2\2\60\3\2\2\2\2\62\3\2\2\2\2\64\3\2\2\2\2\66\3\2\2\2\2"+
		"8\3\2\2\2\2:\3\2\2\2\2<\3\2\2\2\2>\3\2\2\2\2@\3\2\2\2\2B\3\2\2\2\2D\3"+
		"\2\2\2\2F\3\2\2\2\2H\3\2\2\2\2J\3\2\2\2\2L\3\2\2\2\2N\3\2\2\2\2P\3\2\2"+
		"\2\2R\3\2\2\2\2T\3\2\2\2\2V\3\2\2\2\2X\3\2\2\2\2Z\3\2\2\2\2\\\3\2\2\2"+
		"\2^\3\2\2\2\2`\3\2\2\2\2b\3\2\2\2\2d\3\2\2\2\2f\3\2\2\2\2h\3\2\2\2\2j"+
		"\3\2\2\2\2l\3\2\2\2\2n\3\2\2\2\2p\3\2\2\2\2r\3\2\2\2\2t\3\2\2\2\2v\3\2"+
		"\2\2\2x\3\2\2\2\2z\3\2\2\2\2|\3\2\2\2\2~\3\2\2\2\2\u0080\3\2\2\2\2\u0082"+
		"\3\2\2\2\2\u0084\3\2\2\2\2\u0086\3\2\2\2\2\u0088\3\2\2\2\2\u008a\3\2\2"+
		"\2\2\u008c\3\2\2\2\2\u008e\3\2\2\2\2\u0090\3\2\2\2\2\u0092\3\2\2\2\2\u0094"+
		"\3\2\2\2\2\u0096\3\2\2\2\2\u0098\3\2\2\2\2\u009a\3\2\2\2\2\u009c\3\2\2"+
		"\2\2\u00a0\3\2\2\2\2\u00ac\3\2\2\2\2\u00ae\3\2\2\2\2\u00b0\3\2\2\2\2\u00b2"+
		"\3\2\2\2\2\u00b4\3\2\2\2\2\u00b6\3\2\2\2\2\u00b8\3\2\2\2\3\u00ba\3\2\2"+
		"\2\3\u00bc\3\2\2\2\3\u00be\3\2\2\2\3\u00c0\3\2\2\2\4\u00c2\3\2\2\2\6\u00c9"+
		"\3\2\2\2\b\u00cf\3\2\2\2\n\u00d6\3\2\2\2\f\u00da\3\2\2\2\16\u00dd\3\2"+
		"\2\2\20\u00e4\3\2\2\2\22\u00ed\3\2\2\2\24\u00f6\3\2\2\2\26\u00fa\3\2\2"+
		"\2\30\u00fe\3\2\2\2\32\u0102\3\2\2\2\34\u0106\3\2\2\2\36\u010c\3\2\2\2"+
		" \u0113\3\2\2\2\"\u011c\3\2\2\2$\u0123\3\2\2\2&\u012b\3\2\2\2(\u012e\3"+
		"\2\2\2*\u0133\3\2\2\2,\u013a\3\2\2\2.\u0140\3\2\2\2\60\u0145\3\2\2\2\62"+
		"\u014a\3\2\2\2\64\u0150\3\2\2\2\66\u0157\3\2\2\28\u015d\3\2\2\2:\u0161"+
		"\3\2\2\2<\u0168\3\2\2\2>\u016a\3\2\2\2@\u016d\3\2\2\2B\u0170\3\2\2\2D"+
		"\u0173\3\2\2\2F\u0176\3\2\2\2H\u0179\3\2\2\2J\u017c\3\2\2\2L\u017f\3\2"+
		"\2\2N\u0182\3\2\2\2P\u0186\3\2\2\2R\u018a\3\2\2\2T\u018d\3\2\2\2V\u0190"+
		"\3\2\2\2X\u0193\3\2\2\2Z\u0196\3\2\2\2\\\u0198\3\2\2\2^\u019b\3\2\2\2"+
		"`\u019e\3\2\2\2b\u01a0\3\2\2\2d\u01a3\3\2\2\2f\u01a6\3\2\2\2h\u01a8\3"+
		"\2\2\2j\u01ab\3\2\2\2l\u01ad\3\2\2\2n\u01b0\3\2\2\2p\u01b2\3\2\2\2r\u01b4"+
		"\3\2\2\2t\u01b6\3\2\2\2v\u01b8\3\2\2\2x\u01ba\3\2\2\2z\u01bc\3\2\2\2|"+
		"\u01bf\3\2\2\2~\u01c1\3\2\2\2\u0080\u01c3\3\2\2\2\u0082\u01c5\3\2\2\2"+
		"\u0084\u01c7\3\2\2\2\u0086\u01c9\3\2\2\2\u0088\u01cb\3\2\2\2\u008a\u01cd"+
		"\3\2\2\2\u008c\u01cf\3\2\2\2\u008e\u01d1\3\2\2\2\u0090\u01d3\3\2\2\2\u0092"+
		"\u01d5\3\2\2\2\u0094\u01d9\3\2\2\2\u0096\u01db\3\2\2\2\u0098\u01e1\3\2"+
		"\2\2\u009a\u01e6\3\2\2\2\u009c\u01ef\3\2\2\2\u009e\u01f1\3\2\2\2\u00a0"+
		"\u01f4\3\2\2\2\u00a2\u01f8\3\2\2\2\u00a4\u0202\3\2\2\2\u00a6\u020e\3\2"+
		"\2\2\u00a8\u0210\3\2\2\2\u00aa\u0212\3\2\2\2\u00ac\u021c\3\2\2\2\u00ae"+
		"\u0223\3\2\2\2\u00b0\u0232\3\2\2\2\u00b2\u0238\3\2\2\2\u00b4\u0248\3\2"+
		"\2\2\u00b6\u024c\3\2\2\2\u00b8\u0250\3\2\2\2\u00ba\u0255\3\2\2\2\u00bc"+
		"\u0259\3\2\2\2\u00be\u025e\3\2\2\2\u00c0\u0261\3\2\2\2\u00c2\u00c3\7o"+
		"\2\2\u00c3\u00c4\7q\2\2\u00c4\u00c5\7f\2\2\u00c5\u00c6\7w\2\2\u00c6\u00c7"+
		"\7n\2\2\u00c7\u00c8\7g\2\2\u00c8\5\3\2\2\2\u00c9\u00ca\7e\2\2\u00ca\u00cb"+
		"\7n\2\2\u00cb\u00cc\7c\2\2\u00cc\u00cd\7u\2\2\u00cd\u00ce\7u\2\2\u00ce"+
		"\7\3\2\2\2\u00cf\u00d0\7u\2\2\u00d0\u00d1\7v\2\2\u00d1\u00d2\7t\2\2\u00d2"+
		"\u00d3\7w\2\2\u00d3\u00d4\7e\2\2\u00d4\u00d5\7v\2\2\u00d5\t\3\2\2\2\u00d6"+
		"\u00d7\7p\2\2\u00d7\u00d8\7g\2\2\u00d8\u00d9\7y\2\2\u00d9\13\3\2\2\2\u00da"+
		"\u00db\7c\2\2\u00db\u00dc\7u\2\2\u00dc\r\3\2\2\2\u00dd\u00de\7g\2\2\u00de"+
		"\u00df\7z\2\2\u00df\u00e0\7v\2\2\u00e0\u00e1\7g\2\2\u00e1\u00e2\7t\2\2"+
		"\u00e2\u00e3\7p\2\2\u00e3\17\3\2\2\2\u00e4\u00e5\7e\2\2\u00e5\u00e6\7"+
		"c\2\2\u00e6\u00e7\7n\2\2\u00e7\u00e8\7n\2\2\u00e8\u00e9\7c\2\2\u00e9\u00ea"+
		"\7d\2\2\u00ea\u00eb\7n\2\2\u00eb\u00ec\7g\2\2\u00ec\21\3\2\2\2\u00ed\u00ee"+
		"\7h\2\2\u00ee\u00ef\7w\2\2\u00ef\u00f0\7p\2\2\u00f0\u00f1\7e\2\2\u00f1"+
		"\u00f2\7v\2\2\u00f2\u00f3\7k\2\2\u00f3\u00f4\7q\2\2\u00f4\u00f5\7p\2\2"+
		"\u00f5\23\3\2\2\2\u00f6\u00f7\7x\2\2\u00f7\u00f8\7c\2\2\u00f8\u00f9\7"+
		"t\2\2\u00f9\25\3\2\2\2\u00fa\u00fb\7i\2\2\u00fb\u00fc\7g\2\2\u00fc\u00fd"+
		"\7v\2\2\u00fd\27\3\2\2\2\u00fe\u00ff\7u\2\2\u00ff\u0100\7g\2\2\u0100\u0101"+
		"\7v\2\2\u0101\31\3\2\2\2\u0102\u0103\7h\2\2\u0103\u0104\7q\2\2\u0104\u0105"+
		"\7t\2\2\u0105\33\3\2\2\2\u0106\u0107\7y\2\2\u0107\u0108\7j\2\2\u0108\u0109"+
		"\7k\2\2\u0109\u010a\7n\2\2\u010a\u010b\7g\2\2\u010b\35\3\2\2\2\u010c\u010d"+
		"\7u\2\2\u010d\u010e\7v\2\2\u010e\u010f\7c\2\2\u010f\u0110\7v\2\2\u0110"+
		"\u0111\7k\2\2\u0111\u0112\7e\2\2\u0112\37\3\2\2\2\u0113\u0114\7c\2\2\u0114"+
		"\u0115\7d\2\2\u0115\u0116\7u\2\2\u0116\u0117\7v\2\2\u0117\u0118\7t\2\2"+
		"\u0118\u0119\7c\2\2\u0119\u011a\7e\2\2\u011a\u011b\7v\2\2\u011b!\3\2\2"+
		"\2\u011c\u011d\7r\2\2\u011d\u011e\7w\2\2\u011e\u011f\7d\2\2\u011f\u0120"+
		"\7n\2\2\u0120\u0121\7k\2\2\u0121\u0122\7e\2\2\u0122#\3\2\2\2\u0123\u0124"+
		"\7r\2\2\u0124\u0125\7t\2\2\u0125\u0126\7k\2\2\u0126\u0127\7x\2\2\u0127"+
		"\u0128\7c\2\2\u0128\u0129\7v\2\2\u0129\u012a\7g\2\2\u012a%\3\2\2\2\u012b"+
		"\u012c\7k\2\2\u012c\u012d\7h\2\2\u012d\'\3\2\2\2\u012e\u012f\7g\2\2\u012f"+
		"\u0130\7n\2\2\u0130\u0131\7u\2\2\u0131\u0132\7g\2\2\u0132)\3\2\2\2\u0133"+
		"\u0134\7t\2\2\u0134\u0135\7g\2\2\u0135\u0136\7v\2\2\u0136\u0137\7w\2\2"+
		"\u0137\u0138\7t\2\2\u0138\u0139\7p\2\2\u0139+\3\2\2\2\u013a\u013b\7d\2"+
		"\2\u013b\u013c\7t\2\2\u013c\u013d\7g\2\2\u013d\u013e\7c\2\2\u013e\u013f"+
		"\7m\2\2\u013f-\3\2\2\2\u0140\u0141\7p\2\2\u0141\u0142\7w\2\2\u0142\u0143"+
		"\7n\2\2\u0143\u0144\7n\2\2\u0144/\3\2\2\2\u0145\u0146\7v\2\2\u0146\u0147"+
		"\7j\2\2\u0147\u0148\7k\2\2\u0148\u0149\7u\2\2\u0149\61\3\2\2\2\u014a\u014b"+
		"\7w\2\2\u014b\u014c\7u\2\2\u014c\u014d\7k\2\2\u014d\u014e\7p\2\2\u014e"+
		"\u014f\7i\2\2\u014f\63\3\2\2\2\u0150\u0151\7k\2\2\u0151\u0152\7o\2\2\u0152"+
		"\u0153\7r\2\2\u0153\u0154\7q\2\2\u0154\u0155\7t\2\2\u0155\u0156\7v\2\2"+
		"\u0156\65\3\2\2\2\u0157\u0158\7u\2\2\u0158\u0159\7v\2\2\u0159\u015a\7"+
		"c\2\2\u015a\u015b\7t\2\2\u015b\u015c\7v\2\2\u015c\67\3\2\2\2\u015d\u015e"+
		"\7w\2\2\u015e\u015f\7u\2\2\u015f\u0160\7g\2\2\u01609\3\2\2\2\u0161\u0162"+
		"\7v\2\2\u0162\u0163\7j\2\2\u0163\u0164\7t\2\2\u0164\u0165\7g\2\2\u0165"+
		"\u0166\7c\2\2\u0166\u0167\7f\2\2\u0167;\3\2\2\2\u0168\u0169\7?\2\2\u0169"+
		"=\3\2\2\2\u016a\u016b\7-\2\2\u016b\u016c\7?\2\2\u016c?\3\2\2\2\u016d\u016e"+
		"\7/\2\2\u016e\u016f\7?\2\2\u016fA\3\2\2\2\u0170\u0171\7,\2\2\u0171\u0172"+
		"\7?\2\2\u0172C\3\2\2\2\u0173\u0174\7\61\2\2\u0174\u0175\7?\2\2\u0175E"+
		"\3\2\2\2\u0176\u0177\7\'\2\2\u0177\u0178\7?\2\2\u0178G\3\2\2\2\u0179\u017a"+
		"\7(\2\2\u017a\u017b\7?\2\2\u017bI\3\2\2\2\u017c\u017d\7~\2\2\u017d\u017e"+
		"\7?\2\2\u017eK\3\2\2\2\u017f\u0180\7`\2\2\u0180\u0181\7?\2\2\u0181M\3"+
		"\2\2\2\u0182\u0183\7>\2\2\u0183\u0184\7>\2\2\u0184\u0185\7?\2\2\u0185"+
		"O\3\2\2\2\u0186\u0187\7@\2\2\u0187\u0188\7@\2\2\u0188\u0189\7?\2\2\u0189"+
		"Q\3\2\2\2\u018a\u018b\7~\2\2\u018b\u018c\7~\2\2\u018cS\3\2\2\2\u018d\u018e"+
		"\7(\2\2\u018e\u018f\7(\2\2\u018fU\3\2\2\2\u0190\u0191\7#\2\2\u0191\u0192"+
		"\7?\2\2\u0192W\3\2\2\2\u0193\u0194\7?\2\2\u0194\u0195\7?\2\2\u0195Y\3"+
		"\2\2\2\u0196\u0197\7@\2\2\u0197[\3\2\2\2\u0198\u0199\7@\2\2\u0199\u019a"+
		"\7@\2\2\u019a]\3\2\2\2\u019b\u019c\7@\2\2\u019c\u019d\7?\2\2\u019d_\3"+
		"\2\2\2\u019e\u019f\7>\2\2\u019fa\3\2\2\2\u01a0\u01a1\7>\2\2\u01a1\u01a2"+
		"\7>\2\2\u01a2c\3\2\2\2\u01a3\u01a4\7>\2\2\u01a4\u01a5\7?\2\2\u01a5e\3"+
		"\2\2\2\u01a6\u01a7\7/\2\2\u01a7g\3\2\2\2\u01a8\u01a9\7/\2\2\u01a9\u01aa"+
		"\7/\2\2\u01aai\3\2\2\2\u01ab\u01ac\7-\2\2\u01ack\3\2\2\2\u01ad\u01ae\7"+
		"-\2\2\u01ae\u01af\7-\2\2\u01afm\3\2\2\2\u01b0\u01b1\7\61\2\2\u01b1o\3"+
		"\2\2\2\u01b2\u01b3\7,\2\2\u01b3q\3\2\2\2\u01b4\u01b5\7#\2\2\u01b5s\3\2"+
		"\2\2\u01b6\u01b7\7\60\2\2\u01b7u\3\2\2\2\u01b8\u01b9\7A\2\2\u01b9w\3\2"+
		"\2\2\u01ba\u01bb\7<\2\2\u01bby\3\2\2\2\u01bc\u01bd\7/\2\2\u01bd\u01be"+
		"\7@\2\2\u01be{\3\2\2\2\u01bf\u01c0\7\'\2\2\u01c0}\3\2\2\2\u01c1\u01c2"+
		"\7\u0080\2\2\u01c2\177\3\2\2\2\u01c3\u01c4\7(\2\2\u01c4\u0081\3\2\2\2"+
		"\u01c5\u01c6\7`\2\2\u01c6\u0083\3\2\2\2\u01c7\u01c8\7~\2\2\u01c8\u0085"+
		"\3\2\2\2\u01c9\u01ca\7*\2\2\u01ca\u0087\3\2\2\2\u01cb\u01cc\7+\2\2\u01cc"+
		"\u0089\3\2\2\2\u01cd\u01ce\7]\2\2\u01ce\u008b\3\2\2\2\u01cf\u01d0\7_\2"+
		"\2\u01d0\u008d\3\2\2\2\u01d1\u01d2\7.\2\2\u01d2\u008f\3\2\2\2\u01d3\u01d4"+
		"\7=\2\2\u01d4\u0091\3\2\2\2\u01d5\u01d6\7&\2\2\u01d6\u0093\3\2\2\2\u01d7"+
		"\u01da\5\u0096K\2\u01d8\u01da\5\u0098L\2\u01d9\u01d7\3\2\2\2\u01d9\u01d8"+
		"\3\2\2\2\u01da\u0095\3\2\2\2\u01db\u01dc\7h\2\2\u01dc\u01dd\7c\2\2\u01dd"+
		"\u01de\7n\2\2\u01de\u01df\7u\2\2\u01df\u01e0\7g\2\2\u01e0\u0097\3\2\2"+
		"\2\u01e1\u01e2\7v\2\2\u01e2\u01e3\7t\2\2\u01e3\u01e4\7w\2\2\u01e4\u01e5"+
		"\7g\2\2\u01e5\u0099\3\2\2\2\u01e6\u01e7\5\u00a0P\2\u01e7\u009b\3\2\2\2"+
		"\u01e8\u01ea\5\u00a4R\2\u01e9\u01eb\5\u00a6S\2\u01ea\u01e9\3\2\2\2\u01ea"+
		"\u01eb\3\2\2\2\u01eb\u01f0\3\2\2\2\u01ec\u01ed\5\u00aaU\2\u01ed\u01ee"+
		"\5\u00a6S\2\u01ee\u01f0\3\2\2\2\u01ef\u01e8\3\2\2\2\u01ef\u01ec\3\2\2"+
		"\2\u01f0\u009d\3\2\2\2\u01f1\u01f2\t\2\2\2\u01f2\u009f\3\2\2\2\u01f3\u01f5"+
		"\5\u009eO\2\u01f4\u01f3\3\2\2\2\u01f5\u01f6\3\2\2\2\u01f6\u01f4\3\2\2"+
		"\2\u01f6\u01f7\3\2\2\2\u01f7\u00a1\3\2\2\2\u01f8\u01f9\t\3\2\2\u01f9\u00a3"+
		"\3\2\2\2\u01fa\u01fc\5\u00aaU\2\u01fb\u01fa\3\2\2\2\u01fb\u01fc\3\2\2"+
		"\2\u01fc\u01fd\3\2\2\2\u01fd\u01fe\7\60\2\2\u01fe\u0203\5\u00aaU\2\u01ff"+
		"\u0200\5\u00aaU\2\u0200\u0201\7\60\2\2\u0201\u0203\3\2\2\2\u0202\u01fb"+
		"\3\2\2\2\u0202\u01ff\3\2\2\2\u0203\u00a5\3\2\2\2\u0204\u0206\7g\2\2\u0205"+
		"\u0207\5\u00a8T\2\u0206\u0205\3\2\2\2\u0206\u0207\3\2\2\2\u0207\u0208"+
		"\3\2\2\2\u0208\u020f\5\u00aaU\2\u0209\u020b\7G\2\2\u020a\u020c\5\u00a8"+
		"T\2\u020b\u020a\3\2\2\2\u020b\u020c\3\2\2\2\u020c\u020d\3\2\2\2\u020d"+
		"\u020f\5\u00aaU\2\u020e\u0204\3\2\2\2\u020e\u0209\3\2\2\2\u020f\u00a7"+
		"\3\2\2\2\u0210\u0211\t\4\2\2\u0211\u00a9\3\2\2\2\u0212\u0219\5\u009eO"+
		"\2\u0213\u0215\7)\2\2\u0214\u0213\3\2\2\2\u0214\u0215\3\2\2\2\u0215\u0216"+
		"\3\2\2\2\u0216\u0218\5\u009eO\2\u0217\u0214\3\2\2\2\u0218\u021b\3\2\2"+
		"\2\u0219\u0217\3\2\2\2\u0219\u021a\3\2\2\2\u021a\u00ab\3\2\2\2\u021b\u0219"+
		"\3\2\2\2\u021c\u0220\t\5\2\2\u021d\u021f\t\6\2\2\u021e\u021d\3\2\2\2\u021f"+
		"\u0222\3\2\2\2\u0220\u021e\3\2\2\2\u0220\u0221\3\2\2\2\u0221\u00ad\3\2"+
		"\2\2\u0222\u0220\3\2\2\2\u0223\u0224\7\61\2\2\u0224\u0225\7,\2\2\u0225"+
		"\u0229\3\2\2\2\u0226\u0228\13\2\2\2\u0227\u0226\3\2\2\2\u0228\u022b\3"+
		"\2\2\2\u0229\u022a\3\2\2\2\u0229\u0227\3\2\2\2\u022a\u022c\3\2\2\2\u022b"+
		"\u0229\3\2\2\2\u022c\u022d\7,\2\2\u022d\u022e\7\61\2\2\u022e\u022f\3\2"+
		"\2\2\u022f\u0230\bW\2\2\u0230\u00af\3\2\2\2\u0231\u0233\t\7\2\2\u0232"+
		"\u0231\3\2\2\2\u0233\u0234\3\2\2\2\u0234\u0232\3\2\2\2\u0234\u0235\3\2"+
		"\2\2\u0235\u0236\3\2\2\2\u0236\u0237\bX\2\2\u0237\u00b1\3\2\2\2\u0238"+
		"\u0239\7\61\2\2\u0239\u023a\7\61\2\2\u023a\u023e\3\2\2\2\u023b\u023d\n"+
		"\b\2\2\u023c\u023b\3\2\2\2\u023d\u0240\3\2\2\2\u023e\u023c\3\2\2\2\u023e"+
		"\u023f\3\2\2\2\u023f\u0242\3\2\2\2\u0240\u023e\3\2\2\2\u0241\u0243\7\17"+
		"\2\2\u0242\u0241\3\2\2\2\u0242\u0243\3\2\2\2\u0243\u0244\3\2\2\2\u0244"+
		"\u0245\7\f\2\2\u0245\u0246\3\2\2\2\u0246\u0247\bY\2\2\u0247\u00b3\3\2"+
		"\2\2\u0248\u0249\7$\2\2\u0249\u024a\3\2\2\2\u024a\u024b\bZ\3\2\u024b\u00b5"+
		"\3\2\2\2\u024c\u024d\7}\2\2\u024d\u024e\3\2\2\2\u024e\u024f\b[\4\2\u024f"+
		"\u00b7\3\2\2\2\u0250\u0251\7\177\2\2\u0251\u0252\3\2\2\2\u0252\u0253\b"+
		"\\\5\2\u0253\u00b9\3\2\2\2\u0254\u0256\n\t\2\2\u0255\u0254\3\2\2\2\u0256"+
		"\u0257\3\2\2\2\u0257\u0255\3\2\2\2\u0257\u0258\3\2\2\2\u0258\u00bb\3\2"+
		"\2\2\u0259\u025a\7&\2\2\u025a\u025b\7}\2\2\u025b\u025c\3\2\2\2\u025c\u025d"+
		"\b^\4\2\u025d\u00bd\3\2\2\2\u025e\u025f\7^\2\2\u025f\u0260\13\2\2\2\u0260"+
		"\u00bf\3\2\2\2\u0261\u0262\7$\2\2\u0262\u0263\3\2\2\2\u0263\u0264\b`\6"+
		"\2\u0264\u0265\b`\5\2\u0265\u00c1\3\2\2\2\25\2\3\u01d9\u01ea\u01ef\u01f6"+
		"\u01fb\u0202\u0206\u020b\u020e\u0214\u0219\u0220\u0229\u0234\u023e\u0242"+
		"\u0257\7\b\2\2\7\3\2\7\2\2\6\2\2\tU\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}