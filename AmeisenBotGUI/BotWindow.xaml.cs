﻿using AmeisenAI;
using AmeisenLogging;
using AmeisenManager;
using AmeisenUtilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für mainscreenForm.xaml
    /// </summary>
    public partial class BotWindow : Window
    {
        public BotWindow(WowExe wowExe)
        {
            InitializeComponent();
            BotManager = AmeisenBotManager.Instance;

            // Load Settings
            BotManager.LoadSettingsFromFile(wowExe.characterName);
            ApplyConfigColors();
            BotManager.StartBot(wowExe);
        }

        private string lastImgPath;

        private DispatcherTimer uiUpdateTimer;

        private AmeisenBotManager BotManager { get; }

        private void ApplyConfigColors()
        {
            Application.Current.Resources["AccentColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.accentColor);
            Application.Current.Resources["BackgroundColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.backgroundColor);
            Application.Current.Resources["TextColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.textColor);

            Application.Current.Resources["MeNodeColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.meNodeColor);
            Application.Current.Resources["WalkableNodeColorLow"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorLow);
            Application.Current.Resources["WalkableNodeColorHigh"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.walkableNodeColorHigh);

            Application.Current.Resources["healthColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.healthColor);
            Application.Current.Resources["energyColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.energyColor);
            Application.Current.Resources["expColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.expColor);
            Application.Current.Resources["targetHealthColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.targetHealthColor);
            Application.Current.Resources["targetEnergyColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.targetEnergyColor);
            Application.Current.Resources["threadsColor"] = (Color)ColorConverter.ConvertFromString(BotManager.Settings.threadsColor);
        }

        private void ButtonCobatClassEditor_Click(object sender, RoutedEventArgs e)
        {
            new CombatClassWindow().Show();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            BotManager.StopBot();
        }

        private void ButtonExtendedDebugUI_Click(object sender, RoutedEventArgs e)
        {
            new DebugWindow().Show();
        }

        private void ButtonGroup_Click(object sender, RoutedEventArgs e)
        {
            new GroupWindow().Show();
        }

        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            new MapWindow().Show();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonMoveInteractTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.INTERACT_TARGET, (AmeisenActionType)comboboxInteraction.SelectedItem, null));
        }

        private void ButtonMoveToTarget_Click(object sender, RoutedEventArgs e)
        {
            BotManager.AddActionToAIQueue(new AmeisenAction(AmeisenActionType.MOVE_TO_POSITION, null, null));
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                AddExtension = true,
                RestoreDirectory = true,
                Filter = "CombatClass JSON|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AmeisenBotManager.Instance.LoadCombatClass(openFileDialog.FileName);
            }
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void CheckBoxAssistPartyAttack_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyBuff_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToBuff = (bool)checkBoxAssistPartyBuff.IsChecked;
        }

        private void CheckBoxAssistPartyHeal_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToHeal = (bool)checkBoxAssistPartyAttack.IsChecked;
        }

        private void CheckBoxAssistPartyTank_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToTank = (bool)checkBoxAssistPartyTank.IsChecked;
        }

        private void CheckBoxFollowMaster_Click(object sender, RoutedEventArgs e)
        {
            AmeisenBotManager.Instance.FollowGroup = (bool)checkBoxFollowMaster.IsChecked;
        }

        private void CheckBoxTopMost_Click(object sender, RoutedEventArgs e)
        {
            SetTopMost();
        }

        private void SetTopMost()
        {
            Topmost = (bool)checkBoxTopMost.IsChecked;
            BotManager.Settings.topMost = (bool)checkBoxTopMost.IsChecked;
        }

        private void Mainscreen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveViewSettings();
        }

        private void SaveViewSettings()
        {
            BotManager.Settings.behaviourAttack = (bool)checkBoxAssistPartyAttack.IsChecked;
            BotManager.Settings.behaviourTank = (bool)checkBoxAssistPartyTank.IsChecked;
            BotManager.Settings.behaviourHeal = (bool)checkBoxAssistPartyHeal.IsChecked;
            BotManager.Settings.behaviourBuff = (bool)checkBoxAssistPartyBuff.IsChecked;
            BotManager.Settings.followMaster = (bool)checkBoxFollowMaster.IsChecked;
            BotManager.SaveSettingsToFile(BotManager.GetLoadedConfigName());
        }

        private void Mainscreen_Loaded(object sender, RoutedEventArgs e)
        {
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Loaded MainScreen", this);

            Title = $"AmeisenBot - {BotManager.GetWowExe().characterName} [{BotManager.GetWowExe().process.Id}]";

            UpdateUI();
            StartUIUpdateTime();

            LoadViewSettings();

            DebugLoadDefaultValues();
        }

        private void StartUIUpdateTime()
        {
            uiUpdateTimer = new DispatcherTimer();
            uiUpdateTimer.Tick += new EventHandler(UIUpdateTimer_Tick);
            uiUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, BotManager.Settings.dataRefreshRate);
            uiUpdateTimer.Start();
            AmeisenLogger.Instance.Log(LogLevel.DEBUG, "Started UI-Update-Timer", this);

        }

        private void DebugLoadDefaultValues()
        {
            comboboxInteraction.Items.Add(InteractionType.FACETARGET);
            comboboxInteraction.Items.Add(InteractionType.FACEDESTINATION);
            comboboxInteraction.Items.Add(InteractionType.STOP);
            comboboxInteraction.Items.Add(InteractionType.MOVE);
            comboboxInteraction.Items.Add(InteractionType.INTERACT);
            comboboxInteraction.Items.Add(InteractionType.LOOT);
            comboboxInteraction.Items.Add(InteractionType.INTERACTOBJECT);
            comboboxInteraction.Items.Add(InteractionType.FACEOTHER);
            comboboxInteraction.Items.Add(InteractionType.SKIN);
            comboboxInteraction.Items.Add(InteractionType.ATTACK);
            comboboxInteraction.Items.Add(InteractionType.ATTACKPOS);
            comboboxInteraction.Items.Add(InteractionType.ATTACKGUID);
            comboboxInteraction.Items.Add(InteractionType.WALKANDROTATE);
        }

        private void LoadViewSettings()
        {
            checkBoxAssistPartyAttack.IsChecked = BotManager.Settings.behaviourAttack;
            BotManager.IsAllowedToAttack = BotManager.Settings.behaviourAttack;

            checkBoxAssistPartyTank.IsChecked = BotManager.Settings.behaviourTank;
            BotManager.IsAllowedToTank = BotManager.Settings.behaviourTank;

            checkBoxAssistPartyHeal.IsChecked = BotManager.Settings.behaviourHeal;
            BotManager.IsAllowedToHeal = BotManager.Settings.behaviourHeal;

            checkBoxAssistPartyBuff.IsChecked = BotManager.Settings.behaviourBuff;
            BotManager.IsAllowedToBuff = BotManager.Settings.behaviourBuff;

            checkBoxAssistGroup.IsChecked = BotManager.Settings.assistParty;
            BotManager.IsAllowedToAssistParty = BotManager.Settings.assistParty;

            checkBoxFollowMaster.IsChecked = BotManager.Settings.followMaster;
            AmeisenBotManager.Instance.FollowGroup = BotManager.Settings.followMaster;

            sliderDistance.Value = BotManager.Settings.followDistance;

            checkBoxTopMost.IsChecked = BotManager.Settings.topMost;
            Topmost = BotManager.Settings.topMost;
        }

        private void Mainscreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void SliderDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                labelDistance.Content = $"Follow Distance: {Math.Round(sliderDistance.Value, 2)}m";
                BotManager.AmeisenSettings.Settings.followDistance = Math.Round(sliderDistance.Value, 2);
            }
            catch { }
        }

        private void UIUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (BotManager.IsBotIngame())
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// This thing updates the UI... Note to myself: "may need to improve this thing in the future..."
        /// </summary>
        private void UpdateUI()
        {
            // TODO: find a better way to update this
            //AmeisenManager.Instance.GetObjects();

            labelLoadedCombatClass.Content = $"CombatClass: {Path.GetFileName(BotManager.Settings.combatClassPath)}";

            if (BotManager.Me != null)
            {
                try
                {
                    UpdateMyViews();

                    if (BotManager.Target != null)
                    {
                        UpdateTargetViews();
                    }
                }
                catch (Exception e)
                {
                    AmeisenLogger.Instance.Log(LogLevel.ERROR, e.ToString(), this);
                }
            }

            UpdateAIView();
        }

        private void UpdateAIView()
        {
            labelThreadsActive.Content = $"⚡ Threads: {BotManager.AmeisenAIManager.GetBusyThreadCount()}/{BotManager.AmeisenAIManager.GetActiveThreadCount()}";
            progressBarBusyAIThreads.Maximum = BotManager.AmeisenAIManager.GetActiveThreadCount();
            progressBarBusyAIThreads.Value = BotManager.AmeisenAIManager.GetBusyThreadCount();
        }

        private void UpdateTargetViews()
        {
            labelNameTarget.Content = $"{BotManager.Target.Name} lvl.{BotManager.Target.Level}";

            labelTargetHP.Content = $"Health {BotManager.Target.Health} / {BotManager.Target.MaxHealth}";
            progressBarHPTarget.Maximum = BotManager.Target.MaxHealth;
            progressBarHPTarget.Value = BotManager.Target.Health;

            labelTargetEnergy.Content = $"Energy {BotManager.Target.Energy} / {BotManager.Target.MaxEnergy}";
            progressBarEnergyTarget.Maximum = BotManager.Target.MaxEnergy;
            progressBarEnergyTarget.Value = BotManager.Target.Energy;

            labelTargetDistance.Content = $"Distance: {BotManager.Target.Distance}m";
        }

        private void UpdateMyViews()
        {
            if (BotManager.Settings.picturePath != lastImgPath)
                if (BotManager.Settings.picturePath.Length > 0)
                {
                    botPicture.Source = new BitmapImage(new Uri(BotManager.Settings.picturePath));
                    lastImgPath = BotManager.Settings.picturePath;
                }

            labelName.Content = BotManager.Me.Name + " lvl." + BotManager.Me.Level;

            labelHP.Content = $"Health {BotManager.Me.Health} / {BotManager.Me.MaxHealth}";
            progressBarHP.Maximum = BotManager.Me.MaxHealth;
            progressBarHP.Value = BotManager.Me.Health;

            labelEnergy.Content = $"Energy {BotManager.Me.Energy} / {BotManager.Me.MaxEnergy}";
            progressBarEnergy.Maximum = BotManager.Me.MaxEnergy;
            progressBarEnergy.Value = BotManager.Me.Energy;

            labelExp.Content = $"Exp {BotManager.Me.Exp} / {BotManager.Me.MaxExp}";
            progressBarXP.Maximum = BotManager.Me.MaxExp;
            progressBarXP.Value = BotManager.Me.Exp;
        }

        private void CheckBoxAssistGroup_Click(object sender, RoutedEventArgs e)
        {
            BotManager.IsAllowedToAssistParty = (bool)checkBoxAssistGroup.IsChecked;
        }
    }
}