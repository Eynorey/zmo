﻿

#pragma checksum "C:\Users\Adrian\SkyDrive\dokumente\visual studio 2013\Projects\ZMO\WP Pivot\PivotPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6E8C04FF27CC72FB58CC37C2C5D6E724"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WP_Pivot
{
    partial class PivotPage : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 142 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.faecherUebersicht_SelectionChanged;
                 #line default
                 #line hidden
                #line 143 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.SecondPivot_Loaded;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 184 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.ListViewRectangle_Tapped;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 35 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.FrameworkElement)(target)).Loaded += this.SecondPivot_Loaded;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 118 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.zensurNeuFach_SelectionChanged;
                 #line default
                 #line hidden
                break;
            case 5:
                #line 119 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.zensurNeuArt_SelectionChanged;
                 #line default
                 #line hidden
                break;
            case 6:
                #line 126 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.zensurNeuEintragen_Click;
                 #line default
                 #line hidden
                break;
            case 7:
                #line 48 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.Grid_Tapped;
                 #line default
                 #line hidden
                break;
            case 8:
                #line 46 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).Tapped += this.semesterCaption_Tapped;
                 #line default
                 #line hidden
                break;
            case 9:
                #line 195 "..\..\PivotPage.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.AddAppBarButton_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}


