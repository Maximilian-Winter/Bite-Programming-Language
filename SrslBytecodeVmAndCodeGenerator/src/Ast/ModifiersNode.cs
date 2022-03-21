using Srsl_Parser;
using System.Collections.Generic;

namespace MemoizeSharp
{

    public class ModifiersNode : HeteroAstNode
    {
        public enum ModifierTypes
        {
            DeclarePrivate,
            DeclarePublic,
            DeclareAbstract,
            DeclareStatic
        }

        public List<ModifierTypes> Modifiers;

        #region Public

        public ModifiersNode(Token accessMod, Token staticAbstractMod)
        {
            Modifiers = new List<ModifierTypes>();

            if (accessMod != null && accessMod.type == SrslLexer.DeclarePublic)
            {
                Modifiers.Add(ModifierTypes.DeclarePublic);
            }
            else
            {
                Modifiers.Add(ModifierTypes.DeclarePrivate);
            }

            if (staticAbstractMod != null && staticAbstractMod.type == SrslLexer.DeclareStatic)
            {
                Modifiers.Add(ModifierTypes.DeclareStatic);
            }
            else if (staticAbstractMod != null && staticAbstractMod.type == SrslLexer.DeclareAbstract)
            {
                Modifiers.Add(ModifierTypes.DeclareAbstract);
            }
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion
    }

}
