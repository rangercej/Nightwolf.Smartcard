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
                actionHandlers = new Dictionary<string, ActionBase>(StringComparer.OrdinalIgnoreCase);

                var targetHandlers = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(t => t.GetTypes())
                    .Where(t => t.IsClass && t.Namespace == "Nightwolf.SmartTrigger.Action").ToList();

                foreach (var handler in targetHandlers)
                {
                    var fields = handler.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
                    var actionType = fields.FirstOrDefault(f => f.Name == "ActionId");
                    if (!string.IsNullOrEmpty(actionType?.Name))
                    {
                        var handlerClass = (ActionBase)Activator.CreateInstance(handler);
                        actionHandlers.Add((string)actionType.GetValue(null), handlerClass);
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
