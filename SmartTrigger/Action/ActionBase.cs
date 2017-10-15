namespace Nightwolf.SmartTrigger.Action
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    using Nightwolf.Smartcard;

    internal abstract class ActionBase
    {
        /// <summary>Singleton containing action handler instantiations</summary>
        private static Dictionary<string, ActionBase> actionHandlers;

        /// <summary>Contains state tracking for each action handler</summary>
        private readonly Dictionary<string, object> actionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionBase"/> class.
        /// </summary>
        protected ActionBase()
        {
            this.actionState = new Dictionary<string, object>();
        }

        /// <summary>Gets the action ID</summary>
        internal abstract string ActionId { get; }

        /// <summary>
        /// Intantiate each action handler and save in the action cache
        /// </summary>
        /// <returns>Dictionary of action state</returns>
        internal static ReadOnlyDictionary<string, ActionBase> GetHandlers()
        {
            if (actionHandlers == null)
            {
                actionHandlers = new Dictionary<string, ActionBase>(StringComparer.OrdinalIgnoreCase);

                var targetHandlers = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "Nightwolf.SmartTrigger.Action").ToList();

                foreach (var handler in targetHandlers)
                {
                    var actionProp = handler.GetProperty("ActionId", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (actionProp != null && !handler.IsAbstract)
                    {
                        var handlerClass = (ActionBase)Activator.CreateInstance(handler);
                        var actionId = (string)actionProp.GetValue(handlerClass);
                        actionHandlers.Add(actionId, handlerClass);
                    }
                }
            }

            return new ReadOnlyDictionary<string, ActionBase>(actionHandlers);
        }

        /// <summary>
        /// Insert action method signature
        /// </summary>
        /// <param name="scard">Smartcard object where the insert is being performed</param>
        /// <param name="targetCertSubject">Certificate subject that started this action</param>
        /// <param name="pin">PIN number for smartcard</param>
        /// <param name="parameters">Parameters for the action</param>
        /// <returns>True on action success, false otherwise</returns>
        internal abstract bool PerformInsertAction(Smartcard scard, string targetCertSubject, string pin, IList<Config.Parameter> parameters);

        /// <summary>
        /// Remove action method signature
        /// </summary>
        /// <param name="scard">Smartcard object where the remove is being performed</param>
        /// <param name="parameters">Parameters for the action</param>
        /// <returns>True on action success, false otherwise</returns>
        internal abstract bool PerformRemoveAction(Smartcard scard, IList<Config.Parameter> parameters);

        /// <summary>
        /// Get saved state
        /// </summary>
        /// <typeparam name="T">Type of the state item</typeparam>
        /// <param name="key">Key to the state item</param>
        /// <returns>State value or default(T) if key does not exist</returns>
        protected T GetStateItem<T>(string key)
        {
            if (this.actionState.ContainsKey(key))
            {
                return (T)this.actionState[key];
            }

            return default(T);
        }

        /// <summary>
        /// Save an item into long term state
        /// </summary>
        /// <typeparam name="T">Type of the item</typeparam>
        /// <param name="key">Item key</param>
        /// <param name="value">Item value</param>
        protected void SaveStateItem<T>(string key, T value)
        {
            this.actionState.Add(key, value);
        }
    }
}
