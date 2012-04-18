﻿using Composite.C1Console.Security;
using System.Collections.Generic;
using Composite.Core.Serialization;
using System.Text;


namespace Composite.C1Console.Trees.Foundation
{
    [SecurityAncestorProvider(typeof(Composite.C1Console.Security.SecurityAncestorProviders.NoAncestorSecurityAncestorProvider))]
    internal class TreePerspectiveEntityToken : EntityToken
    {
        private readonly string _id;
        private List<string> _childTrees = new List<string>();


        public TreePerspectiveEntityToken(string id)
        {
            _id = id;
        }


        public override string Id
        {
            get { return _id; }
        }


        public override string Type
        {
            get { return "C1Trees"; }
        }


        public override string Source
        {
            get { return "C1Trees"; }
        }


        public void AddChildTree(string treeId)
        {
            _childTrees.Add(treeId);
        }


        public override string Serialize()
        {
            return Id;
            /*StringBuilder sb = new StringBuilder();
            
            StringConversionServices.SerializeKeyValuePair(sb, "Id", Id);
            
            int counter = 0;
            foreach (string treeId in _childTrees)
            {
                string key = "TreeId" + (counter++);
                StringConversionServices.SerializeKeyValuePair(sb, key, treeId);
            }

            return sb.ToString();*/
        }


        public static EntityToken Deserialize(string serializedEntityToken)
        {
           /* Dictionary<string, string> dic = StringConversionServices.ParseKeyValueCollection(serializedEntityToken);

            string id = StringConversionServices.DeserializeValueString(dic["Id"]);

            int count = 0;
            while (true)
            {
                string key = "TreeId" + (counter++);
            }*/

            return new TreePerspectiveEntityToken(serializedEntityToken);
        }
    }
}
