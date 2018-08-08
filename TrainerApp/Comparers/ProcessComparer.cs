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
using System.Collections;
using System.Diagnostics;

namespace TrainerApp.Comparers
{
	/// <summary>An <see cref="IComparer"/> for comparing <see cref="Process"/> instances, used for sorting lists of processes.</summary>
	public class ProcessComparer : IComparer
	{
		#region INTERFACE IMPLEMENTATION: IComparer
		/// <summary>Compares two processes for sorting purposes.</summary>
		/// <param name="p1">Reference to the first <see cref="Process"/> object.</param>
		/// <param name="p2">Reference to the second <see cref="Process"/> object.</param>
		/// <returns>
		///    Returns an integer representing the comparison result, as
		///    in <see cref="IComparer.Compare(object, object)"/> method's specification.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when either one of the arguments is set to <code>null</code>.</exception>
		/// <exception cref="ArgumentException">
		///    Thrown when either one of the arguments is set to <code>null</code> or is not an
		///    instance of the <see cref="Process"/> class.
		/// </exception>
		public int Compare( object p1, object p2 )
		{
			// Arguments should not be null, and must be of type Process
			if ( p1 == null || p2 == null )
				throw new ArgumentNullException();

			if ( p1.GetType() != typeof( Process ) || p2.GetType() != typeof( Process ) )
				throw new ArgumentException();

			Process p1Ref = (Process) p1,
				p2Ref = (Process) p2;

			// Fist, try to compare processes by name
			string p1Name = p1Ref.ProcessName.Trim().ToLowerInvariant(),
				p2Name = p2Ref.ProcessName.Trim().ToLowerInvariant();
			int result = p1Name.CompareTo( p2Name );

			if ( result != 0 )
				return result;

			// If the names do not match, compare by PID
			return p1Ref.Id.CompareTo( p2Ref.Id );
		}
		#endregion
	}
}
