//-------------------------------------------------------------------------------------------------
// <copyright file="ObservableCollectionWrapper.cs" company="Microsoft" author="Bailin Wei">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace TestViewer.Utilities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Threading;
    
    using TestViewer.ViewModels;
    using System.Reflection;

    /// <summary>
    /// ObservableCollection wrapper
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    public class MTObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Override the event so this class can access it
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// On collection changed event
        /// </summary>
        /// <param name="e">Notify collection changed event args</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (this.BlockReentrancy())
            {
                NotifyCollectionChangedEventHandler eventHandler = this.CollectionChanged;
                if (eventHandler == null)
                {
                    return;
                }

                Delegate[] delegates = eventHandler.GetInvocationList();

                // Walk thru invocation list
                try
                {
                    foreach (NotifyCollectionChangedEventHandler handler in delegates)
                    {
                        DispatcherObject dispatcherObject = handler.Target as DispatcherObject;

                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                        {
                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
                        }
                        else
                        {
                            // Execute handler as is
                            handler(this, e);
                        }
                    }
                }
                catch (TargetInvocationException ex)
                {
                    LoggerViewModel.Log(string.Format("Throw a TargetInvocationException: {0}.", ex.Message), Colors.Red);   
                }
                catch (Exception ex)
                {
                    LoggerViewModel.Log(string.Format("Throw an Exception: {0}.", ex.Message), Colors.Red);   
                }
                
            }
        }
    }

    /// <summary>
    /// Similar impliment as above...
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionWrapper<T> : ObservableCollection<T>
    {
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var eh = CollectionChanged;
            if (eh != null)
            {
                Dispatcher dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
                                         let dpo = nh.Target as DispatcherObject
                                         where dpo != null
                                         select dpo.Dispatcher).FirstOrDefault();

                if (dispatcher != null && dispatcher.CheckAccess() == false)
                {
                    dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
                }
                else
                {
                    foreach (NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
                        nh.Invoke(this, e);
                }
            }
        }
    } 
 
}
