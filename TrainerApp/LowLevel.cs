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

/* This file keeps definitions for code elements which are part of the low-level features of the trainer. */
using RAMvader.CodeInjection;
using System;

namespace TrainerApp
{
	/// <summary>Identifiers for all cheats available in the trainer.</summary>
	public enum ECheat
	{
        /// <summary>Identifier for the "God Mode" cheat.</summary>
        evCheatGodMode,
        /// <summary>Identifier for the "God Mode (Hacking Mode)" cheat.</summary>
        evCheatGodModeHackingMode,
        /// <summary>Identifier for the "Infinite Items" cheat.</summary>
        evCheatInfiniteItems,
        /// <summary>Identifier for the "Level Up" cheat.</summary>
        evCheatLevelUp,
        /// <summary>Identifier for the "One Hit Kills (Enemies)" cheat.</summary>
        evCheatOneHitKillsEnemies,
        /// <summary>Identifier for the "One Hit Kills (Animals)" cheat.</summary>
        evCheatOneHitKillsAnimals,
        /// <summary>Identifier for the "Easy Money" cheat.</summary>
        evCheatEasyMoney,
        /// <summary>Identifier for the "Instant Skill Recharge" cheat.</summary>
        evCheatInstantSkillRecharge,
        /// <summary>Identifier for the "Instant Hacking" cheat.</summary>
        evCheatInstantHacking,
    }





    /// <summary>Identifiers for all of the code caves injected into the game process' memory space,
    /// once the trainer gets attached to the game.</summary>
    public enum ECodeCave
	{
		/// <summary>Identifier for the "God Mode" code cave.</summary>
		evCodeCaveInfiniteItems,
        /// <summary>Identifier for the "Level Up" code cave.</summary>
        evCodeCaveLevelUp,
        /// <summary>Identifier for the "One Hit Kills (Enemies)" code cave.</summary>
        evCodeCaveOneHitKillsEnemies,
        /// <summary>Identifier for the "One Hit Kills (Animals)" code cave.</summary>
        evCodeCaveOneHitKillsAnimals,
        /// <summary>Identifier for the "Easy Money" code cave.</summary>
        evCodeCaveEasyMoney,
        /// <summary>Identifier for the "Instant Skill Recharge" code cave.</summary>
        evCodeCaveInstantSkillRecharge,
        /// <summary>Identifier for the "Instant Hacking" code cave.</summary>
        evCodeCaveInstantHacking,
    }





    /// <summary>Identifiers for all variables injected into the game process' memory space,
    /// once the trainer gets attached to the game.</summary>
    public enum EVariable
	{
	}
}
