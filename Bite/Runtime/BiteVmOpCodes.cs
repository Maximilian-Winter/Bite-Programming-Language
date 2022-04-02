﻿namespace Bite.Runtime
{

    public enum BiteVmOpCodes : byte
    {
        OpNone,
        OpPopStack,
        OpConstant,
        OpAssign,
        OpPlusAssign,
        OpMinusAssign,
        OpMultiplyAssign,
        OpDivideAssign,
        OpModuloAssign,
        OpBitwiseAndAssign,
        OpBitwiseOrAssign,
        OpBitwiseXorAssign,
        OpBitwiseLeftShiftAssign,
        OpBitwiseRightShiftAssign,
        OpAdd,
        OpSubtract,
        OpMultiply,
        OpDivide,
        OpModulo,
        OpEqual,
        OpNotEqual,
        OpSmaller,
        OpSmallerEqual,
        OpGreater,
        OpGreaterEqual,
        OpAnd,
        OpOr,
        OpBitwiseOr,
        OpBitwiseAnd,
        OpBitwiseXor,
        OpBitwiseLeftShift,
        OpBitwiseRightShift,
        OpPostfixIncrement,
        OpPostfixDecrement,
        OpPrefixIncrement,
        OpPrefixDecrement,
        OpNegate,
        OpAffirm,
        OpCompliment,
        OpNot,
        OpDefineModule,
        OpDefineClass,
        OpUsingStatmentHead,
        OpUsingStatmentEnd,
        OpDefineMethod,
        OpDefineLocalVar,
        OpDeclareLocalVar,
        OpSetLocalVar,
        OpGetLocalVar,
        OpGetModule,
        OpDefineLocalInstance,
        OpSetLocalInstance,
        OpGetLocalInstance,
        OpJumpIfFalse,
        OpJump,
        OpTernary,
        OpEnterBlock,
        OpExitBlock,
        OpForLoopHeader,
        OpForLoopBody,
        OpWhileLoop,
        OpBindToFunction,
        OpGetElement,
        OpSetElement,
        OpCallFunction,
        OpCallMemberFunction,
        OpGetMember,
        OpGetMemberWithString,
        OpSetMember,
        OpSetMemberWithString,
        OpKeepLastItemOnStack,
        OpBreak,
        OpReturn
    }

}
