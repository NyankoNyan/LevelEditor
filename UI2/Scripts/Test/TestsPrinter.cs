using System.Reflection;

using global::System;

using UI2.Feature;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2.Test
{
    /// <summary>
    /// Атрибут для тестов. Укажите атрибут у тестового класса и у его методов,
    /// которые нужно запускать.
    /// </summary>
    public class UITestAttribute : Attribute
    {
    }

    /// <summary>
    /// Компонент печатает результаты всех тестов в виде плиточек.
    /// Тесты собираются через рефлексию.
    /// </summary>
    public class TestsPrinter : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        void Awake()
        {
            Assert.IsNotNull(_target);
        }

        void Start()
        {
            var setup = new PanelElement().Write()
                .Grid(new Vector2(200, 200), new Vector2(10, 10));

            foreach (var type in this.GetType().Assembly.GetTypes()) {
                var clAttr = type.GetCustomAttribute(typeof(UITestAttribute));
                if (clAttr == null) {
                    continue;
                }

                foreach (var mType in type.GetMethods()) {
                    var mAttr = mType.GetCustomAttribute(typeof(UITestAttribute));
                    if (mAttr == null || !mType.IsStatic) {
                        continue;
                    }

                    var elemSetup = mType.Invoke(null, null) as IElementSetupWrite;
                    if (elemSetup == null) {
                        Debug.LogError($"Can't create element setup in {type.Name}.{mType.Name}");
                    } else {
                        setup.Sub(
                            new PanelElement().Write()
                                .Style("named-panel")
                                .Feature<MainText>(f => f.SetText(mType.Name))
                                .Sub(elemSetup)
                        );
                    }
                }
            }

            UIProvider.Get().Attach(setup.Read(), _target);
        }
    }
}