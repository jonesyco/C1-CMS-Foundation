﻿using System;
using System.Collections.Generic;
using System.Linq;
using Composite.Data;
using Composite.Data.Types;
using Composite.C1Console.Elements;
using Composite.C1Console.Elements.Plugins.ElementProvider;
using Composite.Core.ResourceSystem;
using Composite.Core.ResourceSystem.Icons;
using Composite.C1Console.Security;
using Composite.Plugins.Elements.ElementProviders.BaseFunctionProviderElementProvider;
using Composite.C1Console.Workflow;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ObjectBuilder;
using Microsoft.Practices.ObjectBuilder;


namespace Composite.Plugins.Elements.ElementProviders.VisualFunctionProviderElementProvider
{
    [ConfigurationElementType(typeof(VisualFunctionProviderElementProviderData))]
    internal sealed class VisualFunctionProviderElementProvider : BaseFunctionProviderElementProvider.BaseFunctionProviderElementProvider
    {
        public static ResourceHandle AddFunction { get { return GetIconHandle("visual-function-add"); } }
        public static ResourceHandle EditFunction { get { return GetIconHandle("visual-function-edit"); } }
        public static ResourceHandle DeleteFunction { get { return GetIconHandle("visual-function-delete"); } }

        private static readonly ActionGroup PrimaryActionGroup = new ActionGroup(ActionGroupPriority.PrimaryHigh);

        public VisualFunctionProviderElementProvider()
        {
        }

        protected override string RootFolderLabel
        {
            get
            {
                return StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.RootFolderLabel");
            }
        }


        protected override string RootFolderToolTip
        {
            get
            {
                return StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.RootFolderToolTip");
            }
        }


        protected override IEnumerable<IFunctionTreeBuilderLeafInfo> OnGetFunctionInfos(SearchToken searchToken)
        {
            if (searchToken.IsValidKeyword() == false)
            {
                return
                    from function in DataFacade.GetData<IVisualFunction>()
                    select (IFunctionTreeBuilderLeafInfo)new VisualFunctionTreeBuilderLeafInfo(function);
            }
            else
            {
                string keyword = searchToken.Keyword.ToLowerInvariant();

                return
                    from function in DataFacade.GetData<IVisualFunction>()
                    where function.Name.ToLowerInvariant().Contains(keyword) ||
                          function.Namespace.ToLowerInvariant().Contains(keyword) ||
                          function.TypeManagerName.ToLowerInvariant().Contains(keyword)
                    select (IFunctionTreeBuilderLeafInfo)new VisualFunctionTreeBuilderLeafInfo(function);
            }
        }



        protected override IEnumerable<Type> OnGetEntityTokenTypes()
        {
            yield return typeof(DataEntityToken);
        }



        protected override IFunctionTreeBuilderLeafInfo OnIsEntityOwner(EntityToken entityToken)
        {
            DataEntityToken dataEntityToken = entityToken as DataEntityToken;
            if (dataEntityToken == null) return null;

            if (dataEntityToken.InterfaceType != typeof(IVisualFunction)) return null;

            return new VisualFunctionTreeBuilderLeafInfo(dataEntityToken.Data as IVisualFunction);
        }



        protected override IEnumerable<ElementAction> OnGetFolderActions()
        {
            return new ElementAction[]
                {
                    new ElementAction(new ActionHandle(
                        new WorkflowActionToken(
                            WorkflowFacade.GetWorkflowType("Composite.Plugins.Elements.ElementProviders.VisualFunctionProviderElementProvider.AddNewVisualFunctionWorkflow"),
                            new PermissionType[] { PermissionType.Add }
                        )))
                    {
                        VisualData = new ActionVisualizedData
                        {
                            Label = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.AddNewLabel"),
                            ToolTip = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.AddNewToolTip"),
                            Icon = VisualFunctionProviderElementProvider.AddFunction,
                            Disabled = false,
                            ActionLocation = new ActionLocation
                            {
                                ActionType = ActionType.Add,
                                IsInFolder = false,
                                IsInToolbar = true,
                                ActionGroup = PrimaryActionGroup
                            }
                        }
                    }
                };
        }



        protected override IEnumerable<ElementAction> OnGetFunctionActions(IFunctionTreeBuilderLeafInfo function)
        {
            return new ElementAction[] 
                {
                    new ElementAction(new ActionHandle(
                        new WorkflowActionToken(
                            WorkflowFacade.GetWorkflowType("Composite.Plugins.Elements.ElementProviders.VisualFunctionProviderElementProvider.EditVisualFunctionWorkflow"),
                            new PermissionType[] { PermissionType.Edit }
                        ))) {
                        VisualData = new ActionVisualizedData { 
                            Label = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.EditLabel"),
                            ToolTip = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.EditToolTip"),
                            Icon = VisualFunctionProviderElementProvider.EditFunction,
                            Disabled = false, 
                            ActionLocation = new ActionLocation { 
                                ActionType = ActionType.Edit,
                                IsInFolder = false,
                                IsInToolbar = true,
                               ActionGroup = PrimaryActionGroup
                            }
                        }
                    },
                    new ElementAction(new ActionHandle(
                        new WorkflowActionToken(
                            WorkflowFacade.GetWorkflowType("Composite.Plugins.Elements.ElementProviders.VisualFunctionProviderElementProvider.DeleteVisualFunctionWorkflow"),
                            new PermissionType[] { PermissionType.Delete }
                        ) { Payload = GetContext().ProviderName })) {
                        VisualData = new ActionVisualizedData { 
                            Label = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.DeleteLabel"),
                            ToolTip = StringResourceSystemFacade.GetString("Composite.Plugins.VisualFunction", "VisualFunctionElementProvider.DeleteToolTip"),
                            Icon = VisualFunctionProviderElementProvider.DeleteFunction,
                            Disabled = false, 
                            ActionLocation = new ActionLocation { 
                                ActionType = ActionType.Delete,
                                IsInFolder = false,
                                IsInToolbar = true,
                               ActionGroup = PrimaryActionGroup
                            }
                        }
                    }
                };
        }




        private sealed class VisualFunctionTreeBuilderLeafInfo : IFunctionTreeBuilderLeafInfo
        {
            private IVisualFunction _function;

            public VisualFunctionTreeBuilderLeafInfo(IVisualFunction function)
            {
                _function = function;
            }

            public string Name
            {
                get { return _function.Name; }
            }

            public string Namespace
            {
                get { return _function.Namespace; }
            }

            public EntityToken EntityToken
            {
                get { return _function.GetDataEntityToken(); }
            }
        }
    }




    [Assembler(typeof(VisualFunctionProviderElementProviderAssembler))]
    internal sealed class VisualFunctionProviderElementProviderData : HooklessElementProviderData
    {
    }




    internal sealed class VisualFunctionProviderElementProviderAssembler : IAssembler<IHooklessElementProvider, HooklessElementProviderData>
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public IHooklessElementProvider Assemble(IBuilderContext context, HooklessElementProviderData objectConfiguration, IConfigurationSource configurationSource, ConfigurationReflectionCache reflectionCache)
        {
            return new VisualFunctionProviderElementProvider();
        }
    }
}
