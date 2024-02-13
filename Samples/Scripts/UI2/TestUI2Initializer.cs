using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;

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
                .Sub(new List<IElementSetupReadWrite> {
                    ServerAddress.Create()
                        .Then(Snaps.HorizontalSnap(partSize: .8f))
                        .Then(Snaps.VerticalSnap(top: 0f, fixedSize: 100f)),
                    MapName.Create()
                        .Then(Snaps.HorizontalSnap(partSize: .8f))
                        .Then(Snaps.VerticalSnap(top: 150f, fixedSize: 100f)),
                    WaitStatus.Create()
                        .Then(Snaps.HorizontalSnap(fixedSize: 200f))
                        .Then(Snaps.VerticalSnap(fixedSize: 500f)),
                    ErrorStatus.Create()
                        .Then(Snaps.HorizontalSnap(partSize: 1))
                        .Then(Snaps.VerticalSnap(bottom: 0, fixedSize: 100f)),
                    ConfirmButton.Create()
                        .Then(Snaps.HorizontalSnap(partSize: .3f))
                        .Then(Snaps.VerticalSnap(top: 300f, fixedSize: 100f))
                        .MoveRelative(new Vector2(-.25f, 0)),
                    CancelButton.Create()
                        .Then(Snaps.HorizontalSnap(partSize: .3f))
                        .Then(Snaps.VerticalSnap(top: 300f, fixedSize: 100f))
                        .MoveRelative(new Vector2(.25f, 0))
                })
                .Handle((sig, ctx) => {
                        switch (sig.Name) {
                            case "QUIT": {
                                ctx.Element.Hide();
                                ctx.DrillUpSignal("RETURN_CONTROL");
                                break;
                            }
                            case "CONFIRM": {
                                ctx.Element.Hide();
                                //waiting simulation
                                break;
                            }
                            default:
                                return;
                        }
                        sig.Consume();
                    }
                );
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
        private MapName() { }

        public static IElementSetupReadWrite Create()
        {
            return new MapName()
                .SetId("MapName")
                .SetStyle("field");
        }
    }

    public class WaitStatus : BaseElement
    {
        private WaitStatus() { }

        public static IElementSetupReadWrite Create()
        {
            return new WaitStatus()
                .SetId("WaitStatus")
                .SetStyle("wait");
        }
    }

    public class ErrorStatus : BaseElement
    {
        private ErrorStatus() { }

        public static IElementSetupReadWrite Create()
        {
            return new ErrorStatus()
                .SetId("ErrorStatus")
                .SetStyle("status-bar");
        }
    }

    public class CancelButton : BaseElement
    {
        private CancelButton() { }

        public static IElementSetupReadWrite Create()
        {
            return new CancelButton()
                .SetId("CancelButton")
                .SetStyle("button");
        }
    }

    public class ConfirmButton : BaseElement
    {
        private ConfirmButton() { }

        public static IElementSetupReadWrite Create()
        {
            return new ConfirmButton()
                .SetId("ConfirmButton")
                .SetStyle("button");
        }
    }

    #endregion Test Zone
}