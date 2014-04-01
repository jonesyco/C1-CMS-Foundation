﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Composite.Core.Extensions;
using Composite.Data;
using Composite.Data.ProcessControlled;
using Composite.Core.Types;
using Composite.Data.Types;
using Texts = Composite.Core.ResourceSystem.LocalizationFiles.Composite_Core_PackageSystem_PackageFragmentInstallers;

namespace Composite.Core.PackageSystem.PackageFragmentInstallers
{
    /// <summary>    
    /// </summary>
    /// <exclude />
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)] 
    public sealed class DataPackageFragmentUninstaller : BasePackageFragmentUninstaller
    {
        private List<DataType> _datasToDelete = null;



        /// <exclude />
        public override IEnumerable<PackageFragmentValidationResult> Validate()
        {
            List<PackageFragmentValidationResult> validationResult = new List<PackageFragmentValidationResult>();

            if (this.Configuration.Count(f => f.Name == "Types") > 1)
            {
                validationResult.AddFatal(Texts.DataPackageFragmentUninstaller_OnlyOneElement);
                return validationResult;
            }

            _datasToDelete = new List<DataType>();

            XElement typesElement = this.Configuration.SingleOrDefault(f => f.Name == "Types");

            if (typesElement != null)
            {
                foreach (XElement typeElement in typesElement.Elements("Type").Reverse())
                {
                    XAttribute typeAttribute = typeElement.Attribute("type");
                    XAttribute dataScopeIdentifierAttribute = typeElement.Attribute("dataScopeIdentifier");

                    if (typeAttribute == null) 
                    {
                        validationResult.AddFatal(Texts.DataPackageFragmentUninstaller_MissingAttribute("type"), typeElement); 
                        continue; 
                    }

                    if (dataScopeIdentifierAttribute == null) 
                    {
                        validationResult.AddFatal(Texts.DataPackageFragmentUninstaller_MissingAttribute("dataScopeIdentifier"), typeElement); 
                        continue; 
                    }

                    Type type = TypeManager.TryGetType(typeAttribute.Value);
                    if (type == null) continue;

                    if (!DataFacade.GetAllInterfaces().Contains(type)) continue;


                    DataScopeIdentifier dataScopeIdentifier;
                    try
                    {
                        dataScopeIdentifier = DataScopeIdentifier.Deserialize(dataScopeIdentifierAttribute.Value);
                    }
                    catch (Exception)
                    {
                        validationResult.AddFatal("Wrong DataScopeIdentifier ({0}) name in the configuration".FormatWith(dataScopeIdentifierAttribute.Value), dataScopeIdentifierAttribute);
                        continue;
                    }


                    foreach (XElement datasElement in typeElement.Elements("Datas").Reverse())
                    {
                        CultureInfo locale = null;

                        XAttribute localeAttribute = datasElement.Attribute("locale");
                        if (localeAttribute != null)
                        {
                            locale = CultureInfo.CreateSpecificCulture(localeAttribute.Value);
                        }

                        foreach (XElement keysElement in datasElement.Elements("Keys"))
                        {
                            bool allKeyPropertiesValidated = true;
                            var dataKeyPropertyCollection = new DataKeyPropertyCollection();

                            foreach (XElement keyElement in keysElement.Elements("Key"))
                            {
                                XAttribute keyNameAttribute = keyElement.Attribute("name");
                                XAttribute keyValueAttribute = keyElement.Attribute("value");


                                if (keyNameAttribute == null || keyValueAttribute == null)
                                {
                                    if (keyNameAttribute == null) validationResult.AddFatal(GetText("DataPackageFragmentUninstaller.MissingAttribute").FormatWith("name"), keyElement);
                                    if (keyValueAttribute == null) validationResult.AddFatal(GetText("DataPackageFragmentUninstaller.MissingAttribute").FormatWith("value"), keyElement);

                                    allKeyPropertiesValidated = false;
                                    continue;
                                }
                                
                                string keyName = keyNameAttribute.Value;
                                PropertyInfo keyPropertyInfo = type.GetPropertiesRecursively().SingleOrDefault(f => f.Name == keyName);
                                if (keyPropertyInfo == null)
                                {
                                    validationResult.AddFatal(GetText("DataPackageFragmentUninstaller.MissingKeyProperty").FormatWith(type, keyName));
                                    allKeyPropertiesValidated = false;
                                }
                                else
                                {
                                    try
                                    {
                                        object keyValue = ValueTypeConverter.Convert(keyValueAttribute.Value, keyPropertyInfo.PropertyType);
                                        dataKeyPropertyCollection.AddKeyProperty(keyName, keyValue);
                                    }
                                    catch (Exception)
                                    {
                                        allKeyPropertiesValidated = false;
                                        validationResult.AddFatal(GetText("DataPackageFragmentUninstaller.DataPackageFragmentUninstaller").FormatWith(keyValueAttribute.Value, keyPropertyInfo.PropertyType));
                                    }
                                }
                            }

                            var pagesReferencingPageTypes = new HashSet<string>();

                            if (allKeyPropertiesValidated)
                            {
                                IData data;
                                using (new DataScope(dataScopeIdentifier, locale))
                                {
                                    data = DataFacade.TryGetDataByUniqueKey(type, dataKeyPropertyCollection);
                                }

                                if (data != null)
                                {
                                    List<IData> referees = data.GetReferees();

                                    bool addToDelete = true;
                                    foreach (IData referee in referees)
                                    {
                                        if (!this.UninstallerContext.IsPendingForDeletionData(referee))
                                        {
                                            addToDelete = false;

                                            if (referee is IPage && data is IPageType)
                                            {
                                                string pathToPage;

                                                using (new DataScope(referee.DataSourceId.PublicationScope, referee.DataSourceId.LocaleScope))
                                                {
                                                    pathToPage = GetPathToPage(referee as IPage);
                                                }

                                                if (!pagesReferencingPageTypes.Contains(pathToPage))
                                                {
                                                    validationResult.AddFatal(Texts.DataPackageFragmentUninstaller_PageTypeIsReferenced(data.GetLabel(), pathToPage));
                                                    pagesReferencingPageTypes.Add(pathToPage);
                                                }
                                            }
                                            else
                                            {
                                                var refereeType = referee.DataSourceId.InterfaceType;

                                                validationResult.AddFatal(Texts.DataPackageFragmentUninstaller_DataIsReferenced(
                                                    data.GetLabel(),
                                                    type.FullName,
                                                    referee.GetLabel(),
                                                    refereeType.FullName));
                                            }
                                        }
                                    }

                                    if (addToDelete)
                                    {
                                        AddDataToDelete(type, dataScopeIdentifier, locale, dataKeyPropertyCollection);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (validationResult.Count > 0)
            {
                _datasToDelete = null;
            }

            return validationResult;
        }

        private static string GetPathToPage(IPage page)
        {
            var parentPageId = PageManager.GetParentId(page.Id);

            if (parentPageId != Guid.Empty)
            {
                var parentPage = PageManager.GetPageById(parentPageId);

                if (parentPage != null)
                {
                    return GetPathToPage(parentPage) + "/" + page.Title;
                }
            }

            return page.Title;
        }

        /// <exclude />
        public override void Uninstall()
        {
            if (_datasToDelete == null) throw new InvalidOperationException("DataPackageFragmentUninstaller has not been validated");

            foreach (DataType dataType in _datasToDelete)
            {
                using (new DataScope(dataType.DataScopeIdentifier, dataType.Locale))
                {
                    Log.LogVerbose("DataPackageFragmentUninstaller", string.Format("Uninstalling data for the type '{0}'", dataType.InterfaceType));

                    foreach (DataKeyPropertyCollection dataKeyPropertyCollection in dataType.DataKeys)
                    {
                        IData data = DataFacade.TryGetDataByUniqueKey(dataType.InterfaceType, dataKeyPropertyCollection);

                        using (ProcessControllerFacade.NoProcessControllers)
                        {
                            DataFacade.Delete(data, CascadeDeleteType.Disable);
                        }
                    }
                }
            }
        }



        private void AddDataToDelete(Type interfaceType, DataScopeIdentifier dataScopeIdentifier, CultureInfo locale, DataKeyPropertyCollection dataKeyPropertyCollection)
        {
            DataType dataType =
                (from dt in _datasToDelete
                 where dt.InterfaceType == interfaceType &&
                       dt.DataScopeIdentifier.Equals(dataScopeIdentifier) &&
                       (((dt.Locale == null) && (locale == null)) || (dt.Locale.Equals(locale)))
                 select dt).SingleOrDefault();

            if (dataType == null)
            {
                dataType = new DataType { InterfaceType = interfaceType, DataScopeIdentifier = dataScopeIdentifier, Locale = locale };
                _datasToDelete.Add(dataType);
            }

            dataType.DataKeys.Add(dataKeyPropertyCollection);
            this.UninstallerContext.AddPendingForDeletionData(interfaceType, dataScopeIdentifier, locale, dataKeyPropertyCollection);
        }



        private static string GetText(string stringId)
        {
            return GetResourceString(stringId);
        }


        private sealed class DataType
        {
            public DataType()
            {
                this.DataKeys = new List<DataKeyPropertyCollection>();
            }

            public Type InterfaceType { get; set; }
            public DataScopeIdentifier DataScopeIdentifier { get; set; }
            public CultureInfo Locale { get; set; }
            public List<DataKeyPropertyCollection> DataKeys { get; set; }
        }
    }
}
