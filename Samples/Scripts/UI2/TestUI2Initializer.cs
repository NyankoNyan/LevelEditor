using System;
using System.Collections.Generic;

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

    public interface IElementInstance
    {
        IElementInstance Show();

        IElementInstance Hide();

        IElementSetup Proto { get; }
    }

    public delegate void SetupThenDelegate(IElementSetup setup);

    public interface IElementSetup
    {
        IElementSetup SetId(string id);

        IElementSetup SetStyle(string style);

        string Style { get; }

        IElementSetup Sub(IElementSetup element);

        IElementSetup Sub(IEnumerable<IElementSetup> elements);

        IEnumerable<IElementSetup> Subs { get; }

        IElementSetup Then(SetupThenDelegate fn);

        IElementSetup SetPivot(Vector2 pivot);

        Vector2 Pivot { get; }
        bool NewPivot { get; }

        IElementSetup SetAnchor(Vector2 min, Vector2 max);

        (Vector2, Vector2) Anchor { get; }
        bool NewAnchor { get; }

        IElementSetup SetSizeDelta(Vector2 delta);

        Vector2 SizeDelta { get; }
        bool NewSizeDelta { get; }

        IElementSetup SetAnchoredPosition(Vector2 pos);

        Vector2 AnchoredPosition { get; }
        bool NewAnchoredPosition { get; }
    }

    public class ElementWorkflowException : Exception
    {
    }

    public abstract class BaseElement : IElementSetup
    {
        private string _id;
        private string _style;
        private List<IElementSetup> _children;
        private Vector2 _pivot;
        private bool _newPivot;
        private Vector2 _anchorMin;
        private Vector2 _anchorMax;
        private bool _newAnchor;
        private Vector2 _sizeDelta;
        private bool _newSizeDelta;
        private Vector2 _anchoredPosition;
        private bool _newAnchorePos;

        public string Style => _style;

        public IEnumerable<IElementSetup> Subs => _children.AsReadOnly();

        public Vector2 Pivot => _pivot;

        public (Vector2, Vector2) Anchor => (_anchorMin, _anchorMax);

        public Vector2 SizeDelta => _sizeDelta;

        public Vector2 AnchoredPosition => _anchoredPosition;

        public bool NewPivot => _newPivot;

        public bool NewAnchor => _newAnchor;

        public bool NewSizeDelta => _newSizeDelta;

        public bool NewAnchoredPosition => _newAnchorePos;

        public virtual void Init()
        {
        }

        public IElementSetup SetStyle(string style)
        {
            _style = style;
            return this;
        }

        public IElementSetup SetId(string id)
        {
            _id = id;
            return this;
        }

        public IElementSetup Sub(IElementSetup element)
        {
            if (_children == null) {
                _children = new();
            }
            _children.Add(element);
            return this;
        }

        public IElementSetup Sub(IEnumerable<IElementSetup> elements)
        {
            foreach (var element in elements) {
                Sub(element);
            }
            return this;
        }

        public IElementSetup Then(SetupThenDelegate fn)
        {
            fn(this);
            return this;
        }

        public IElementSetup SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
            _newPivot = true;
            return this;
        }

        public IElementSetup SetAnchor(Vector2 min, Vector2 max)
        {
            _anchorMin = min;
            _anchorMax = max;
            _newAnchor = true;
            return this;
        }

        public IElementSetup SetSizeDelta(Vector2 delta)
        {
            _sizeDelta = delta;
            _newSizeDelta = true;
            return this;
        }

        public IElementSetup SetAnchoredPosition(Vector2 pos)
        {
            _anchoredPosition = pos;
            _newAnchorePos = true;
            return this;
        }
    }

    [Serializable]
    public struct Style
    {
        public string name;
        public GameObject prefab;
    }

    internal class ElementInstance : IElementInstance
    {
        private readonly IElementSetup _proto;
        private readonly GameObject _instance;

        public ElementInstance(IElementSetup proto, GameObject instance)
        {
            Assert.IsNotNull(proto);
            Assert.IsNotNull(instance);
            _proto = proto;
            _instance = instance;
        }

        public IElementSetup Proto => _proto;

        public IElementInstance Hide()
        {
            _instance.SetActive(false);
            return this;
        }

        public IElementInstance Show()
        {
            _instance.SetActive(true);
            return this;
        }
    }

    public class UIRoot
    {
        private readonly Dictionary<string, Style> _styles = new();
        private readonly HashSet<IElementInstance> _instances = new();

        public bool Reg(Style style)
        {
            return _styles.TryAdd(style.name, style);
        }

        public IEnumerable<Style> Reg(IEnumerable<Style> styles)
        {
            foreach (var style in styles) {
                if (!Reg(style)) {
                    yield return style;
                }
            }
        }

        public IElementInstance Attach(IElementSetup setup, Transform parent)
        {
            if (!parent) {
                throw new ArgumentException("Empty parent");
            }

            if (_styles.TryGetValue(setup.Style, out Style style)) {
                var newGO = GameObject.Instantiate(style.prefab, parent);
                var instance = new ElementInstance(setup, newGO);

                var facade = newGO.GetComponent<ElementInstanceFacade>();
                if (facade) {
                    foreach (var sub in setup.Subs) {
                        Attach(sub, facade.SubZone);
                    }
                }

                var rectTransform = newGO.GetComponent<RectTransform>();
                if (rectTransform) {
                    if (setup.NewAnchor) {
                        (rectTransform.anchorMin, rectTransform.anchorMax) = setup.Anchor;
                    }
                    if (setup.NewAnchoredPosition) {
                        rectTransform.anchoredPosition = setup.AnchoredPosition;
                    }
                    if (setup.NewPivot) {
                        rectTransform.pivot = setup.Pivot;
                    }
                    if (setup.NewSizeDelta) {
                        rectTransform.sizeDelta = setup.SizeDelta;
                    }
                }

                _instances.Add(instance);
                return instance;
            } else {
                throw new ElementWorkflowException();
            }
        }
    }

    public static class Snaps
    {
        //public static SetupThenDelegate 
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
        { }

        public static IElementSetup Create()
        {
            return new MyWindow()
                .SetId("MyWindow")
                .SetStyle("window")
                .Sub(new List<IElementSetup>{
                    ServerAddres.Create()
                });
        }
    }

    public class ServerAddres : BaseElement
    {
        private ServerAddres()
        { }

        public static IElementSetup Create()
        {
            return new ServerAddres()
                .SetId("ServerAddres")
                .SetStyle("field");
        }
    }

    public class MapName : BaseElement
    {
        public override void Init()
        {
            SetId("MapName")
                .SetStyle("field");
        }
    }

    public class WaitStatus : BaseElement
    {
        public override void Init()
        {
            SetId("WaitStatus")
                .SetStyle("wait");
        }
    }

    public class ErrorStatus : BaseElement
    {
        public override void Init()
        {
            SetId("ErrorStatus")
                .SetStyle("status-bar");
        }
    }

    public class CancelButton : BaseElement
    {
        public override void Init()
        {
            SetId("CancelButton")
                .SetStyle("button");
        }
    }

    public class ConfirmButton : BaseElement
    {
        public override void Init()
        {
            SetId("ConfirmButton")
                .SetStyle("button");
        }
    }

    #endregion Test Zone
}