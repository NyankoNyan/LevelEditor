using System;
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
            /* ******************************
             * *    LEVEL_NAME              *
             * * STORAGE_MODE(LOCAL/REMOTE) *
             * *    storage_settings_frame  *
             * *  NEW  OPEN  SAVE  SAVE_AS  *
             * ****************************** */
            return new LevelStorageMenu().Write()
                .Id(nameof(LevelStorageMenu))
                .Style("window")
                .State("StorageMode", initFunc: () => {
                    string uri = level.LevelSettings.levelStoreURI;
                    Regex httpRe = new(@"^https?://");
                    return httpRe.IsMatch(uri) ? 1 : 0;
                })
                .State("LevelAPI", level)
                .State("Path", initFunc: () => level.LevelSettings.levelStoreURI)
                .State("Name", initFunc: () => level.LevelSettings.name)
                .Sub(
                    new InputElement().Write()
                        .Id("LevelName")
                        .Feature<MainText>(f =>
                            f.SetText(level.LevelSettings.name)),
                    new PanelElement().Write()
                        .Sub(
                            new LabelElement("Current location: ").Write(),
                            new LabelElement().Write()
                                .Id("CurrentLocation")
                        )
                        .GroupHorizontal(),
                    new OptionsButtonLine(
                        new ButtonElement(),
                        ("LOCAL", "Local Storage"),
                        ("REMOTE", "Remote storage")
                    ).Write(),
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
                        ("NEW", "New"),
                        ("OPEN", "Open..."),
                        ("SAVE", "Save"),
                        ("SAVE_AS", "Save As...")
                    ).Write(),
                    new QuestionWindow("CHOOSE_OPEN", "Save current changes?", "Yes", "No", "Cancel").Write()
                        .Id("ChooseOpenWindow")
                        .DefaultHide()
                )
                .GroupVertical()
                .StatesFrom("LocalFrame")
                .StatesFrom("RemoteFrame")
                .Handle("NEW", (sig, ctx) => {
                    // TODO lock self, open save yes/not/cancel window
                    // TODO open new default scene
                })
                .Handle("OPEN", (sig, ctx) => {
                    // TODO lock self, open save yes/not/cancel window
                    // TODO load and open existed level
                })
                .Handle("SAVE", (sig, ctx) => {
                    // TODO save current level
                })
                .Handle("SAVE_AS", (sig, ctx) => { })
                .Handle("LOCAL", (sig, ctx) => {
                    ctx.Element.State("StorageMode").Set<int>(0);
                    UpdateButtonsActivity(ctx);
                })
                .Handle("REMOTE", (sig, ctx) => {
                    ctx.Element.State("StorageMode").Set<int>(1);
                    UpdateButtonsActivity(ctx);
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
                .Init(UpdateButtonsActivity)
                .SignalBlock();
        }

        private static void UpdateButtonsActivity(
            IElementRuntimeContext ctx)
        {
            int state = ctx.Element.State("StorageMode").Get<int>();

            var locActive = ctx.Find("LOCAL").Feature<Active>();
            var remActive = ctx.Find("REMOTE").Feature<Active>();
            if (state == 0) {
                locActive.Deactivate();
                remActive.Activate();
                ctx.Find("LocalFrame").Show();
                ctx.Find("RemoteFrame").Hide();
            } else {
                locActive.Activate();
                remActive.Deactivate();
                ctx.Find("LocalFrame").Hide();
                ctx.Find("RemoteFrame").Show();
            }
        }

        protected override BaseElement GetEmptyClone() => new LevelStorageMenu();
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

    public class QuestionWindow : BaseElement
    {
        private readonly string _text;
        private readonly string[] _buttons;
        private readonly string _outSignal;

        public QuestionWindow(string outSignal, string text, params string[] buttons)
        {
            _outSignal = outSignal;
            _text = text;
            _buttons = buttons;

            var buttonPanel = new PanelElement().Write()
                .GroupHorizontal();

            // ButtonSetup[] buttonSetups = new ButtonSetup[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++) {
                var i1 = i + 1;
                buttonPanel.Sub(
                    new ButtonElement(null, _buttons[i]).Write()
                        .Handle(Facade.Click, (_, ctx) => {
                            ctx.DrillUpSignal("CHOOSE", i1);
                        })
                );
            }

            Write()
                .Sub(
                    new LabelElement(_text).Write()
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(100, 0)),
                    buttonPanel
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(bottom: 0, fixedSize: 100))
                )
                .SignalBlock()
                .Handle("CHOOSE", (sig, ctx) => {
                    ctx.Element.Hide();
                    ctx.DrillUpSignal(_outSignal, sig.Data);
                });
        }

        protected override BaseElement GetEmptyClone() => new QuestionWindow(_outSignal, _text, _buttons);
    }
}