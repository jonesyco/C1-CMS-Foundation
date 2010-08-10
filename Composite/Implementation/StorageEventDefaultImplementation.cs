﻿using System;
using Composite.Data;


namespace Composite.Implementation
{
#warning RELEASE: Missing documentation
    public class StorageEventsDefaultImplementation<T> : StorageEvents
        where T: class, IData
    {    
        public override event StorageEventHandler OnBeforeAdd
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataBeforeAdd<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataBeforeAdd(typeof(T), value);
            }
        }



        public override event StorageEventHandler OnAfterAdd
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataAfterAdd<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataAfterAdd(typeof(T), value);
            }
        }



        public override event StorageEventHandler OnBeforeUpdate
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataBeforeUpdate<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataBeforeUpdate(typeof(T), value);
            }
        }



        public override event StorageEventHandler OnAfterUpdate
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataAfterUpdate<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataAfterUpdate(typeof(T), value);
            }
        }



        public override event StorageEventHandler OnDeleted
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataDeleted<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataDeleted(typeof(T), value);
            }
        }



        public override event StorageEventHandler OnNew
        {
            add
            {
                DataEventSystemFacade.SubscribeToDataAfterBuildNew<T>(value);
            }
            remove
            {
                DataEventSystemFacade.UnsubscribeToDataAfterBuildNew(typeof(T), value);
            }
        }
    }
}
