using System;
using System.Collections;

using UI2.Feature;

using UnityEngine;

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

    public delegate void SetupDelegate(IElementSetupWrite setup);

    public delegate void SetupHandleDelegate(ISignalContext signal, IElementRuntimeContext context);

    public delegate void SimpleHandleDelegate(IElementRuntimeContext context);

    public delegate IEnumerator OperationDelegate();

    public class ElementWorkflowException : Exception
    {
        public ElementWorkflowException(string msg) : base(msg) { }
    }

    [Serializable]
    public struct Style
    {
        public string name;
        public GameObject prefab;
    }

    public static class Facade
    {
        public const string Click = "F_CLICK";
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
                .Id("MyWindow")
                .Style("window")
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
                            ctx.Find("WaitStatus")?.Show();
                            ctx.DrillDownSignal("MSG", consumable: false);
                        })
                        .Wait(new WaitForSeconds(3))
                        .Do(() => {
                            ctx.Find("WaitStatus")?.Hide();
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
                .Id("ServerAddress")
                .Style("field");
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
                .Id("MapName")
                .Style("field");
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
                .Id("WaitStatus")
                .Style("wait");
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
                .Id("ErrorStatus")
                .Style("status-bar")
                .Handle("MSG", (sig, ctx) => {
                    if (sig.Data is string s) {
                        ctx.Element.Feature<MainText>()?.SetText(s);
                    } else {
                        ctx.Element.Feature<MainText>()?.SetText("");
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
                .Id("CancelButton")
                .Style("button")
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
                .Id("ConfirmButton")
                .Style("button")
                .Handle(Facade.Click, (sig, ctx) =>
                    ctx.DrillUpSignal("CONFIRM")
                );
        }

        protected override BaseElement GetEmptyClone() => new ConfirmButton();
    }

    #endregion Test Zone
}