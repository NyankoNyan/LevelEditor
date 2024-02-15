using System;
using System.Collections;

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

            UIProvider.Get().Attach(MyWindow.Create(), _canvas);
        }
    }

    public delegate void SetupThenDelegate(IElementSetupReadWrite setupReadWrite);

    public delegate void SetupHandleDelegate(ISignalContext signal, IElementRuntimeContext context);

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

        public static IElementSetupReadWrite Create()
        {
            return new MyWindow()
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
                    ctx.Element.Hide();
                    //waiting simulation
                    ctx.Start(new Operation()
                        .Do(() => {
                            ctx.DrillDownSignal("DEACTIVATE");
                            ctx.Sub("WaitStatus").Show();
                            ctx.DrillDownSignal("MSG", consumable: false);
                        })
                        .Wait(new WaitForSeconds(3))
                        .Do(() => {
                            ctx.Sub("WaitStatus").Hide();
                            ctx.DrillDownSignal("MSG", data: "Something happens...", consumable: false);
                            ctx.DrillDownSignal("ACTIVATE");
                        })
                    );
                });
        }
    }

    public class ServerAddress : BaseElement
    {
        private ServerAddress()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new ServerAddress()
                .SetId("ServerAddress")
                .SetStyle("field");
        }
    }

    public class MapName : BaseElement
    {
        private MapName()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new MapName()
                .SetId("MapName")
                .SetStyle("field");
        }
    }

    public class WaitStatus : BaseElement
    {
        private WaitStatus()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new WaitStatus()
                .SetId("WaitStatus")
                .SetStyle("wait");
        }
    }

    public class ErrorStatus : BaseElement
    {
        private ErrorStatus()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new ErrorStatus()
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
    }

    public class CancelButton : BaseElement
    {
        private CancelButton()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new CancelButton()
                .SetId("CancelButton")
                .SetStyle("button")
                .Handle(Facade.Click, (sig, ctx) =>
                    ctx.DrillUpSignal("QUIT")
                );
        }
    }

    public class ConfirmButton : BaseElement
    {
        private ConfirmButton()
        {
        }

        public static IElementSetupReadWrite Create()
        {
            return new ConfirmButton()
                .SetId("ConfirmButton")
                .SetStyle("button")
                .Handle(Facade.Click, (sig, ctx) =>
                    ctx.DrillUpSignal("CONFIRM")
                );
        }
    }

    #endregion Test Zone
}