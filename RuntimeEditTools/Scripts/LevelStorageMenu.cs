using System.Drawing;
using System.Text.RegularExpressions;

using Level.API;
using Level.IO;

using LevelView;

using UI2;

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
                .SetId(nameof(LevelStorageMenu))
                .SetStyle("window")
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
                        .SetId("LevelName")
                        .Feature<MainTextFeature>(f =>
                            f.SetText(level.LevelSettings.name)),
                    new OptionsButtonLine(
                        new ButtonElement(),
                        ("LOCAL", "Local Storage"),
                        ("REMOTE", "Remote storage")
                    ).Write(),
                    new LocalStorageSettings().Write()
                        .SetId("LocalFrame")
                        .DefaultHide()
                        .Lazy(),
                    new RemoteStorageSettings().Write()
                        .SetId("RemoteFrame")
                        .DefaultHide()
                        .Lazy(),
                    new OptionsButtonLine(
                        new ButtonElement(),
                        ("NEW", "New"),
                        ("OPEN", "Open..."),
                        ("SAVE", "Save"),
                        ("SAVE_AS", "Save As...")
                    ).Write()
                )
                .GroupVertical()
                .Handle("NEW", (sig, ctx) => { })
                .Handle("OPEN", (sig, ctx) => { })
                .Handle("SAVE", (sig, ctx) => { })
                .Handle("SAVE_AS", (sig, ctx) => { })
                .Handle("LOCAL", (sig, ctx) => {
                    ctx.Element.State("StorageMode").Set<int>(0);
                    UpdateButtonsActivity(ctx);
                })
                .Handle("REMOTE", (sig, ctx) => {
                    ctx.Element.State("StorageMode").Set<int>(1);
                    UpdateButtonsActivity(ctx);
                })
                .Init(UpdateButtonsActivity)
                .SignalBlock();
        }

        private static void UpdateButtonsActivity(
            IElementRuntimeContext ctx)
        {
            int state = ctx.Element.State("StorageMode").As<int>();

            var locActive = ctx.Sub("LOCAL").GetFacadeFeature<ActivateFeature>();
            var remActive = ctx.Sub("REMOTE").GetFacadeFeature<ActivateFeature>();
            if (state == 0) {
                locActive.Deactivate();
                remActive.Activate();
                ctx.Sub("LocalFrame").Show();
                ctx.Sub("RemoteFrame").Hide();
            } else {
                locActive.Activate();
                remActive.Deactivate();
                ctx.Sub("LocalFrame").Hide();
                ctx.Sub("RemoteFrame").Show();
            }
        }

        protected override BaseElement GetEmptyClone() => new LevelStorageMenu();
    }

    public class LocalStorageSettings : BaseElement
    {
        public LocalStorageSettings()
        {
            Write()
                .SetStyle("sub-settings")
                .Sub(
                    new InputElement().Write()
                        .UseState("FilePath")
                        .Feature<MainTextFeature>(f => f.SetText("Level folder"))
                );
        }

        protected override BaseElement GetEmptyClone() => new LocalStorageSettings();
    }

    public class RemoteStorageSettings : BaseElement
    {
        public RemoteStorageSettings()
        {
            Write()
                .SetStyle("sub-settings")
                .Sub(
                    new InputElement().Write()
                        .UseState("RemoteAddress")
                        .Feature<MainTextFeature>(f => f.SetText("URL")),
                    new InputElement().Write()
                        .UseState("Port")
                        .Feature<Input>(f => f.Numbers(4))
                        .Feature<MainTextFeature>(f => f.SetText("Port")),
                    new FlagElement().Write()
                        .UseState("UseHTTPS")
                        .Feature<MainTextFeature>(f => f.SetText("HTTPS"))
                )
                .GroupHorizontal();
        }

        protected override BaseElement GetEmptyClone() => new RemoteStorageSettings();
    }
    

    public class OptionsButtonLine : BaseElement
    {
        private readonly IElementSetupRead _proto;
        private Button[] _buttons;

        public struct Button
        {
            public string id;
            public string name;

            public Button(string id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        public OptionsButtonLine(IElementSetupRead proto, params (string, string)[] buttons) : this
            (proto, buttons.Select(x => new Button(x.Item1, x.Item2)).ToArray())
        {
        }

        public OptionsButtonLine(IElementSetupRead proto, params Button[] buttons)
        {
            _proto = proto;
            _buttons = new Button[buttons.Length];
            buttons.CopyTo(_buttons, 0);

            SetStyle("button-line");
            GroupHorizontal();
            foreach (var button in buttons) {
                Sub(
                    proto.Write()
                        .Clone()
                        .SetId(button.id)
                        .Feature<MainTextFeature>(f => f.SetText(button.name))
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