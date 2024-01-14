﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace StageLightManeuver
{
    [CustomEditor(typeof(StageLight))]
    [CanEditMultipleObjects]
    public class MovingStageLightEditor:Editor
    {
        private StageLight targetStageLight;
        private List<string> channelList = new List<string>();
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            // return base.CreateInspectorGUI();
            
            targetStageLight = target as StageLight;
            var indexField = new PropertyField(serializedObject.FindProperty("index"));
            indexField.SetEnabled(false); 
            root.Add(indexField);
            root.Add(new PropertyField(serializedObject.FindProperty("stageLightChannels")));
            channelList = new List<string>();
            channelList.Add("Add New Channel");
          
           

            Init();

            var center = new VisualElement();
            center.style.alignItems = Align.Center;
            var popupField = new PopupField<string>(channelList, 0);
            popupField.SetEnabled( channelList.Count > 1 );
            popupField.RegisterValueChangedCallback((evt =>
            {
                if (popupField.index != 0)
                {
                    var type = GetTypeByClassName(popupField.value);
                    MethodInfo mi = typeof(GameObject).GetMethod(
                        "AddComponent",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null,
                        new Type[0],
                        null
                    );
                    MethodInfo bound = mi.MakeGenericMethod(type);
                    var channel =bound.Invoke(targetStageLight.gameObject, null) as StageLightChannelBase;
                    if(channel)channel.Init();
                    targetStageLight.FindChannels();
                }
            }));
            center.Add(popupField);
            root.Add(center);
            root.Add(new PropertyField(serializedObject.FindProperty("syncStageLight")));

            return root;
        }

        private void Init()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = executingAssembly.GetReferencedAssemblies();
            if(targetStageLight == null) return;
            foreach ( var assemblyName in referencedAssemblies )
            {
                var assembly = Assembly.Load( assemblyName );

                if ( assembly == null )
                {
                    continue;
                }
                var types = assembly.GetTypes();
                types.Where(t => t.IsSubclassOf(typeof(StageLightChannelBase)))
                    .ToList()
                    .ForEach(t =>
                    {
                        if (targetStageLight.StageLightChannels != null && targetStageLight.StageLightChannels.Count>=0)
                        {
                            if (targetStageLight.StageLightChannels.Find(x =>x!= null && x.GetType().Name == t.Name) == null)
                            {
                                // Debug.Log(t.Name);
                                channelList.Add(t.Name);
                            }      
                        }
                    });
            }
        }
        public static Type GetTypeByClassName( string className )
        {
            foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() ) {
                foreach( Type type in assembly.GetTypes() ) {
                    if( type.Name == className ) {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}