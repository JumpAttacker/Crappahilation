using System.Windows.Input;

using Divine.Input;

namespace InvokerCrappahilationPaid.InvokerStuff.npc_dota_hero_invoker
{
    public interface IHaveFastInvokeKey
    {
        Key Key { get; set; }


        //System.Windows.Input.Key GetKey(int key);
        //var key = KeyInterop.KeyFromVirtualKey((int)info.Key);
    }
}