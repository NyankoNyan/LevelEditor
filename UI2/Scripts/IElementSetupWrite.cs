using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public interface IElementSetupWrite
    {
        IElementSetupRead Read();

        IElementSetupWrite Clone();

        IElementSetupWrite Id(string id);

        IElementSetupWrite Style(string style);

        IElementSetupWrite Sub(params IElementSetupWrite[] elements);

        IElementSetupWrite Sub(IEnumerable<IElementSetupWrite> elements);

        IElementSetupWrite Apply(params SetupDelegate[] fns);

        IElementSetupWrite Pivot(Vector2 pivot);

        IElementSetupWrite Pivot(float x, float y) => Pivot(new Vector2(x, y));

        IElementSetupWrite Anchor(Vector2 min, Vector2 max);

        IElementSetupWrite Anchor((float, float) min, (float, float) max) =>
            Anchor(new Vector2(min.Item1, min.Item2), new Vector2(max.Item1, max.Item2));

        IElementSetupWrite SizeDelta(Vector2 delta);

        IElementSetupWrite SizeDelta(float x, float y) => SizeDelta(new Vector2(x, y));

        IElementSetupWrite AnchoredPos(Vector2 pos);

        IElementSetupWrite AnchoredPos(float x, float y) => AnchoredPos(new Vector2(x, y));

        IElementSetupWrite MoveRelative(Vector2 move);

        IElementSetupWrite MoveRelative(float x, float y) => MoveRelative(new Vector2(x, y));

        IElementSetupWrite Move(Vector2 move);

        IElementSetupWrite Move(float x, float y) => Move(new Vector2(x, y));

        IElementSetupWrite Handle(string signalName, SetupHandleDelegate handler);

        IElementSetupWrite DefaultHide();

        IElementSetupWrite Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature;

        IElementSetupWrite GroupVertical();

        IElementSetupWrite GroupHorizontal();

        IElementSetupWrite SignalBlock(bool block = true);

        IElementSetupWrite Lazy(bool lazy = true);

        IElementSetupWrite State(string name, object value = null, StateInitDelegate initFunc = null);

        IElementSetupWrite Init(SimpleHandleDelegate handler)
            => Handle(UIRoot.INIT_SIGNAL, (_, ctx) => {
                // Simple wrapper
                handler(ctx);
            });

        IElementSetupWrite Timer(float timer, SimpleHandleDelegate handler) =>
            Init(ctx =>
                ctx.Start(new Operation()
                    .Do(() => handler.Invoke(ctx))
                    .Wait((timer == 0) ? null : new WaitForSeconds(timer))
                    .CallSelf()
                )
            );

        IElementSetupWrite UseState(string varName, SimpleHandleDelegate updateCall = null);

        IElementSetupWrite StatesFrom(string elemId);

        IElementSetupWrite StateFrom(string elemId, string elemState, string newId = null);

        IElementSetupWrite Grid(Vector2 cellSize, RectOffset padding = default);

        IElementSetupWrite Grid((float, float) cellSize, (int, int, int, int) padding = default)
            => Grid(new Vector2(cellSize.Item1, cellSize.Item2),
                new RectOffset(padding.Item1, padding.Item2, padding.Item3, padding.Item4));
    }
}