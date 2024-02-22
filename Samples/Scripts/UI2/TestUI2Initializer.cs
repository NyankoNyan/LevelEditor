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
            /* Шо нам нужно
            1. Требования к фасаду, чтобы делать клёвую вьюху
            - Фасад - это компонента, которая вешается на готовую вьюху
            - Интерфейс или родительский объект? Ваще-то похуй. Тогда интерфейс.
            2. Построение пользовательского интерфейса должно быть максимально простым.
            3. Все обработчики могут писаться как лямбды.
            - Обработчики для редактора, а не кода будут представлены компонентами.
            - Нужно цепное связвание вызывов. Так красивее.
            4. Вьюха может быть собрана из кода или из редактора. Или комбинированно.
            - Тогда мы приравниваем класс, как текстовое описание, к конфигу (SO), как к параметрическому представлению билдера.

            Рекомедации к проектированию.
            - Делать хоть что-то и будь что будет

            */

            /* Как сделать окно
             * UIRoot
             *     .Create(new WindowSpecificationClass())
             *     .AttachTo(targetCanvas);
             */

            UIProvider.Get().Attach(MyWindow.Create(), _canvas);
        }
    }

    public delegate void SetupThenDelegate(IElementSetup setup);

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

    public delegate object StateInitDelegate();

    public class StateDef
    {
        public string name;
        public object defaultValue;
        public StateInitDelegate stateInitCall;
    }

    public class StateVar
    {
        public string name;
        public object value;
    }

    #region Test Zone

    /* Нужно сделать окошко с вводом адреса сервера, которое хранит последнее значение и выдаёт значение по умолчанию при первом запуске.
     * Адрес сервера должен валидироваться по регэкспу. Если имя некорректное, это должно отображаться.
     * Также должно быть имя карты, которое можно поменять. Имя карты берётся из текущей карты или подставляется по умолчанию, если карта новая.
     * В имя карты нельзя ввести невалидный символ.
     * Инпуты в текстовые поступают через специальный объект инпутов. Поле имени карты сообщает список символов объекту инпута.
     * Закрытие окна вызывается специльным событием. В окне есть кнопка закрытия, которая это событие вызывает.
     * Есть кнопка подтверждения, которая вызывает процесс соединения с сервером, отображая подсказку.
     * В случае успеха, отправляет текущее окно в стек окон и вызывает заглушку.
     * В случае неудачи отображает сообщение с ошибкой.
     * Весь элемент не является цельным, а должен быть инициализирован из кусков.
     */

    public class MyWindow : BaseElement
    {
        private MyWindow()
        {
        }

        public static IElementSetup Create()
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

        public static IElementSetup Create()
        {
            return new ServerAddress()
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

        public static IElementSetup Create()
        {
            return new MapName()
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

        public static IElementSetup Create()
        {
            return new WaitStatus()
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

        public static IElementSetup Create()
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

        protected override BaseElement GetEmptyClone() => new ErrorStatus();
    }

    public class CancelButton : BaseElement
    {
        private CancelButton()
        {
        }

        public static IElementSetup Create()
        {
            return new CancelButton()
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

        public static IElementSetup Create()
        {
            return new ConfirmButton()
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