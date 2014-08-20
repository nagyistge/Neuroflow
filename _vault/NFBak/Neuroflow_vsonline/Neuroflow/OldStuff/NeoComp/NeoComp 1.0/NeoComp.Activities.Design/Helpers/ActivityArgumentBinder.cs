using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.Activities;
using System.Windows;
using System.ComponentModel;

namespace NeoComp.Activities.Design.Helpers
{
    internal sealed class ActivityArgumentBinder
    {
        class Bind : IWeakEventListener
        {
            internal Bind(ModelProperty sourceProp, ModelProperty activityArgProp, IArgHelper helper, bool isArg)
            {
                if (this.isArg = isArg)
                {
                    sourceModelItem = sourceProp.Value;
                    PropertyChangedEventManager.AddListener(sourceModelItem, this, "Name");
                    this.helper = helper;
                    this.activityArgProp = activityArgProp;
                    last = sourceModelItem.Properties["Name"].Value.GetCurrentValue() as string;
                }
                else
                {
                    sourceModelItem = sourceProp.Parent;
                    PropertyChangedEventManager.AddListener(sourceModelItem, this, "ItemResultName");
                    this.helper = helper;
                    this.activityArgProp = activityArgProp;
                    this.sourceProp = sourceProp;
                    last = sourceProp.Value.GetCurrentValue() as string;
                }
            }

            ModelItem sourceModelItem;

            IArgHelper helper;

            ModelProperty activityArgProp, sourceProp;

            string last;

            bool isArg;

            bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (managerType == typeof(PropertyChangedEventManager))
                {
                    string to;
                    if (isArg)
                    {
                        to = sourceModelItem.Properties["Name"].Value.GetCurrentValue() as string;
                    }
                    else
                    {
                        to = sourceProp.Value.GetCurrentValue() as string;
                    }
                    string current = helper.Get(activityArgProp);
                    if (current == last)
                    {
                        helper.Set(activityArgProp, to);
                    }
                    last = to;
                    return true;
                }
                return false;
            }
        }
        
        internal ActivityArgumentBinder(ModelItem activityModelItem)
        {
            Contract.Requires(activityModelItem != null);

            this.activityModelItem = activityModelItem;
            Init();
        }

        ModelItem activityModelItem;

        List<Bind> binds;

        Dictionary<string, ModelProperty> activityArgPropDict;

        private Dictionary<string, ModelProperty> ActivityArgPropDict
        {
            get
            {
                if (activityArgPropDict == null)
                {
                    activityArgPropDict = (from prop in activityModelItem.Properties
                                           where prop.PropertyType.IsSubclassOf(typeof(Argument))
                                           select prop).ToDictionary(p => p.Name, p => p);
                }
                return activityArgPropDict;
            }
        }

        private void Init()
        {
            bool bindResult = true;
            foreach (var parentModelItem in activityModelItem.GetParents())
            {
                if (parentModelItem.Attributes.OfType<CollectionBlueprintActivityMarkAttribute>().Any())
                {
                    var itemResultNameProp = parentModelItem.Properties["ItemResultName"];
                    if (itemResultNameProp != null)
                    {
                        ModelProperty activityArgProp;
                        if (ActivityArgPropDict.TryGetValue("Result", out activityArgProp))
                        {
                            var helper = ArgHelper.Create(activityArgProp);
                            helper.Set(activityArgProp, itemResultNameProp.Value.GetCurrentValue() as string);
                            AddBind(itemResultNameProp, activityArgProp, helper, false);
                        }
                    }
                    bindResult = false;
                }
                else
                {
                    var parentObj = parentModelItem.GetCurrentValue();
                    if (parentObj != null)
                    {
                        if (parentObj is ActivityDelegate)
                        {
                            var metadata = (from pp in parentModelItem.Parent.Properties
                                            where pp.Value == parentModelItem
                                            from a in pp.Attributes.OfType<ActivityDelegateMetadataAttribute>()
                                            select a).FirstOrDefault();
                            if (metadata == null) metadata = new ActivityDelegateMetadataAttribute();
                            Init(parentModelItem, metadata, bindResult);
                            return;
                        }
                    }
                }
            }
        }

        private void Init(ModelItem delegateModelItem, ActivityDelegateMetadataAttribute metadata, bool bindResult)
        {
            if (bindResult) InitArg(delegateModelItem, "Result", metadata.ResultName);
            if (InitArg(delegateModelItem, "Argument", metadata.Argument1Name)) return;
            if (!InitArg(delegateModelItem, "Argument1", metadata.Argument1Name)) return;
            if (!InitArg(delegateModelItem, "Argument2", metadata.Argument2Name)) return;
            if (!InitArg(delegateModelItem, "Argument3", metadata.Argument3Name)) return;
            if (!InitArg(delegateModelItem, "Argument4", metadata.Argument4Name)) return;
            if (!InitArg(delegateModelItem, "Argument5", metadata.Argument5Name)) return;
            if (!InitArg(delegateModelItem, "Argument6", metadata.Argument6Name)) return;
            if (!InitArg(delegateModelItem, "Argument7", metadata.Argument7Name)) return;
            if (!InitArg(delegateModelItem, "Argument8", metadata.Argument8Name)) return;
        }

        private bool InitArg(ModelItem delegateModelItem, string delegateArgName, string delegateArgNameFromMetadata)
        {
            var delegateArgProp = delegateModelItem.Properties[delegateArgName];
            if (delegateArgProp != null)
            {
                ModelProperty activityArgProp;
                if (ActivityArgPropDict.TryGetValue(delegateArgNameFromMetadata, out activityArgProp))
                {
                    var helper = ArgHelper.Create(delegateArgProp);
                    helper.Set(activityArgProp, delegateArgProp.Value.Properties["Name"].Value.GetCurrentValue() as string);
                    AddBind(delegateArgProp, activityArgProp, helper, true);
                }
                return true;
            }
            return false;
        }

        private void AddBind(ModelProperty delegateArgProp, ModelProperty activityArgProp, IArgHelper helper, bool isArg)
        {
            if (binds == null) binds = new List<Bind>();
            binds.Add(new Bind(delegateArgProp, activityArgProp, helper, isArg));
        }
    }
}
