using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PiroCIE.Unity
{
    public class ForgeProperties : MonoBehaviour {
        [HideInInspector]
        public Dictionary<string, List<KeyValuePair<string, object>>> properties {get; set;}

        private  int _propertiesNumber; 
        public int PropertiesNumber {
            get 
            {
                _propertiesNumber = properties.Count;
                return _propertiesNumber;
            }
            set
            {
                _propertiesNumber = value;
            }
        }

    }
    
}

