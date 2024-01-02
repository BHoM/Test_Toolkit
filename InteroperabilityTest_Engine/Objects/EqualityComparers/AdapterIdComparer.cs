/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using System.Collections;
using System.Linq;
using BH.oM.Base;
using BH.Engine.Base;

namespace BH.Engine.Test.Interoperability
{
    public class AdapterIDComparer : IEqualityComparer<IBHoMObject>
    {
        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public AdapterIDComparer(Type adapterIdFragmentType)
        {
            m_adapterIDFragmentType = adapterIdFragmentType;
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

            //Check if the adapter ID are the same
            IFragment fragment1;
            IFragment fragment2;

            //Check if ID exists
            if (!obj1.Fragments.TryGetValue(m_adapterIDFragmentType, out fragment1))
                return false;

            if (!obj2.Fragments.TryGetValue(m_adapterIDFragmentType, out fragment2))
                return false;

            object id1 = ((IAdapterId)fragment1).Id;
            object id2 = ((IAdapterId)fragment2).Id;

            bool id1IsEnumerable = (!(id1 is string) && id1 is IEnumerable);
            bool id2IsEnumerable = (!(id2 is string) && id2 is IEnumerable);

            if (id1IsEnumerable || id2IsEnumerable)
            {
                IEnumerable<object> ids1;
                IEnumerable<object> ids2;

                if (id1IsEnumerable)
                    ids1 = (id1 as IEnumerable).Cast<object>();
                else
                    ids1 = new List<object> { id1 };

                if (id2IsEnumerable)
                    ids2 = (id1 as IEnumerable).Cast<object>();
                else
                    ids2 = new List<object> { id2 };

                return ids1.Zip(ids2, (x, y) => x.ToString() == y.ToString()).All(x => x);
            }


            return ((IAdapterId)fragment1).Id.ToString() == ((IAdapterId)fragment2).Id.ToString();
        }

        /***************************************************/

        public int GetHashCode(IBHoMObject obj)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(obj, null)) return 0;

            //Check if identifier exists
            IFragment fragment;

            //Check if ID exists
            if (!obj.Fragments.TryGetValue(m_adapterIDFragmentType, out fragment))
                return 0;

            return ((IAdapterId)fragment).Id.ToString().GetHashCode();
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private Type m_adapterIDFragmentType;

        /***************************************************/
    }
}





