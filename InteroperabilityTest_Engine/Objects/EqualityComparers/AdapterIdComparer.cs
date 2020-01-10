/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections.Generic;
using BH.oM.Base;

namespace BH.Engine.Test.Interoperability
{
    public class AdapterIdComparer : IEqualityComparer<IBHoMObject>
    {
        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public AdapterIdComparer(string adapterId)
        {
            m_adapterId = adapterId;
        }

        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public bool Equals(IBHoMObject obj1, IBHoMObject obj2)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(obj1, obj2)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(obj1, null) || Object.ReferenceEquals(obj2, null))
                return false;

            //Check if the adapter id are the same
            object obj1Id;
            object obj2Id;

            //Check if ID exists
            if (!obj1.CustomData.TryGetValue(m_adapterId, out obj1Id))
                return false;

            if (!obj2.CustomData.TryGetValue(m_adapterId, out obj2Id))
                return false;

            return obj1Id.ToString() == obj2Id.ToString();
        }

        /***************************************************/

        public int GetHashCode(IBHoMObject obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Check if identifier exists
            object id;

            if (!obj.CustomData.TryGetValue(m_adapterId, out id))
                return 0;

            return id.ToString().GetHashCode();
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private string m_adapterId;

        /***************************************************/
    }
}
