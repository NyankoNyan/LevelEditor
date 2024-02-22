using Level.API;

using UI2;

using UnityEngine.Assertions;

namespace RuntimeEditTools.UI
{
    public class LevelStorageMenu : BaseElement
    {
        private LevelStorageMenu() { }

        public static IElementSetup Create(LevelAPI level)
        {
            Assert.IsNotNull(level);
            /* ******************************
             * *    LEVEL_NAME              *
             * * STORAGE_MODE(LOCAL/REMOTE) *
             * *    storage_settings_frame  *
             * *  NEW  OPEN  SAVE  SAVE_AS  *
             * ****************************** */
            return new LevelStorageMenu()
                .SetId(nameof(LevelStorageMenu))
                .SetStyle("window")
                .State("StorageMode", 0)
                .Sub(
                    new InputElement()
                        .SetId("LevelName")
                        .Feature<MainTextFeature>(f =>
                            f.SetText(level.LevelSettings.name)),
                    new OptionsButtonLine(
                            new ButtonElement(),
                            ("LOCAL", "Local Storage"),
                            ("REMOTE", "Remote storage")
                        )
                        ,
                    new LocalStorageSettings()
                        .SetId("LocalFrame")
                        .Lazy(),
                    new RemoteStorageSettings()
                        .SetId("RemoteFrame")
                        .Lazy(),
                    new OptionsButtonLine(
                        new ButtonElement(),
                        ("NEW", "New"),
                        ("OPEN", "Open..."),
                        ("SAVE", "Save"),
                        ("SAVE_AS", "Save As...")
                    )
                )
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
                .GroupVertical()
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
        protected override BaseElement GetEmptyClone() => new LocalStorageSettings();
    }

    public class RemoteStorageSettings : BaseElement
    {
        protected override BaseElement GetEmptyClone() => new RemoteStorageSettings();
    }

    public class ButtonLine : BaseElement
    {
        protected override BaseElement GetEmptyClone() => new ButtonLine();
    }

    public class OptionsButtonLine : BaseElement
    {
        private readonly IElementSetup _proto;
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

        public OptionsButtonLine(IElementSetup proto, params (string, string)[] buttons) : this
            (proto, buttons.Select(x => new Button(x.Item1, x.Item2)).ToArray())
        {
        }

        public OptionsButtonLine(IElementSetup proto, params Button[] buttons)
        {
            _proto = proto;
            _buttons = new Button[buttons.Length];
            buttons.CopyTo(_buttons, 0);

            SetStyle("button-line");
            GroupHorizontal();
            foreach (var button in buttons) {
                Sub(
                    proto
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