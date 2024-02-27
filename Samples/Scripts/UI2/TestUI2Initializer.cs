using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2
{
    public class TestUI2Initializer : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvas;

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            /* �� ��� �����
            1. ���������� � ������, ����� ������ ����� �����
            - ����� - ��� ����������, ������� �������� �� ������� �����
            - ��������� ��� ������������ ������? ����-�� �����. ����� ���������.
            2. ���������� ����������������� ���������� ������ ���� ����������� �������.
            3. ��� ����������� ����� �������� ��� ������.
            - ����������� ��� ���������, � �� ���� ����� ������������ ������������.
            - ����� ������ ��������� �������. ��� ��������.
            4. ����� ����� ���� ������� �� ���� ��� �� ���������. ��� ��������������.
            - ����� �� ������������ �����, ��� ��������� ��������, � ������� (SO), ��� � ���������������� ������������� �������.

            ����������� � ��������������.
            - ������ ���� ���-�� � ���� ��� �����

            */

            /* ��� ������� ����
             * UIRoot
             *     .Create(new WindowSpecificationClass())
             *     .AttachTo(targetCanvas);
             */

            UIProvider.Get().Attach(MyWindow.Create().Read(), _canvas);
        }
    }

    public delegate void SetupThenDelegate(IElementSetupWrite setup);

    public delegate void SetupHandleDelegate(ISignalContext signal, IElementRuntimeContext context);

    public delegate void SimpleHandleDelegate(IElementRuntimeContext context);

    public delegate IEnumerator OperationDelegate();

    public class ElementWorkflowException : Exception
    {
    }

    [Serializable]
    public struct Style
    {
        public string name;
        public GameObject prefab;
    }

    public static class Facade
    {
        public const string Click = "CLICK";
    }

    public delegate object StateInitDelegate();

    public class StateDef
    {
        public string name;
        public object defaultValue;
        public StateInitDelegate stateInitCall;
    }

    public class StateProxyDef
    {
        public string name;
        public string refVarName;
        public string refId;
    }

    public interface IStateVar
    {
        void Set<T>(T v);
        T As<T>();
        Action onChanged { get; set; }
    }

    public class StateVar : IStateVar, IDisposable
    {
        public string name;
        public object value;
        public bool isProxy;

        public Action onChanged { get; set; }

        public StateVar(string name, object value = null)
        {
            this.name = name;
            this.value = value;
        }

        public StateVar(StateDef stateDef)
        {
            name = stateDef.name;
            if (stateDef.defaultValue != null) {
                value = stateDef.defaultValue switch {
                    bool b => b,
                    int i => i,
                    ICloneable c => c.Clone(),
                    _ => stateDef.defaultValue
                };
            } else if (stateDef.stateInitCall != null) {
                value = stateDef.stateInitCall();
            }
        }

        /// <summary>
        /// Create reference to state
        /// </summary>
        /// <param name="refVar"></param>
        /// <param name="newName"></param>
        public StateVar(StateVar refVar, string newName = null)
        {
            Assert.IsNotNull(refVar);
            name = newName ?? refVar.name;
            isProxy = true;
            value = refVar;
            refVar.onChanged += OnProxyChanged;
        }

        private void OnProxyChanged() => onChanged?.Invoke();

        public void Set<T>(T v)
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    sv.Set(v);
                } else {
                    throw new ElementWorkflowException();
                }
            } // isProxy 
            else {
                bool callOnChanged;
                if (v is IEquatable<T> newEq && value is IEquatable<T> oldEq) {
                    callOnChanged = !newEq.Equals(oldEq);
                } else {
                    callOnChanged = true;
                }

                if (typeof(T) == typeof(bool)
                    || typeof(T) == typeof(int)) {
                    value = v;
                } else if (v is ICloneable c) {
                    value = c.Clone();
                } else {
                    value = v;
                }

                if (callOnChanged) {
                    onChanged?.Invoke();
                }
            } // !isProxy
        }

        public T As<T>()
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    return sv.As<T>();
                } else {
                    throw new ElementWorkflowException();
                }
            } // isProxy 
            else {
                if (value is T t) {
                    return t;
                } else {
                    return default;
                }
            } // !isProxy
        }

        public void Dispose()
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    sv.onChanged -= OnProxyChanged;
                }
            }
        }
    }

    #region Test Zone

    /* ����� ������� ������ � ������ ������ �������, ������� ������ ��������� �������� � ����� �������� �� ��������� ��� ������ �������.
     * ����� ������� ������ �������������� �� ��������. ���� ��� ������������, ��� ������ ������������.
     * ����� ������ ���� ��� �����, ������� ����� ��������. ��� ����� ������ �� ������� ����� ��� ������������� �� ���������, ���� ����� �����.
     * � ��� ����� ������ ������ ���������� ������.
     * ������ � ��������� ��������� ����� ����������� ������ �������. ���� ����� ����� �������� ������ �������� ������� ������.
     * �������� ���� ���������� ���������� ��������. � ���� ���� ������ ��������, ������� ��� ������� ��������.
     * ���� ������ �������������, ������� �������� ������� ���������� � ��������, ��������� ���������.
     * � ������ ������, ���������� ������� ���� � ���� ���� � �������� ��������.
     * � ������ ������� ���������� ��������� � �������.
     * ���� ������� �� �������� �������, � ������ ���� ��������������� �� ������.
     */

    public class MyWindow : BaseElement
    {
        private MyWindow()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new MyWindow()
                .Write()
                .SetId("MyWindow")
                .SetStyle("window")
                .Sub(ServerAddress.Create()
                        .Apply(
                            Snaps.HorizontalSnap(partSize: .8f),
                            Snaps.VerticalSnap(top: 0f, fixedSize: 100f),
                            Traits.Active),
                    MapName.Create()
                        .Apply(
                            Snaps.HorizontalSnap(partSize: .8f),
                            Snaps.VerticalSnap(top: 150f, fixedSize: 100f),
                            Traits.Active),
                    WaitStatus.Create()
                        .Apply(
                            Snaps.HorizontalSnap(fixedSize: 500f),
                            Snaps.VerticalSnap(fixedSize: 500f))
                        .DefaultHide(),
                    ErrorStatus.Create()
                        .Apply(
                            Snaps.HorizontalSnap(partSize: 1),
                            Snaps.VerticalSnap(bottom: 0, fixedSize: 100f)),
                    ConfirmButton.Create()
                        .Apply(
                            Snaps.HorizontalSnap(partSize: .3f),
                            Snaps.VerticalSnap(top: 300f, fixedSize: 100f),
                            Traits.Active)
                        .MoveRelative(new Vector2(-.25f, 0)),
                    CancelButton.Create()
                        .Apply(
                            Snaps.HorizontalSnap(partSize: .3f),
                            Snaps.VerticalSnap(top: 300f, fixedSize: 100f),
                            Traits.Active)
                        .MoveRelative(new Vector2(.25f, 0))
                )
                .Handle("QUIT", (sig, ctx) => {
                    ctx.Element.Hide();
                    ctx.DrillUpSignal("RETURN_CONTROL");
                })
                .Handle("CONFIRM", (sig, ctx) => {
                    //waiting simulation
                    ctx.Start(new Operation()
                        .Do(() => {
                            ctx.DrillDownSignal("DEACTIVATE");
                            ctx.Sub("WaitStatus")?.Show();
                            ctx.DrillDownSignal("MSG", consumable: false);
                        })
                        .Wait(new WaitForSeconds(3))
                        .Do(() => {
                            ctx.Sub("WaitStatus")?.Hide();
                            ctx.DrillDownSignal("MSG", data: "Something happens...", consumable: false);
                            ctx.DrillDownSignal("ACTIVATE");
                        })
                    );
                });
        }

        protected override BaseElement GetEmptyClone() => new MyWindow();
    }

    public class ServerAddress : BaseElement
    {
        private ServerAddress()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new ServerAddress()
                .Write()
                .SetId("ServerAddress")
                .SetStyle("field");
        }

        protected override BaseElement GetEmptyClone() => new ServerAddress();
    }

    public class MapName : BaseElement
    {
        private MapName()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new MapName()
                .Write()
                .SetId("MapName")
                .SetStyle("field");
        }

        protected override BaseElement GetEmptyClone() => new MapName();
    }

    public class WaitStatus : BaseElement
    {
        private WaitStatus()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new WaitStatus()
                .Write()
                .SetId("WaitStatus")
                .SetStyle("wait");
        }

        protected override BaseElement GetEmptyClone() => new WaitStatus();
    }

    public class ErrorStatus : BaseElement
    {
        private ErrorStatus()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new ErrorStatus()
                .Write()
                .SetId("ErrorStatus")
                .SetStyle("status-bar")
                .Handle("MSG", (sig, ctx) => {
                    if (sig.Data is string s) {
                        ctx.Element.GetFacadeFeature<MainTextFeature>()?.SetText(s);
                    } else {
                        ctx.Element.GetFacadeFeature<MainTextFeature>()?.SetText("");
                    }
                });
        }

        protected override BaseElement GetEmptyClone() => new ErrorStatus();
    }

    public class CancelButton : BaseElement
    {
        private CancelButton()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new CancelButton()
                .Write()
                .SetId("CancelButton")
                .SetStyle("button")
                .Handle(Facade.Click, (sig, ctx) =>
                    ctx.DrillUpSignal("QUIT")
                );
        }

        protected override BaseElement GetEmptyClone() => new CancelButton();
    }

    public class ConfirmButton : BaseElement
    {
        private ConfirmButton()
        {
        }

        public static IElementSetupWrite Create()
        {
            return new ConfirmButton()
                .Write()
                .SetId("ConfirmButton")
                .SetStyle("button")
                .Handle(Facade.Click, (sig, ctx) =>
                    ctx.DrillUpSignal("CONFIRM")
                );
        }

        protected override BaseElement GetEmptyClone() => new ConfirmButton();
    }

    #endregion Test Zone
}