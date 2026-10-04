using CCLBStudio.DependencyInjection;
using Game.Player;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    [CreateAssetMenu(menuName = "Reaali/Systems/Scriptable Value/Value Objects/Player Facade List Value", fileName = "NewPlayerFacadeListValue")]
    [Provide]
    public partial class PlayerFacadeListValue : ScriptableListValue<PlayerFacade>
    {
    }
}