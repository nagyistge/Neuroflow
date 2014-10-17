using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Activities
{
    public abstract class NewFactoryActivity<T> : NewObjectActivity<IFactory<T>>
        where T : class
    {
        [Serializable]
        class ObjectFactory : FactoryBase<T>
        {
            internal ObjectFactory(Type objectType, PropertyValue[] propertyValues, Dictionary<string, object> createdValues)
            {
                Contract.Requires(objectType != null);
                Contract.Requires(propertyValues != null);
                Contract.Requires(createdValues != null);

                this.objectType = objectType;
                this.propertyValues = propertyValues;
                this.createdValues = createdValues;
            }

            Type objectType;

            PropertyValue[] propertyValues;

            Dictionary<string, object> createdValues;

            public override T Create()
            {
                return NewObjectActivityHelpers.CreateObject<T>(objectType, propertyValues, createdValues);
            }
        }
        
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }

        protected override void ValuesCreated(System.Activities.NativeActivityContext context)
        {
            var values = CreatedValues.Get(context);
            var propValues = GetPropertyValues();
            Result.Set(context, new ObjectFactory(ObjectType, propValues.ToArray(), values));
        }
    }
}
