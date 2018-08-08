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

using System.Diagnostics;
using System.Windows;

namespace TrainerApp
{
	/// <summary>Interaction logic for HelpDialog.xaml.</summary>
	public partial class HelpDialog : Window
	{
		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		public HelpDialog()
		{
			InitializeComponent();

			string htmlToDisplay = Properties.Resources.strHelpContentsHTML;
			if ( string.IsNullOrWhiteSpace( htmlToDisplay ) == false )
				m_webBrowser.NavigateToString( htmlToDisplay );
		}


		/// <summary>
		///    Called when the <see cref="System.Windows.Controls.WebBrowser"/> that displays the
		///    help contents is navigating to another page.
		/// </summary>
		/// <param name="sender">Object which sent the event.</param>
		/// <param name="e">Arguments from the event.</param>
		private void WebBrowserNavigating( object sender, System.Windows.Navigation.NavigatingCancelEventArgs e )
		{
			if ( e.Uri != null )
			{
				// Veto the navigation on the WebBrowser control
				e.Cancel = true;

				// Use an external browser to navigate to the URL
				Process.Start( e.Uri.ToString() );
			}
		}
		#endregion
	}
}
