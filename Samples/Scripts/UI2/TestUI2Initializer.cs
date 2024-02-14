using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.CoreModule;

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

    public static class Traits
    {
        public static readonly SetupThenDelegate Active = elem => {
            elem
                .Handle("ACTIVATE", (sig, ctx) => {
                    //TODO unlock element
                })
                .Handle("DEACTIVATE", (sig, ctx) => {
                    //TODO lock element
                });
        };
    }

    public interface IOperation
    {
        IOperation Do(Action callback);
        IOperation Wait(YieldInstruction wait);

        IEnumerator Exec();
    }

    public class Operation : IOperation
    {
        private readonly List<Instruction> _instructions = new();

        public IOperation Do(Action callback)
        {
            Assert.IsNotNull(callback);
            _instructions.Add(new Instruction() {
                iType = InstructionType.Do,
                callback = callback
            });
            return this;
        }

        public IOperation Wait(YieldInstruction wait)
        {
            _instructions.Add(new Instruction() {
                iType = InstructionType.Wait,
                wait = wait
            });
            return this;
        }

        public IEnumerator Exec()
        {
            foreach (var instruction in _instructions) {
                switch (instruction.iType) {
                    case InstructionType.Do:
                        instruction.callback();
                        break;
                    case InstructionType.Wait:
                        yield return instruction.wait;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private enum InstructionType { Do, Wait }

        private struct Instruction
        {
            public InstructionType iType;
            public Action callback;
            public YieldInstruction wait;
        }
    }

    public class OperationDescriptor
    {
        private readonly Coroutine _coroutine;

        internal OperationDescriptor(Coroutine coroutine)
        {
            _coroutine = coroutine;
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
                    //TODO change element text
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