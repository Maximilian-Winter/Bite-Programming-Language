using System.Collections.Generic;

namespace Srsl.Ast
{

    public class ModuleIdentifier : HeteroAstNode
    {

        public Identifier ModuleId;
        private List<Identifier> ParentModules;
        #region Public

        public ModuleIdentifier()
        {
        }

        public ModuleIdentifier(string id, List<string> parentModules)
        {
            ModuleId = new Identifier(id);
            ParentModules = new List<Identifier>();
            foreach (string parentModule in parentModules)
            {
                ParentModules.Add(new Identifier(parentModule));
            }
        }

        public override object Accept(IAstVisitor visitor)
        {
            return visitor.Visit(this);
        }

        #endregion

        public override string ToString()
        {
            string qualifiedName = "";
            foreach (Identifier parentModule in ParentModules)
            {
                qualifiedName += parentModule.Id + ".";
            }

            qualifiedName += ModuleId.Id;
            return qualifiedName;
        }
    }

}
