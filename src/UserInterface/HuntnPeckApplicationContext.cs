﻿using HuntAndPeck.UserInterface.Renderer;
using HuntAndPeck.Engine.Services;
using System;
using System.Linq;
using System.Windows.Forms;
using HuntAndPeck.UserInterface.Forms;
using Autofac;
using HuntAndPeck.Engine;
using HuntAndPeck.Engine.Services.Interfaces;

namespace HuntAndPeck.UserInterface
{
    public class HuntAndPeckApplicationContext : ApplicationContext
    {
        private IContainer _container;

        public HuntAndPeckApplicationContext()
        {
            Bootstrap();
        }

        private void Bootstrap()
        {
            BuildContainer();
            ShowTray();
        }

        private void ShowTray()
        {
            var tray = _container.Resolve<TrayListener>();
            tray.HotKey = new Tuple<NativeMethods.KeyModifier, Keys>(NativeMethods.KeyModifier.Alt, Keys.OemSemicolon);

            tray.OnHotKeyActivated += () =>
            {
                var hintSession = _container.Resolve<IHintProviderService>().EnumHints();

                using (var overlay = _container.Resolve<HintOverlay>())
                {
                    overlay.HintSession = hintSession;

                    if (overlay.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        var selectedHint = overlay.SelectedHint;

                        // TODO: Make this "capability" focused, rather than hint
                        var frist = selectedHint.Capabilities.FirstOrDefault();
                        if (frist != null)
                        {
                            frist.Activate();
                        }
                    }
                }
            };

            tray.Show();
        }

        /// <summary>
        /// Builds the super-light-weight IoC container to stop me going mad
        /// </summary>
        private void BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Renderer.HintRenderer>()
                   .As<IHintRenderer>()
                   .SingleInstance();

            // For our forms
            builder.RegisterType<OptionsForm>()
                   .AsSelf()
                   .ExternallyOwned();

            builder.RegisterType<TrayListener>()
                   .AsSelf()
                   .ExternallyOwned();

            builder.RegisterType<HintOverlay>()
                   .AsSelf()
                   .ExternallyOwned();

            // No good without an engine ;)
            builder.RegisterModule(new EngineAutofacModule());

            _container = builder.Build();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
