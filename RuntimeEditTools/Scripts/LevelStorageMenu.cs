using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

using Level.API;
using Level.IO;

using LevelView;

using UI2;
using UI2.Feature;

using UnityEngine;
using UnityEngine.Assertions;

namespace RuntimeEditTools.UI
{
    public class LevelStorageMenuInitializer : MonoBehaviour
    {
        [SerializeField] private RectTransform _parent;

        void Start()
        {
            UIProvider.Get().Attach(
                LevelStorageMenu.Create(LevelStorage.Instance.API).Read(),
                _parent
            );
        }
    }

    public class LevelStorageMenu : BaseElement
    {
        private LevelStorageMenu()
        {
        }

        public static IElementSetupWrite Create(LevelAPI level)
        {
            Assert.IsNotNull(level);

            /* ****************************************************
             * *               LEVEL_NAME                         *
             * * STATE(new/changed/saved)  LOCATION(local/remote) *
             * * STORAGE_MODE_INFO(LOCAL/REMOTE)                  *
             * *             storage_mode_info                    *
             * *     CREATE      OPEN      SAVE       SAVE_AS     *
             * **************************************************** */
            return new LevelStorageMenu().Write()
                .Id("LevelMenu")
                .Style("window")
                .Init(ctx => {
                    ctx.Element.State("SaveStatus").Set(0);
                })
                .State("LevelAPI", level)
                .StateSync("Path", new VarProxy<string>(
                    () => level.LevelPathURL,
                    ref level.settingsChanged,
                    v => level.LevelPathURL = v
                ))
                .StateSync("Name", new VarProxy<string>(
                    () => level.Name,
                    ref level.settingsChanged,
                    v => level.Name = v
                ))
                .StateSync("StorageMode", new VarProxy<int>(
                    () => (int)level.LevelSettings.storageMode,
                    ref level.settingsChanged
                ))
                .StateSync("LevelDirty", new VarProxy<bool>(
                    () => level.Changed,
                    ref level.settingsChanged
                ))
                .Sub(
                    // Имя уровня
                    new InputElement().Write()
                        .StateFrom("LevelMenu", "Name").UseState("Name"),
                    // Основные состояния уровня
                    new Element("empty").Write()
                        .GroupHorizontal()
                        .Apply(Snaps.Center(height: 100),
                            Snaps.HorizontalSnap(0, 0))
                        .Sub(
                            new LabelElement().Write()
                                .StateFrom("LevelMenu", "LevelDirty")
                                .StateFrom("LevelMenu", "StorageMode")
                                // OnState вызывает обработчик при изменении и инициализации переменных 
                                .OnState(ctx => {
                                    string txt;
                                    if (ctx.Element.State("StorageMode").Get<int>() == (int)StorageMode.None) {
                                        txt = "New";
                                    } else {
                                        txt = ctx.Element.State("LevelDirty").Get<bool>() ? "Changed" : "Saved";
                                    }

                                    ctx.Element.Feature<MainText>().SetText($"STATUS: {txt}");
                                }, "LevelDirty", "StorageMode"),
                            new LabelElement().Write()
                                .StateFrom("LevelMenu", "StorageMode")
                                .UseState("StorageMenu", ctx => {
                                    StorageMode storageMode = (StorageMode)ctx.Element.State("LevelMenu").Get<int>();
                                    string txt = storageMode switch {
                                        StorageMode.Local => "Local",
                                        StorageMode.RemoteFS => "Remote",
                                        _ => "Unknown"
                                    };

                                    ctx.Element.Feature<MainText>().SetText($"STORAGE: {txt}");
                                })
                        ),
                    new LocalStorageSettings().Write()
                        .Id("LocalFrame")
                        .DefaultHide()
                        .Lazy(),
                    new RemoteStorageSettings().Write()
                        .Id("RemoteFrame")
                        .DefaultHide()
                        .Lazy(),
                    new OptionsButtonLine(
                        new ButtonElement(),
                        ("CREATE", "Create"),
                        ("OPEN", "Open..."),
                        ("SAVE", "Save"),
                        ("SAVE_AS", "Save As...")
                    ).Write(),
                    // Must be another window
                    new QuestionWindow("CHOOSE_OPEN", "Save current changes?", "Yes", "No", "Cancel").Write()
                        .Id("ChooseOpenWindow")
                        .DefaultHide()
                )
                .GroupVertical()
                .Handle("NEW", (sig, ctx) => {
                    // TODO lock self, open save yes/not/cancel window
                    // TODO open new default scene
                })
                .Handle("OPEN", (sig, ctx) => {
                    // TODO lock self, open save yes/not/cancel window
                    // TODO load and open existed level
                })
                .Handle("SAVE", (sig, ctx) => {
                    Save(level);
                })
                .Handle("SAVE_AS", (sig, ctx) => {
                    SaveAs();
                })
                .Handle("CHOOSE_OPEN", (sig, ctx) => {
                    if (sig.Data is not int i) {
                        return;
                    }

                    switch (i) {
                        case 1:
                            // TODO save scene
                            // TODO open scene
                            break;
                        case 2:
                            // TODO open scene
                            break;
                        default:
                            break;
                    }

                    ctx.Element.Show();
                })
                .SignalBlock();
        }

        private static void SaveAs()
        {
            //TODO something
        }

        private static void Save(LevelAPI level)
        {
            if (string.IsNullOrEmpty(level.LevelPathURL)) {
                SaveAs();
            } else {
                level.SaveLevel();
            }
        }

        private static void CheckChangesAskSave()
        {
            //TODO something
        }

        protected override BaseElement GetEmptyClone() => new LevelStorageMenu();
    }

    public class FileBrowser : BaseElement
    {
        private readonly BrowserType _browserType;

        public enum BrowserType { Open, Save }

        public IElementSetupWrite Create(BrowserType browserType)
        {
            return new FileBrowser(browserType).Write()
                .Id("FileBrowser")
                .State("MainPath", initFunc: () => {
                    // TODO Init from level
                    return "";
                })
                .Sub(
                    // URL или файловый путь до хранилища уровней
                    new Element("named-input").Write()
                        .StateFrom("FileBrowser", "MainPath")
                        .UseState("MainPath")
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(top: 0, fixedSize: 100)),
                    // Кнопки
                    new Element("empty").Write()
                        .GroupHorizontal()
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(bottom: 0, fixedSize: 100))
                        .Sub(
                            new ButtonElement("CANCEL", "Cancel").Write(),
                            new ButtonElement("CONFIRM").Write()
                                .Init(ctx => {
                                    string txt = browserType switch {
                                        BrowserType.Open => "Open",
                                        BrowserType.Save => "Save",
                                        _ => throw new ArgumentOutOfRangeException(nameof(browserType), browserType,
                                            null)
                                    };
                                    ctx.Element.Feature<MainText>().SetText(txt);
                                })
                        ),
                    // Список уровней
                    new Element("vertical-scroll").Write()
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(100, 100))
                        .DefaultHide(),
                    // Иконка загрузки
                    new Element("loading").Write()
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(100, 100))
                        .DefaultHide()
                );
        }

        private FileBrowser(BrowserType browserType)
        {
            _browserType = browserType;
        }

        protected override BaseElement GetEmptyClone() => new FileBrowser(_browserType);
    }

    public class LocalStorageSettings : BaseElement
    {
        public LocalStorageSettings()
        {
            Write()
                .Style("sub-settings")
                .Sub(
                    new InputElement().Write()
                        .UseState("FilePath")
                        .Feature<MainText>(f => f.SetText("Level folder"))
                );
        }

        protected override BaseElement GetEmptyClone() => new LocalStorageSettings();
    }

    public class RemoteStorageSettings : BaseElement
    {
        public RemoteStorageSettings()
        {
            Write()
                .Style("sub-settings")
                .Sub(
                    new InputElement().Write()
                        .UseState("RemoteAddress")
                        .Feature<MainText>(f => f.SetText("URL")),
                    new InputElement().Write()
                        .UseState("Port")
                        .Feature<UI2.Feature.Input>(f => f.Number(4))
                        .Feature<MainText>(f => f.SetText("Port")),
                    new FlagElement().Write()
                        .UseState("UseHTTPS")
                        .Feature<MainText>(f => f.SetText("HTTPS"))
                )
                .GroupHorizontal();
        }

        protected override BaseElement GetEmptyClone() => new RemoteStorageSettings();
    }

    public struct ButtonSetup
    {
        public string id;
        public string name;

        public ButtonSetup(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class OptionsButtonLine : BaseElement
    {
        private readonly IElementSetupRead _proto;
        private ButtonSetup[] _buttons;


        public OptionsButtonLine(IElementSetupRead proto, params (string, string)[] buttons) : this
            (proto, buttons.Select(x => new ButtonSetup(x.Item1, x.Item2)).ToArray())
        {
        }

        public OptionsButtonLine(IElementSetupRead proto, params ButtonSetup[] buttons)
        {
            _proto = proto;
            _buttons = new ButtonSetup[buttons.Length];
            buttons.CopyTo(_buttons, 0);

            SetStyle("button-line");
            GroupHorizontal();
            foreach (var button in buttons) {
                Sub(
                    proto.Write()
                        .Clone()
                        .Id(button.id)
                        .Feature<MainText>(f => f.SetText(button.name))
                        .Handle(Facade.Click, (sig, ctx) => {
                                ctx.DrillUpSignal(button.id);
                                sig.Resume();
                            }
                        )
                );
            }
        }

        protected override BaseElement GetEmptyClone() => new OptionsButtonLine(_proto, _buttons);
    }
}