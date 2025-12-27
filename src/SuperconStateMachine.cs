using Godot;
using Raele.GodotUtils.Extensions;

namespace Raele.Supercon2D;

public partial class SuperconStateMachine : Raele.GodotUtils.StateMachine.StateMachine<SuperconState>
{
	public Node? DebugPrintContext = null;

	public SuperconStateMachine()
	{
		this.TransitionCompleted += transition =>
		{
			if (this.DebugPrintContext == null)
				return;
			this.DebugPrintContext?.DebugLog(
				$"🔀 State changed: {transition.ExitState?.Name.ToString().BBCBold()} → {transition.EnterState?.Name.ToString().BBCBold()}",
				transition
			);
		};
	}
}
