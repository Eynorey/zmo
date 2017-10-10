using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechRecognition;
using Windows.Storage;

// Die Vorlage "Leere Anwendung" ist unter http://go.microsoft.com/fwlink/?LinkId=234227 dokumentiert.

namespace ZMO
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird.  Weitere Einstiegspunkte
        /// werden verwendet, wenn die Anwendung zum Öffnen einer bestimmten Datei, zum Anzeigen
        /// von Suchergebnissen usw. gestartet wird.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>

        private void Launch()
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Einen Rahmen erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                // TODO: diesen Wert auf eine Cachegröße ändern, die für Ihre Anwendung geeignet ist
                rootFrame.CacheSize = 2;

                // Den Rahmen im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Entfernt die Drehkreuznavigation für den Start.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();
        }
        
        
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Launch();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Stellt die Inhaltsübergänge nach dem Start der App wieder her.
        /// </summary>
        /// <param name="sender">Das Objekt, an das der Handler angefügt wird.</param>
        /// <param name="e">Details zum Navigationsereignis.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            // Was the app activated by a voice command?
            if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.VoiceCommand)
            {
                var commandArgs = args as Windows.ApplicationModel.Activation.VoiceCommandActivatedEventArgs;
                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                // If so, get the name of the voice command, the actual text spoken, and the value of Command/Navigate@Target.
                string voiceCommandName = speechRecognitionResult.RulePath[0];
                var test = speechRecognitionResult.SemanticInterpretation.Properties;

                switch (voiceCommandName)
                {
                    case "zensurNeuCortana":
                        ApplicationData.Current.RoamingSettings.Values["cortanaText"] = speechRecognitionResult.Text;
                        Launch();
                        break;

                    // Cases for other voice commands.

                    default:
                        // There is no match for the voice command name.
                        Launch();
                        break;
                }
            }
        }

    }
}