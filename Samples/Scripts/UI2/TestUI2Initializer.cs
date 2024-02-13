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