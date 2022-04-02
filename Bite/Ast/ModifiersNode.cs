using System.Collections.Generic;
using Bite.Parser;

namespace Bite.Ast
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

            if (accessMod != null && accessMod.type == BiteLexer.DeclarePublic)
            {
                Modifiers.Add(ModifierTypes.DeclarePublic);
            }
            else
            {
                Modifiers.Add(ModifierTypes.DeclarePrivate);
            }

            if (staticAbstractMod != null && staticAbstractMod.type == BiteLexer.DeclareStatic)
            {
                Modifiers.Add(ModifierTypes.DeclareStatic);
            }
            else if (staticAbstractMod != null && staticAbstractMod.type == BiteLexer.DeclareAbstract)
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
