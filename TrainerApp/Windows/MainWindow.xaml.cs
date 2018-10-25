/*
 * Copyright (C) 2018 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader-NieRAutomata.
 *
 * RAMvader-NieRAutomata is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * RAMvader-NieRAutomata is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with RAMvader-NieRAutomata. If not, see <http://www.gnu.org/licenses/>.
 */
using RAMvader;
using RAMvader.CodeInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace TrainerApp
{
    /// <summary>Interaction logic for MainWindow.xaml.</summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region PRIVATE CONSTANTS
        /// <summary>The name of the process which runs the game.</summary>
        private const string GAME_PROCESS_NAME = "NieRAutomata";
        /// <summary>The URL which points to the webpage that runs when the user clicks the "Donate!" button of the trainer.</summary>
        private const string PROJECT_DONATION_URL = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WJ2D2KRMPRKBS&lc=US&item_name=Supporting%20Vinicius%2eRAS%27%20open%20source%20projects&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted";
        /// <summary>The period of the timer used to look for the game's process, when the trainer is not attached. This number is given in miliseconds.</summary>
        private const int TIMER_LOOK_FOR_GAME_PROCESS_PERIOD_MS = 1000;
        #endregion





        #region PRIVATE FIELDS
        /// <summary>A timer used to periodically look for the game's process, when the trainer is not attached to the game.</summary>
        private Timer m_gameSearchTimer = null;
        /// <summary>A set containing all the cheats the user has enabled in the trainer.</summary>
        private HashSet<ECheat> m_enabledCheats = new HashSet<ECheat>();
        /// <summary>Backs the <see cref="AutomaticallySearchForGame"/> property.</summary>
        private bool m_automaticallySearchForGame = true;
        #endregion





        #region PUBLIC PROPERTIES
        /// <summary>An object used for performing I/O operations on the game process' memory. </summary>
        public Target GameMemoryIO { get; private set; }
        /// <summary>An object used for injecting and managing code caves and arbitrary variables into the
        /// game process' memory.</summary>
        public Injector<ECheat, ECodeCave, EVariable> GameMemoryInjector { get; private set; }
        /// <summary>
        ///    <para>
        ///       A flag indicating if the trainer should automatically search for the game's process.
        ///       Changing this flag will start or stop the timer which searches for the game's process accordingly.
        ///       This property implements the "Property Changed" notifications, so it can be safely used in WPF Bindings.
        ///    </para>
        ///    <para>IMPORTANT: To enable or disable the search timer, use <see cref="SetAutomaticSearchForGameProcessEnabled(bool)"/>.</para>
        /// </summary>
        public bool AutomaticallySearchForGame
        {
            get => m_automaticallySearchForGame;
            set
            {
                // Update the property
                m_automaticallySearchForGame = value;

                // Enable or disable the timer, whenever necessary
                if (m_gameSearchTimer != null)
                    m_gameSearchTimer.Enabled = GameMemoryIO.IsAttached() ? false : value;

                // Send a "property changed" notification (for WPF Bindings to be updated)
                SendPropertyChangedNotification();
            }
        }
        #endregion





        #region PRIVATE METHODS
        /// <summary>
        ///    Called before the code injection process takes place on the target process' memory space, and is used to
        ///    set up the Code Caves and Injection Variables used by the trainer to alter the game's behaviours.
        /// </summary>
        private void BeforeInjection()
        {
            // Clear any previous data that has been defined for code caves and variables
            GameMemoryInjector.ClearAllCodeCaveDefinitions();
            GameMemoryInjector.ClearAllVariableDefinitions();

            // Setup code caves and injection variables
            IntPtr mainModuleAddress = GameMemoryIO.GetTargetProcessModuleBaseAddress("NieRAutomata.exe");
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveInfiniteItems,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0xC7, 0x40, 0x08, 0x32, 0x00, 0x00, 0x00, 0x83, 0x78, 0x08, 0x00, 0xE9, 0x11, 0x2E, 0x8A, 0xFF)
                    .Build()
            );
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveLevelUp,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0x51, 0x81, 0xC1, 0xA0, 0x86, 0x01, 0x00, 0x89, 0x0D, 0x6E, 0xA4, 0xC4, 0x00, 0x59, 0xE9, 0xAD, 0xBF, 0x85, 0xFF)
                    .Build()
            );
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveOneHitKillsEnemies,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0x56, 0xC1, 0xE6, 0x10, 0x29, 0xB7, 0x58, 0x08, 0x00, 0x00, 0x5E, 0xC3)
                    .Build());
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveOneHitKillsAnimals,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0x56, 0xC1, 0xE6, 0x10, 0x29, 0xB3, 0x58, 0x08, 0x00, 0x00, 0x5E, 0xC3)
                    .Build());
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveEasyMoney,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0x56, 0xC1, 0xE6, 0x09, 0x01, 0x30, 0x5E, 0x48, 0xA1, 0x88, 0x61, 0x0F, 0x41, 0x01, 0x00, 0x00, 0x00, 0xC3)
                    .Build());
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveInstantSkillRecharge,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0x51, 0x48, 0x31, 0xC9, 0x48, 0x89, 0x8C, 0xC3, 0x24, 0x6A, 0x01, 0x00, 0x59, 0xC3)
                    .Build());
            GameMemoryInjector.SetCodeCaveDefinition(ECodeCave.evCodeCaveInstantHacking,
                GameMemoryInjector.NewCodeCave()
                    .Bytes(0xC7, 0x86, 0x74, 0x6D, 0x01, 0x00, 0x00, 0x00, 0x80, 0x3F, 0xC3)
                    .Build());
        }



        /// <summary>
        ///    Called after the code injection process takes place on the target process' memory space, and is used to
        ///    setup memory alterations and enable any memory alteration(s) that need to be always present once the
        ///    trainer gets attached to the target (game's) process. See remarks for recommendations about that.
        /// </summary>
        /// <remarks>
        ///	   <para>
        ///	      If there are any Memory Alteration Sets that need to be ALWAYS ACTIVE while the trainer is attached to
        ///	      the game, there are some standard steps that can be followed. These are not rules, but rather standards
        ///	      that are recommended.
        ///	      It is recommended that you follow the rules below for each Memory Alteration that should be always active.
        ///	   </para>
        ///	   <para>First: declare all of the Memory Alterations that need to be ALWAYS ACTIVE in the PRIVATE FIELDS region.</para>
        ///	   <code>
        ///	      private MemoryAlterationX86Call m_memAlterationStorePlayerHPAddress;
        ///	   </code>
        ///	   <para>Second: instantiate the Memory Alteration object in the <see cref="AfterInjection()"/> method, and then ENABLE IT MANUALLY.
        ///	   <code>
        ///	      m_memAlterationStorePlayerHPAddress = new MemoryAlterationX86Call( GameMemoryIO, mainModuleAddress + 0x117A80E, ECodeCave.evCodeCaveStorePtrCharHP, 6 );
        ///	      m_memAlterationStorePlayerHPAddress.SetEnabled( GameMemoryInjector, true );
        ///	   </code>
        ///	   <para>
        ///	      Third: write code on the <see cref="BeforeDetachment"/> method to DISABLE the Memory Alteration manually, and set its reference
        ///	      to null for safety.
        ///	   </para>
        ///	   <code>
        ///	      m_memAlterationStorePlayerHPAddress.SetEnabled( GameMemoryInjector, false );
        ///	      m_memAlterationStorePlayerHPAddress = null;
        ///	   </code>
        /// </remarks>
        private void AfterInjection()
        {
            IntPtr mainModuleAddress = GameMemoryIO.GetTargetProcessModuleBaseAddress("NieRAutomata.exe");
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatGodMode, new MemoryAlterationNOP(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x1FA423), 6));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatGodMode, new MemoryAlterationNOP(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x1FA5B1), 6));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatInfiniteItems, new MemoryAlterationPoke(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x5DCFF6), new byte[] { 0xE9, 0xE2, 0xD1, 0x75, 0x00, 0x90, 0x90, 0x90 }));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatLevelUp, new MemoryAlterationPoke(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x5961AF), new byte[] { 0xE9, 0x41, 0x40, 0x7A, 0x00, 0x90 }));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatOneHitKillsEnemies, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x34748A6), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveOneHitKillsEnemies), EX86BranchInstructionType.evCallNearRelative32, 6));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatOneHitKillsAnimals, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x18D3FC), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveOneHitKillsAnimals), EX86BranchInstructionType.evCallNearRelative32, 6));

            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatEasyMoney, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x1FFF98), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveEasyMoney), EX86BranchInstructionType.evCallNearRelative32, 9));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatEasyMoney, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x200060), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveEasyMoney), EX86BranchInstructionType.evCallNearRelative32, 9));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatEasyMoney, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x5E46CE), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveEasyMoney), EX86BranchInstructionType.evCallNearRelative32, 9));

            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatInstantSkillRecharge, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x23821F), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveInstantSkillRecharge), EX86BranchInstructionType.evCallNearRelative32, 9));
            GameMemoryInjector.AddMemoryAlteration(ECheat.evCheatInstantHacking, new MemoryAlterationX86BranchInstruction(GameMemoryIO, new AbsoluteMemoryAddress(mainModuleAddress + 0x258064), GameMemoryInjector.GetInjectedCodeCaveAddress(ECodeCave.evCodeCaveInstantHacking), EX86BranchInstructionType.evCallNearRelative32, 10));
        }


        /// <summary>
        ///    Called right before the injection process is reverted, and before the trainer is detached from the target
        ///    game's process. This method should be used to disable any Memory Alteration that is "always active" while
        ///    the trainer is attached to the game.
        /// </summary>
        private void BeforeDetachment()
        {
            // Remove all Memory Alterations that have been registered
            foreach (ECheat curCheatId in Enum.GetValues(typeof(ECheat)))
            {
                // Use a clone of the memory alterations (because the collection will be altered, which fires an exception in the following foreach loop in some cases)
                IEnumerable<MemoryAlterationBase> memAlterations = new List<MemoryAlterationBase>(GameMemoryInjector.GetMemoryAlterations(curCheatId));
                foreach (MemoryAlterationBase curMemoryAlteration in memAlterations)
                    GameMemoryInjector.RemoveMemoryAlteration(curCheatId, curMemoryAlteration);
            }
        }


        /// <summary>Enables or disables a timer which is responsible for periodically looking for the game's process automatically.</summary>
        /// <param name="enableAutoSearch">A flag indicating if the timer should be enabled or disabled.</param>
        private void SetAutomaticSearchForGameProcessEnabled(bool enableAutoSearch)
        {
            // Instantiate and configure the timer which looks for the game automatically (if necessary)
            if (m_gameSearchTimer == null)
            {
                m_gameSearchTimer = new Timer(TIMER_LOOK_FOR_GAME_PROCESS_PERIOD_MS);
                m_gameSearchTimer.AutoReset = false;

                // Define the method to be executed when the timer is elapsed
                m_gameSearchTimer.Elapsed += (timerEvtSender, timerEvtArgs) =>
            {
                // Force timer's task to be executed in the SAME THREAD as the MainWindow
                this.Dispatcher.Invoke(() =>
               {
                   // This flag controls whether the timer used for looking for the game's process should be restarted or not
                   bool bRestartLookForGameTimer = true;

                   // Try to find the game's process
                   Process gameProcess = Process.GetProcessesByName(GAME_PROCESS_NAME).FirstOrDefault();
                   if (gameProcess != null)
                       this.AttachToGame(gameProcess, ref bRestartLookForGameTimer);

                   // Update the "timer enabled" flag
                   m_gameSearchTimer.Enabled = bRestartLookForGameTimer;
               });
            };
            }

            // Start/restart or stop the timer
            AutomaticallySearchForGame = enableAutoSearch;
        }


        /// <summary>Called when the trainer needs to be attached to the game's process.</summary>
        /// <param name="gameProcess">The <see cref="Process"/> where the game is running on.</param>
        /// <param name="bRestartLookForGameTimer">
        ///    This flag is filled with a value of <code>false</code> whenever the process of looking
        ///    for the game's process needs to be restarted. This flag should only be considered if the automatic
        ///    game search is on - otherwise its value can be ignored.
        /// </param>
        private void AttachToGame(Process gameProcess, ref bool bRestartLookForGameTimer)
        {
            // Try to attach to the game's process
            if (GameMemoryIO.AttachToProcess(gameProcess))
            {
                // Inject the trainer's code and variables into the game's memory!
                bool exceptionThrown = false;
                try
                {
                    this.BeforeInjection();
                    IntPtr mainModuleAddress = GameMemoryIO.GetTargetProcessModuleBaseAddress("NieRAutomata.exe");
                    GameMemoryInjector.Inject(new AbsoluteMemoryAddress(mainModuleAddress+0xD3A1DD));
                    this.AfterInjection();
                }
                catch (Win32Exception)
                {
#if DEBUG
                    Debugger.Break();
#endif
                    exceptionThrown = true;
                }

                // If no exception has been caught, continue with the process
                if (exceptionThrown == false)
                {
                    // When the game's process exits, the MainWindow's dispatcher is used to invoke the DetachFromGame() method
                    // in the same thread which "runs" our MainWindow
                    GameMemoryIO.TargetProcess.EnableRaisingEvents = true;
                    GameMemoryIO.TargetProcess.Exited += GameProcessExited;

                    // Enable the cheats that the user has checked in the trainer's interface
                    foreach (ECheat curEnabledCheat in m_enabledCheats)
                        GameMemoryInjector.SetMemoryAlterationsActive(curEnabledCheat, true);

                    // In DEBUG mode, print some debugging information that might be interesting when developing cheats
#if DEBUG
                    int longestCodeCaveNameLength = 0;
                    foreach (ECodeCave curCodeCave in Enum.GetValues(typeof(ECodeCave)))
                        longestCodeCaveNameLength = Math.Max(longestCodeCaveNameLength, curCodeCave.ToString().Length);

                    int longestVariableNameLength = 0;
                    foreach (EVariable curVariable in Enum.GetValues(typeof(EVariable)))
                        longestVariableNameLength = Math.Max(longestVariableNameLength, curVariable.ToString().Length);

                    Console.WriteLine("[INJECTED: {0}]", DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss"));
                    Console.WriteLine("PID: {0}", GameMemoryIO.TargetProcess.Id.ToString("X8"));
                    Console.WriteLine("Main Module: {0} (base address: 0x{1})", GameMemoryIO.TargetProcess.MainModule.ModuleName, GameMemoryIO.TargetProcess.MainModule.BaseAddress.ToString("X8"));

                    Console.WriteLine("Injected CODE CAVES:");
                    foreach (ECodeCave curCodeCave in Enum.GetValues(typeof(ECodeCave)))
                    {
                        if (GameMemoryInjector.IsCodeCaveInjected(curCodeCave))
                            Console.WriteLine("   {0}: 0x{1}",
                                curCodeCave.ToString().PadLeft(longestCodeCaveNameLength),
                        GameMemoryInjector.GetInjectedCodeCaveAddress(curCodeCave).Address.ToString("X8"));
                    }

                    Console.WriteLine("Injected VARIABLES:");
                    foreach (EVariable curVariable in Enum.GetValues(typeof(EVariable)))
                    {
                        if (GameMemoryInjector.IsVariableInjected(curVariable))
                            Console.WriteLine("   {0}: 0x{1}",
                                curVariable.ToString().PadLeft(longestVariableNameLength),
                        GameMemoryInjector.GetInjectedVariableAddress(curVariable).Address.ToString("X8"));
                    }

                    Console.WriteLine();
#endif

                    // The timer which looks for the game shouldn't be restarted, as the game has already been found
                    bRestartLookForGameTimer = false;
                }
            }
            else
            {
                // Show a message box telling the user that the trainer has failed to attach to the game's process.
                if (MessageBox.Show(this,
                                Properties.Resources.strMsgFailedToAttach, Properties.Resources.strMsgFailedToAttachCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.No)
                {
                    bRestartLookForGameTimer = false;
                }
            }
        }


        /// <summary>Called when the trainer needs to be detached from the game's process.</summary>
        private void DetachFromGame()
        {
            // Detach from the target process
            if (GameMemoryIO.IsAttached())
            {
                // If the game's process is still running, all cheats must be disabled
                if (GameMemoryIO.TargetProcess.HasExited == false)
                {
                    foreach (ECheat curCheat in Enum.GetValues(typeof(ECheat)))
                        GameMemoryInjector.SetMemoryAlterationsActive(curCheat, false);
                }

                // In any case, we must remove the event we have placed to check when the process has finished,
                // so that it doesn't get called if we attach to another process and the former process died while
                // the later was still running (causing a crash in the trainer's app)
                GameMemoryIO.TargetProcess.Exited -= GameProcessExited;

                // Perform the tasks that need to be performed before the injection process is reverted,
                // and before the trainer is detached from the game's process.
                BeforeDetachment();

                // Release injected memory, cleanup and detach
                GameMemoryInjector.ResetAllocatedMemoryData();
                GameMemoryIO.DetachFromProcess();
            }

            // Restart the timer which looks for the game's process, if necessary
            if (AutomaticallySearchForGame)
                m_gameSearchTimer.Enabled = true;
        }


        /// <summary>Sends a "property changed" notification through the <see cref="PropertyChanged"/> event.</summary>
        /// <param name="propertyName">
        ///    The name of the property that has changed.
        ///    This name is annotated with the attribute <see cref="CallerMemberNameAttribute"/> so that when the method
        ///    is called without parameters inside the body of a setter method, it is automatically filled with the name
        ///    of the property being set.
        /// </param>
        private void SendPropertyChangedNotification([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion





        #region PUBLIC METHODS
        /// <summary>Constructor.</summary>
        public MainWindow()
        {
            // Initialize objects which will perform operations on the game's memory
            GameMemoryIO = new Target();

            GameMemoryInjector = new Injector<ECheat, ECodeCave, EVariable>();
            GameMemoryInjector.SetTargetProcess(GameMemoryIO);

            // Initialize RAMvaderTarget
            GameMemoryIO.SetTargetEndianness(EEndianness.evEndiannessLittle);
            GameMemoryIO.SetTargetPointerSize(EPointerSize.evPointerSize64);

            // Configure all tooltips to be displayed instantaneously, and to last forever
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata((Int32)0));

            // Initialize window
            InitializeComponent();

            // Start looking for the game!
            this.SetAutomaticSearchForGameProcessEnabled(AutomaticallySearchForGame);
        }
        #endregion





        #region EVENT HANDLERS
        /// <summary>Called when the #MainWindow of the trainer is about to be closed (but before actually closing it).</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void WindowClosing(object sender, CancelEventArgs e)
        {
            // Stop the timer which looks for the game's process, if it is running
            this.AutomaticallySearchForGame = false;

            // Detach the trainer, if it is attached
            if (GameMemoryIO.Attached)
                DetachFromGame();
        }


        /// <summary>
        ///    Called by a worker thread once the <see cref="Process"/> of the game the trainer is attached to is exited.
        ///    This method is added to be called by the event <see cref="Process.Exited"/> once the trainer attaches to the
        ///    game's process, and it is removed from this events' callback methods when the trainer is detached.
        /// </summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void GameProcessExited(object sender, EventArgs e)
        {
            // Execute the detachment method from the MainWindow's thread
            this.Dispatcher.Invoke(() => { this.DetachFromGame(); });
        }


        /// <summary>Called when the user clicks the "Help" menu item.</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void MenuItemHelpClicked(object sender, RoutedEventArgs e)
        {
            HelpDialog newDlg = new HelpDialog();
            newDlg.Owner = this;
            newDlg.ShowDialog();
        }


        /// <summary>Called when the user clicks the "Manual Attachment" menu item.</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void MenuItemManualAttachmentClick(object sender, RoutedEventArgs e)
        {
            // If the trainer is already attached, ask the user for confirmation
            if (GameMemoryIO.IsAttached())
            {
                if (MessageBox.Show(this,
                    Properties.Resources.strMsgConfirmDetachment, Properties.Resources.strMsgConfirmDetachmentCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    this.DetachFromGame();
                }
                else
                {
                    // MenuItems that are checkable have ther "IsChecked" property automatically inverted between checked/unchecked state.
                    // Updating the "AutomaticallySearchForGame" to the same value it holds sends a "property changed" notification
                    // which ensures that the menu item doesn't get its "IsChecked" property with a value different of
                    // the "AutomaticallySearchForGame" property.
                    this.AutomaticallySearchForGame = this.AutomaticallySearchForGame;
                    return;
                }
            }

            // Stop the timer which automatically searches for the game's process
            this.AutomaticallySearchForGame = false;

            // Open the dialog which displays the list of available processes
            ProcessSelectionDialog newDlg = new ProcessSelectionDialog();
            newDlg.Owner = this;
            if (newDlg.ShowDialog() == true)
            {
                bool dummyBool = false;
                this.AttachToGame(newDlg.SelectedProcess, ref dummyBool);
            }
        }


        /// <summary>Called when the user clicks the "Restart looking for the game" menu item.</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void MenuItemSearchAutomaticallyGameClick(object sender, RoutedEventArgs e)
        {
            this.SetAutomaticSearchForGameProcessEnabled(true);
        }


        /// <summary>Called when the user clicks the window's "exit" menu.</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void MenuItemExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// Called when the user clicks the "Donate!" button.
        /// </summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void ButtonClickDonate(object sender, RoutedEventArgs e)
        {
            Process.Start(PROJECT_DONATION_URL);
        }


        /// <summary>Called whenever any of the CheckBoxes used to activate/deactivate cheats from the trainer gets checked or unchecked.</summary>
        /// <param name="sender">Object which sent the event.</param>
        /// <param name="e">Arguments from the event.</param>
        private void CheckBoxCheatToggled(object sender, RoutedEventArgs e)
        {
            // Retrieve information which will be used to enable or disable the cheat
            CheckBox chkBox = (CheckBox)e.Source;
            ECheat cheatID = (ECheat)chkBox.Tag;
            bool bEnableCheat = (chkBox.IsChecked == true);

            // Update the list of cheats to be enabled
            if (bEnableCheat)
                m_enabledCheats.Add(cheatID);
            else
                m_enabledCheats.Remove(cheatID);

            // Enable or disable the cheat in the game's memory space
            if (GameMemoryIO.IsAttached())
                GameMemoryInjector.SetMemoryAlterationsActive(cheatID, bEnableCheat);
        }
        #endregion





        #region INTERFACE IMPLEMENTATION: INotifyPropertyChanged
        /// <summary>Used to register "property changed" events on this window's properties.</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
