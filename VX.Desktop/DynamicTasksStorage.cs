﻿using System;
using System.Collections.Generic;
using System.Linq;
using VX.Desktop.ServiceFacade;
using VX.Domain.DataContracts.Interfaces;

namespace VX.Desktop
{
    public sealed class DynamicTasksStorage : IDynamicTasksStorage
    {
        private const int LowTasksCount = 5;
        private const int EmptyStorageTasksCount = 0;

        public event EventHandler RunningLowOfItems;

        public event EventHandler OutOfItems;
        
        private DynamicTasksStorage()
        {
            items = new List<ITask>();
            RunningLowOfItems += ReplenishItemsAsync;
            OutOfItems += ReplenishItemsAsync;
        }

        static DynamicTasksStorage()
        {
            Instance = new DynamicTasksStorage();
        }

        public static IDynamicTasksStorage Instance { get; set; }

        private readonly List<ITask> items;

        public bool IsReplenishInProgress { get; private set; }

        public ITask RetrieveTask()
        {
            if (items.Count <= EmptyStorageTasksCount)
            {
                OutOfItems(this, null);
                return null;
            }

            if (items.Count <= LowTasksCount)
            {
                RunningLowOfItems(this, null);
            }

            var itemToRetrieve = items.First();

            // TODO: possible threading issues
            items.Remove(itemToRetrieve);
            return itemToRetrieve;
        }

        private void ReplenishItemsAsync(object sender, EventArgs args)
        {
            Func<IList<ITask>> asyncDelegate = VocabServiceFacade.Instance.GetTasks;
            IsReplenishInProgress = true;
            asyncDelegate.BeginInvoke(CallbackMethod, asyncDelegate);
        }

        void CallbackMethod(IAsyncResult asyncResult)
        {
            var asyncDelegate = (Func<IList<ITask>>)asyncResult.AsyncState;
            items.AddRange(asyncDelegate.EndInvoke(asyncResult));
            IsReplenishInProgress = false;
        }
    }
}
