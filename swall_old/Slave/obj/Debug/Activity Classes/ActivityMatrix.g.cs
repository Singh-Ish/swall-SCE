﻿#pragma checksum "..\..\..\Activity Classes\ActivityMatrix.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5A57203AEB28C169D1128ECC949E72EAC4077B537E8BA170F1095A7EDEEF1547"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Prototype1;
using Prototype1.Activity_Classes;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Prototype1 {
    
    
    /// <summary>
    /// ActivityMatrix
    /// </summary>
    public partial class ActivityMatrix : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 27 "..\..\..\Activity Classes\ActivityMatrix.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer VerticalScrollViewer;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\Activity Classes\ActivityMatrix.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl Activities;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/childSlave;component/activity%20classes/activitymatrix.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Activity Classes\ActivityMatrix.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 9 "..\..\..\Activity Classes\ActivityMatrix.xaml"
            ((Prototype1.ActivityMatrix)(target)).PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.VerticalScrollViewer_MouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 16 "..\..\..\Activity Classes\ActivityMatrix.xaml"
            ((System.Windows.Data.CollectionViewSource)(target)).Filter += new System.Windows.Data.FilterEventHandler(this.CollectionViewSource_Filter);
            
            #line default
            #line hidden
            return;
            case 3:
            this.VerticalScrollViewer = ((System.Windows.Controls.ScrollViewer)(target));
            
            #line 27 "..\..\..\Activity Classes\ActivityMatrix.xaml"
            this.VerticalScrollViewer.ManipulationBoundaryFeedback += new System.EventHandler<System.Windows.Input.ManipulationBoundaryFeedbackEventArgs>(this.VerticalScrollViewer_ManipulationBoundaryFeedback);
            
            #line default
            #line hidden
            
            #line 27 "..\..\..\Activity Classes\ActivityMatrix.xaml"
            this.VerticalScrollViewer.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.VerticalScrollViewer_MouseDown);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Activities = ((System.Windows.Controls.ItemsControl)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

