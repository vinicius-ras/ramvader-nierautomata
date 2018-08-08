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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TrainerApp.Comparers;

namespace TrainerApp
{
	/// <summary>Interaction logic for ProcessSelectionDialog.xaml.</summary>
	public partial class ProcessSelectionDialog : Window
	{
		#region PRIVATE CONSTANTS
		/// <summary>The interval used to update the list of processes running in the system, given in miliseconds.</summary>
		private const int PROCESSES_LIST_UPDATING_INTERVAL = 2000;
		/// <summary>A <see cref="Brush"/> object with a transparent color.</summary>
		private readonly Brush TRANSPARENT_BRUSH = new SolidColorBrush( Color.FromArgb( 0, 0, 0, 0 ) );
		#endregion





		#region PRIVATE FIELDS
		/// <summary>Timer used to periodically update the list of processes running locally.</summary>
		private Timer m_processesListUpdateTimer = new Timer( PROCESSES_LIST_UPDATING_INTERVAL );
		/// <summary>
		///    Keeps the default <see cref="Brush"/> used by the <see cref="System.Windows.Controls.TextBox"/> where
		///    the user types the filter of the processes' list.
		/// </summary>
		private Brush m_defaultFilterTextBoxBrush;
		/// <summary>
		///    Maps the PID of a given <see cref="Process"/> to the reference of that <see cref="Process"/> object which
		///    is kept in the <see cref="AvailableProcesses"/> collection.
		/// </summary>
		private Dictionary<int, Process> m_pidToProcess = new Dictionary<int, Process>();
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>The collection of available processes running on the local system.</summary>
		public ObservableCollection<Process> AvailableProcesses { get; } = new ObservableCollection<Process>();
		/// <summary>Retrieves the <see cref="Process"/> selected by the user.</summary>
		public Process SelectedProcess { get; private set; }
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		public ProcessSelectionDialog()
		{
			InitializeComponent();

			// Hide the caret on the filter text box (by setting it to a transparent brush)
			m_defaultFilterTextBoxBrush = m_txtFilter.CaretBrush;
			m_txtFilter.CaretBrush = TRANSPARENT_BRUSH;


			// Use a custom sorting algorithm for the list of processes view
			ProcessComparer processComparer = new ProcessComparer();

			ListCollectionView processesListCollectionView = (ListCollectionView) m_lstProcesses.ItemsSource;
			processesListCollectionView.CustomSort = processComparer;


			// Configure the Timer used to update the list of processes
			Action updateProcessesListAction = () =>
			{
				Process selectedProcess = (Process) m_lstProcesses.SelectedItem;

				AvailableProcesses.Clear();
				foreach ( Process curProc in Process.GetProcesses() )
				{
					AvailableProcesses.Add( curProc );
					if ( selectedProcess != null && curProc.Id == selectedProcess.Id )
						m_lstProcesses.SelectedItem = curProc;
				}
			};

			m_processesListUpdateTimer.Elapsed += ( sender, args ) =>
			{
				// Use the dispatcher to update the list of processes in the same Thread that
				// created the dialog
				this.Dispatcher.Invoke( updateProcessesListAction );
			};

			// Manually fire the method that updates the list of processes, so that the list gets updated immediatelly
			// when the dialog is opened
			updateProcessesListAction();
			m_processesListUpdateTimer.Start();
		}
		#endregion





		#region EVENT CALLBACKS
		/// <summary>Called to filter the collection view used to display the list of available processes.</summary>
		/// <param name="sender">Object which sent the event.</param>
		/// <param name="e">Arguments from the event.</param>
		private void FilterProcessesList( object sender, FilterEventArgs e )
		{
			// Generate the list of words that need to be checked
			string [] wordsTyped = m_txtFilter.Text.Split(
				new string [] { " " },
				StringSplitOptions.RemoveEmptyEntries );

			// If no words were typed, there's no filter to apply
			if ( wordsTyped.Length == 0 )
			{
				e.Accepted = true;
				return;
			}

			// Retrieve the name of the process and its PID, which we'll use to filter against
			Process processToVerify = (Process) e.Item;
			string processName = string.Format( "{0} {1}",
				processToVerify.Id,
				processToVerify.ProcessName.ToLowerInvariant() );

			// Search and match ALL of the typed words
			for ( int wordIndex = wordsTyped.Length - 1; wordIndex >= 0; wordIndex-- )
			{
				string curWordToSearch = wordsTyped[wordIndex].ToLowerInvariant();
				if ( processName.Contains( curWordToSearch ) == false )
				{
					// One of the words hasn't matched... Filter that process out!
					e.Accepted = false;
					return;
				}
			}

			e.Accepted = true;
		}


		/// <summary>Called when the text box containing the filter to apply to the list of processes gets focus.</summary>
		/// <param name="sender">Object which sent the event.</param>
		/// <param name="e">Arguments from the event.</param>
		private void TextBoxProcessesFilterTextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
		{
			// Depending on the filter text the user typed, change the visibility of the caret and placeholder
			// that is displayed on the filter text box
			if ( string.IsNullOrWhiteSpace( m_txtFilter.Text ) )
			{
				// Hide the caret and show the placeholder
				m_txtFilter.CaretBrush = TRANSPARENT_BRUSH;
				m_lblFilterPlaceholder.Visibility = Visibility.Visible;
			}
			else
			{
				// Show the caret, hide the placeholder
				m_txtFilter.CaretBrush = m_defaultFilterTextBoxBrush;
				m_lblFilterPlaceholder.Visibility = Visibility.Hidden;
			}

			// Refresh the list of processes
			CollectionView collView = (CollectionView) m_lstProcesses.ItemsSource;
			collView.Refresh();
		}


		/// <summary>Called when the window is about to be closed.</summary>
		/// <param name="sender">Object which sent the event.</param>
		/// <param name="e">Arguments from the event.</param>
		private void WindowClosing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			m_processesListUpdateTimer.Stop();
		}


		/// <summary>Called when the user clicks the "Ok" button.</summary>
		/// <param name="sender">Object which sent the event.</param>
		/// <param name="e">Arguments from the event.</param>
		private void ButtonOkClick( object sender, RoutedEventArgs e )
		{
			this.SelectedProcess = (Process) m_lstProcesses.SelectedItem;
			this.DialogResult = true;
			this.Close();
		}
		#endregion
	}
}
