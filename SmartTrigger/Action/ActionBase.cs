using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nightwolf.SmartTrigger.Action
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    using Nightwolf.Smartcard;

    internal abstract class ActionBase
    {
        private static Dictionary<string, ActionBase> actionHandlers;

        internal static ReadOnlyDictionary<string, ActionBase> GetHandlers()
        {
            if (actionHandlers == null)
            {
                actionHandlers = new Dictionary<string, ActionBase>();

                var targetHandlers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "Nightwolf.SmartTrigger.Action");

                foreach (var handler in targetHandlers)
                {
                    var actionType = handler.GetFields(BindingFlags.Static).First(f => f.Name == "ActionId");
                    if (actionType.Name != string.Empty)
                    {
                        var handlerClass = (ActionBase)Activator.CreateInstance(handler);
                        actionHandlers.Add(actionType.Name, handlerClass);
                    }
                }
            }

            return new ReadOnlyDictionary<string, ActionBase>(actionHandlers);
        }

        internal string ActionId = null;

        abstract internal bool PerformInsertAction(Smartcard scard, string targetCertSubject, string pin, IList<Config.Parameter> parameters);

        abstract internal bool PerformRemoveAction(Smartcard scard, IList<Config.Parameter> parameters);
    }
}
