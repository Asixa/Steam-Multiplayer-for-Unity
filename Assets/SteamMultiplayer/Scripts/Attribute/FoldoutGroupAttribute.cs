using System;
using UnityEngine;


    [AttributeUsage(AttributeTargets.Field)]
    public class LayoutAttribute : PropertyAttribute
    {
        public LayoutAttribute()
        {
         
        }

        public string title;
    }

